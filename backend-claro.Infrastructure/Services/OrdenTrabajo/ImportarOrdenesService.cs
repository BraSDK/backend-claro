using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Infrastructure.Services;

// Carga masiva de SOTs desde el Excel "procesado".
// Columnas esperadas (por nombre de cabecera, el orden no importa):
//   FECHA | APELLIDOS | SOT | TIPO DE SERVICIO | COSTO | ADICIONAL | 150 | 220 | 300 | P TOTAL
// - COSTO ya incluye el ADICIONAL, por eso el precio del servicio principal es COSTO - ADICIONAL.
// - P TOTAL = COSTO + drops; no se lee, se recalcula sumando los detalles.
// - El archivo no se guarda: solo se lee la información.
public class ImportarOrdenesService : IImportarOrdenesService
{
    private const string SERVICIO_ADICIONAL = "ADICIONAL";

    // columna del Excel -> (tipo de detalle, nombre del servicio genérico)
    private static readonly (string Columna, TipoDetalle Tipo, string Servicio)[] Drops =
    {
        ("150", TipoDetalle.DROP150, "DROP 150"),
        ("220", TipoDetalle.DROP200, "DROP 220"),   // el enum se llama DROP200, en el Excel la columna es 220
        ("300", TipoDetalle.DROP300, "DROP 300"),
    };

    private readonly IApplicationDbContext _context;

    public ImportarOrdenesService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ImportarExcelResponse> ImportarExcelAsync(Stream archivo)
    {
        var respuesta = new ImportarExcelResponse();

        using var libro = new XLWorkbook(archivo);
        var hoja = libro.Worksheet(1);
        var rango = hoja.RangeUsed() ?? throw new InvalidOperationException("El Excel está vacío");

        // 1. Ubicar las columnas por su cabecera
        var cabecera = rango.FirstRow();
        var columnas = new Dictionary<string, int>();
        foreach (var celda in cabecera.CellsUsed())
            columnas[Normalizar(Texto(celda))] = celda.Address.ColumnNumber;

        int Col(string nombre) => columnas.TryGetValue(Normalizar(nombre), out var n)
            ? n
            : throw new InvalidOperationException($"Falta la columna '{nombre}' en el Excel");

        int colFecha = Col("FECHA"), colTecnico = Col("APELLIDOS"), colSot = Col("SOT"),
            colServicio = Col("TIPO DE SERVICIO"), colCosto = Col("COSTO"), colAdicional = Col("ADICIONAL");
        var colDrops = Drops.Select(d => (d.Tipo, d.Servicio, Columna: Col(d.Columna))).ToList();

        // 2. Datos de la BD que se usan para buscar (en memoria, para no consultar fila por fila)
        var servicios = await _context.Servicios.ToListAsync();
        var serviciosPorNombre = servicios
            .GroupBy(s => Normalizar(s.Nombre))
            .ToDictionary(g => g.Key, g => g.First());
        int siguienteCodigo = servicios.Count == 0 ? 1 : servicios.Max(s => s.Codigo) + 1;

        var usuariosPorNombre = (await _context.Usuarios.ToListAsync())
            .GroupBy(u => Normalizar(u.NombreCompleto))
            .ToDictionary(g => g.Key, g => g.First());

        // Busca el servicio por nombre; si no existe lo crea con el precio de esta fila
        Servicio ObtenerServicio(string nombre, decimal precio, Servicio.CategoriaServicio categoria)
        {
            var clave = Normalizar(nombre);
            if (serviciosPorNombre.TryGetValue(clave, out var existente)) return existente;

            var nuevo = new Servicio
            {
                Codigo = siguienteCodigo++,
                Nombre = nombre.Trim(),
                Precio = precio,
                Categoria = categoria,
            };
            _context.Servicios.Add(nuevo);
            serviciosPorNombre[clave] = nuevo;
            respuesta.ServiciosCreados.Add(nuevo.Nombre);
            return nuevo;
        }

        // 3. Leer las filas
        var filas = rango.RowsUsed().Skip(1).ToList();

        var sotsDelExcel = filas
            .Select(f => LeerEntero(f.Cell(colSot)))
            .Where(s => s.HasValue)
            .Select(s => s!.Value)
            .Distinct()
            .ToList();

        var ordenesExistentes = await _context.Ordenes
            .Include(o => o.Detalles)
            .Where(o => sotsDelExcel.Contains(o.Sot))
            .ToDictionaryAsync(o => o.Sot);

        var sotsProcesadas = new Dictionary<int, int>();   // sot -> fila donde apareció

        foreach (var fila in filas)
        {
            int numFila = fila.RowNumber();
            var sot = LeerEntero(fila.Cell(colSot));

            // fila sin SOT: es la fila de totales o una fila vacía, se ignora
            if (sot is null)
            {
                if (!fila.Cell(colSot).IsEmpty())
                    respuesta.Errores.Add(new ErrorFilaImportacion { Fila = numFila, Mensaje = $"SOT inválida: '{Texto(fila.Cell(colSot))}'" });
                continue;
            }

            void Error(string mensaje) =>
                respuesta.Errores.Add(new ErrorFilaImportacion { Fila = numFila, Sot = sot, Mensaje = mensaje });

            if (sotsProcesadas.TryGetValue(sot.Value, out var filaAnterior))
            {
                Error($"SOT repetida en el Excel (ya está en la fila {filaAnterior})");
                continue;
            }

            var nombreServicio = Texto(fila.Cell(colServicio)).Trim();
            if (nombreServicio == "") { Error("Falta el TIPO DE SERVICIO"); continue; }

            var costo = LeerMonto(fila.Cell(colCosto));
            if (costo is null) { Error("Falta el COSTO o no es un número"); continue; }

            var nombreTecnico = Texto(fila.Cell(colTecnico)).Trim();
            if (!usuariosPorNombre.TryGetValue(Normalizar(nombreTecnico), out var tecnico))
            {
                Error($"No existe un usuario con el nombre '{nombreTecnico}'");
                continue;
            }

            var adicional = LeerMonto(fila.Cell(colAdicional)) ?? 0;
            var precioBase = costo.Value - adicional;
            if (precioBase < 0) { Error("El ADICIONAL es mayor que el COSTO"); continue; }

            var categoria = Normalizar(nombreServicio).StartsWith("HFC")
                ? Servicio.CategoriaServicio.HFC
                : Servicio.CategoriaServicio.FTH;

            // 4. Armar los detalles: servicio principal + adicional + drops, con los montos del Excel
            var detalles = new List<DetalleTrabajo>();

            void Agregar(Servicio servicio, TipoDetalle tipo, decimal monto) => detalles.Add(new DetalleTrabajo
            {
                ServicioCodigo = servicio.Codigo,
                Servicio = servicio,
                Cantidad = 1,
                Tipo = tipo,
                PrecioTotal = monto,
            });

            Agregar(ObtenerServicio(nombreServicio, precioBase, categoria), TipoDetalle.SERVICIO, precioBase);

            if (adicional > 0)
                Agregar(ObtenerServicio(SERVICIO_ADICIONAL, adicional, categoria), TipoDetalle.DETALLE, adicional);

            foreach (var drop in colDrops)
            {
                var monto = LeerMonto(fila.Cell(drop.Columna)) ?? 0;
                if (monto > 0)
                    Agregar(ObtenerServicio(drop.Servicio, monto, categoria), drop.Tipo, monto);
            }

            // 5. Crear la SOT o reemplazar sus detalles si ya existe
            if (ordenesExistentes.TryGetValue(sot.Value, out var orden))
            {
                _context.Detalles.RemoveRange(orden.Detalles);
                orden.Detalles.Clear();
                foreach (var d in detalles) orden.Detalles.Add(d);
                orden.PrecioTotal = detalles.Sum(d => d.PrecioTotal);
                orden.FechaActualizacion = DateTime.UtcNow;
                respuesta.OrdenesActualizadas++;
            }
            else
            {
                var fecha = LeerFecha(fila.Cell(colFecha));
                _context.Ordenes.Add(new OrdenTrabajo
                {
                    Sot = sot.Value,
                    UsuarioId = tecnico.Id,
                    Estado = Estados.INGRESADA,
                    Detalles = detalles,
                    PrecioTotal = detalles.Sum(d => d.PrecioTotal),
                    FechaCreacion = fecha ?? DateTime.UtcNow,
                    FechaActualizacion = DateTime.UtcNow,
                });
                respuesta.OrdenesCreadas++;
            }

            sotsProcesadas[sot.Value] = numFila;
        }

        // 6. Un solo SaveChanges: o se guarda todo lo válido, o nada si la BD falla
        await _context.SaveChangesAsync();
        return respuesta;
    }

