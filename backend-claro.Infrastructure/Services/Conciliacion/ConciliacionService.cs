using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using backend_claro.Application.DTOs.Conciliacion;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Infrastructure.Services;

// Conciliación de pagos: valorizado por almacén (P. Total) vs lo que se paga tras la auditoría.
// Cuando exista el reporte de Claro, "Pagado" saldrá de ese reporte en lugar de la auditoría.
public class ConciliacionService : IConciliacionService
{
    private readonly IApplicationDbContext _context;

    public ConciliacionService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<ConciliacionResponse> ConciliarAsync(ConciliacionRequest request)
    {
        var (anio, mes) = Periodo(request.Anio, request.Mes);
        var delPeriodo = OrdenesDelPeriodo(anio, mes);
        var auditadas = delPeriodo.Where(o => o.FechaAuditoria != null);

        var respuesta = new ConciliacionResponse
        {
            Anio = anio,
            Mes = mes,
            // 1. KPIs del periodo completo
            TotalSots = await delPeriodo.CountAsync(),
            TotalValorizado = await delPeriodo.SumAsync(o => o.PrecioTotal ?? 0),
            TotalPagado = await auditadas.SumAsync(o => o.SinPago ? 0 : (o.PrecioTotal ?? 0) - o.Descuento),
            DiferenciaTotal = -await auditadas.SumAsync(o => o.SinPago ? (o.PrecioTotal ?? 0) : o.Descuento),
            SotEvaluadas = await auditadas.CountAsync(),
            UltimaAuditoria = await auditadas.MaxAsync(o => (DateTime?)o.FechaAuditoria),
            Pagina = Math.Max(1, request.Pagina),
            CanPagina = Math.Clamp(request.CanPagina, 1, 100),
        };

        var totales = await delPeriodo
            .GroupBy(o => o.EstadoPago)
            .Select(g => new TotalEstadoPago { EstadoPago = g.Key, Cantidad = g.Count(), Monto = g.Sum(o => o.PrecioTotal ?? 0) })
            .ToListAsync();
        // siempre los tres estados, aunque alguno no tenga SOT
        respuesta.TotalesPorEstadoPago = Enum.GetValues<EstadoPago>()
            .Select(e => totales.FirstOrDefault(t => t.EstadoPago == e) ?? new TotalEstadoPago { EstadoPago = e })
            .ToList();

        // 2. Filtros de la tabla (todos menos la categoría, que se aplica después para poder contar cada una)
        var filtradas = AplicarFiltros(delPeriodo, request);

        respuesta.Cuadra = await PorCategoria(filtradas, CategoriaConciliacion.CUADRA).CountAsync();
        respuesta.Diferencia = await PorCategoria(filtradas, CategoriaConciliacion.DIFERENCIA).CountAsync();
        respuesta.NoPagada = await PorCategoria(filtradas, CategoriaConciliacion.NO_PAGADA).CountAsync();
        respuesta.PorAuditar = await PorCategoria(filtradas, CategoriaConciliacion.POR_AUDITAR).CountAsync();

        if (request.Categoria.HasValue)
            filtradas = PorCategoria(filtradas, request.Categoria.Value);

        // 3. Página de la tabla
        respuesta.TotalFilas = await filtradas.CountAsync();
        respuesta.Filas = await Proyectar(filtradas
                .OrderByDescending(o => o.FechaCreacion)
                .ThenBy(o => o.Sot)
                .Skip((respuesta.Pagina - 1) * respuesta.CanPagina)
                .Take(respuesta.CanPagina))
            .ToListAsync();

        return respuesta;
    }

    public async Task<int> MarcarApelacionAsync(MarcarApelacionRequest request)
    {
        if (request.OrdenIds.Count == 0) return 0;

        var ordenes = await _context.Ordenes
            .Where(o => request.OrdenIds.Contains(o.OrdenTrabajoId))
            .ToListAsync();

        foreach (var orden in ordenes)
        {
            orden.MarcadaApelacion = request.Marcar;
            orden.FechaApelacion = request.Marcar ? DateTime.UtcNow : null;
        }

        await _context.SaveChangesAsync();
        return ordenes.Count;
    }

    public async Task<byte[]> ExportarDiferenciasAsync(int anio, int mes)
    {
        // todo lo que no cuadra: diferencia, no pagada y por auditar
        var filas = await Proyectar(OrdenesDelPeriodo(anio, mes)
                .Where(o => !(o.FechaAuditoria != null && !o.SinPago && o.Descuento == 0))
                .OrderBy(o => o.FechaCreacion)
                .ThenBy(o => o.Sot))
            .ToListAsync();

        using var libro = new XLWorkbook();
        var hoja = libro.Worksheets.Add("Diferencias");
        string[] cabecera = { "SOT", "FECHA", "TÉCNICO", "TIPO DE SERVICIO", "P. TOTAL ALMACÉN", "PAGADO", "DIFERENCIA", "CATEGORÍA", "IMÁGENES", "APELACIÓN", "MOTIVO" };
        for (int c = 0; c < cabecera.Length; c++) hoja.Cell(1, c + 1).Value = cabecera[c];

        var fila = 2;
        foreach (var f in filas)
        {
            hoja.Cell(fila, 1).Value = f.Sot;
            hoja.Cell(fila, 2).Value = f.Fecha.Date;
            hoja.Cell(fila, 3).Value = f.Tecnico;
            hoja.Cell(fila, 4).Value = f.TipoServicio;
            hoja.Cell(fila, 5).Value = f.Valorizado;
            if (f.Pagado.HasValue) hoja.Cell(fila, 6).Value = f.Pagado.Value;
            if (f.Diferencia.HasValue) hoja.Cell(fila, 7).Value = f.Diferencia.Value;
            hoja.Cell(fila, 8).Value = NombreCategoria(f.Categoria);
            hoja.Cell(fila, 9).Value = f.CantidadImagenes;
            hoja.Cell(fila, 10).Value = f.MarcadaApelacion ? "SÍ" : "NO";
            hoja.Cell(fila, 11).Value = f.Motivo ?? "";
            fila++;
        }

        var encabezado = hoja.Range(1, 1, 1, cabecera.Length);
        encabezado.Style.Font.Bold = true;
        encabezado.Style.Fill.BackgroundColor = XLColor.FromHtml("#E2E8F0");
        hoja.Column(2).Style.DateFormat.Format = "dd/mm/yyyy";
        hoja.Range(2, 5, Math.Max(2, fila - 1), 7).Style.NumberFormat.Format = "\"S/\" #,##0.00";
        hoja.SheetView.FreezeRows(1);
        hoja.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        libro.SaveAs(stream);
        return stream.ToArray();
    }