    // ============ Lectura de celdas ============

    private static string Texto(IXLCell celda)
    {
        var v = celda.Value;
        if (v.IsBlank) return "";
        if (v.IsNumber) return v.GetNumber().ToString(CultureInfo.InvariantCulture);
        return v.ToString();
    }

    private static int? LeerEntero(IXLCell celda)
    {
        var v = celda.Value;
        if (v.IsNumber) return (int)v.GetNumber();
        if (v.IsText && int.TryParse(v.GetText().Trim(), out var n)) return n;
        return null;
    }

    // Acepta números (197, 20.02) y textos tipo "S/.40,70" o "S/ 1.234,50"
    private static decimal? LeerMonto(IXLCell celda)
    {
        var v = celda.Value;
        if (v.IsBlank) return null;
        if (v.IsNumber) return Math.Round((decimal)v.GetNumber(), 2);
        if (!v.IsText) return null;

        var limpio = new string(v.GetText().Where(c => char.IsDigit(c) || c == ',' || c == '.' || c == '-').ToArray());
        if (limpio == "") return null;

        if (limpio.Contains(',') && limpio.Contains('.'))
            limpio = limpio.Replace(".", "").Replace(',', '.');   // 1.234,50 -> 1234.50
        else
            limpio = limpio.Replace(',', '.');                    // 40,70 -> 40.70

        return decimal.TryParse(limpio, NumberStyles.Number, CultureInfo.InvariantCulture, out var monto)
            ? Math.Round(monto, 2)
            : null;
    }

    // Postgres (timestamptz) exige DateTime en UTC
    private static DateTime? LeerFecha(IXLCell celda)
    {
        var v = celda.Value;
        if (v.IsDateTime) return DateTime.SpecifyKind(v.GetDateTime().Date, DateTimeKind.Utc);
        if (v.IsText && DateTime.TryParse(v.GetText(), CultureInfo.GetCultureInfo("es-PE"), DateTimeStyles.None, out var f))
            return DateTime.SpecifyKind(f.Date, DateTimeKind.Utc);
        return null;
    }

    // Mayúsculas, sin tildes y con un solo espacio: "Saldaña  Juan" -> "SALDANA JUAN"
    private static string Normalizar(string texto)
    {
        var sinTildes = new StringBuilder();
        foreach (var c in texto.Trim().ToUpperInvariant().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                sinTildes.Append(c);
        return string.Join(' ', sinTildes.ToString().Split(' ', StringSplitOptions.RemoveEmptyEntries));
    }
}