    // ============ Consultas reutilizables ============

    private static (int anio, int mes) Periodo(int? anio, int? mes)
    {
        var hoy = DateTime.UtcNow;
        var m = mes is >= 1 and <= 12 ? mes.Value : hoy.Month;
        return (anio ?? hoy.Year, m);
    }

    private IQueryable<OrdenTrabajo> OrdenesDelPeriodo(int anio, int mes)
    {
        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin = inicio.AddMonths(1);
        return _context.Ordenes.AsNoTracking().Where(o => o.FechaCreacion >= inicio && o.FechaCreacion < fin);
    }

    private static IQueryable<OrdenTrabajo> AplicarFiltros(IQueryable<OrdenTrabajo> query, ConciliacionRequest r)
    {
        if (!string.IsNullOrWhiteSpace(r.Buscar))
        {
            var texto = r.Buscar.Trim();
            query = query.Where(o => o.Sot.ToString().Contains(texto));
        }
        if (r.TecnicoId.HasValue)
            query = query.Where(o => o.UsuarioId == r.TecnicoId.Value);
        if (r.ConEvidencia.HasValue)
            query = r.ConEvidencia.Value ? query.Where(o => o.Archivos.Any()) : query.Where(o => !o.Archivos.Any());
        if (r.CategoriaServicio.HasValue)
        {
            var categoria = (Servicio.CategoriaServicio)r.CategoriaServicio.Value;
            query = query.Where(o => o.Detalles.Any(d => d.Tipo == TipoDetalle.SERVICIO && d.Servicio.Categoria == categoria));
        }
        if (r.Apelacion.HasValue)
            query = query.Where(o => o.MarcadaApelacion == r.Apelacion.Value);
        if (r.EstadoPago.HasValue)
            query = query.Where(o => o.EstadoPago == r.EstadoPago.Value);
        return query;
    }

    // las mismas reglas que Categoria() pero traducibles a SQL
    private static IQueryable<OrdenTrabajo> PorCategoria(IQueryable<OrdenTrabajo> query, CategoriaConciliacion categoria) => categoria switch
    {
        CategoriaConciliacion.POR_AUDITAR => query.Where(o => o.FechaAuditoria == null),
        CategoriaConciliacion.NO_PAGADA => query.Where(o => o.FechaAuditoria != null && o.SinPago),
        CategoriaConciliacion.DIFERENCIA => query.Where(o => o.FechaAuditoria != null && !o.SinPago && o.Descuento > 0),
        _ => query.Where(o => o.FechaAuditoria != null && !o.SinPago && o.Descuento == 0),
    };

    private static IQueryable<FilaConciliacion> Proyectar(IQueryable<OrdenTrabajo> query) => query.Select(o => new FilaConciliacion
    {
        OrdenId = o.OrdenTrabajoId,
        Sot = o.Sot,
        Fecha = o.FechaCreacion,
        UsuarioId = o.UsuarioId,
        Tecnico = o.Usuario.NombreCompleto,
        TipoServicio = o.Detalles
            .Where(d => d.Tipo == TipoDetalle.SERVICIO)
            .Select(d => d.Servicio.Nombre)
            .FirstOrDefault() ?? "",
        Valorizado = o.PrecioTotal ?? 0,
        Pagado = o.FechaAuditoria == null ? null : (o.SinPago ? 0 : (o.PrecioTotal ?? 0) - o.Descuento),
        Diferencia = o.FechaAuditoria == null ? null : (o.SinPago ? -(o.PrecioTotal ?? 0) : -o.Descuento),
        Categoria = o.FechaAuditoria == null ? CategoriaConciliacion.POR_AUDITAR
            : o.SinPago ? CategoriaConciliacion.NO_PAGADA
            : o.Descuento > 0 ? CategoriaConciliacion.DIFERENCIA
            : CategoriaConciliacion.CUADRA,
        CantidadImagenes = o.Archivos.Count,
        MarcadaApelacion = o.MarcadaApelacion,
        Motivo = o.ObservacionAuditoria,
        EstadoPago = o.EstadoPago,
        MotivoNoPago = o.MotivoNoPago,
    });

    private static string NombreCategoria(CategoriaConciliacion c) => c switch
    {
        CategoriaConciliacion.CUADRA => "Cuadra",
        CategoriaConciliacion.DIFERENCIA => "Diferencia",
        CategoriaConciliacion.NO_PAGADA => "No pagada",
        _ => "Por auditar",
    };
}
