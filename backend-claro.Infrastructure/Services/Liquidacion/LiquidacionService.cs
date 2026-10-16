using backend_claro.Application.DTOs.Liquidacion;
using backend_claro.Application.Interfaces;
using backend_claro.Application.Mappings;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
// Nota: Aquí asegúrate de tener instalada una librería para leer Excel, por ejemplo ClosedXML o EPPlus.
// Usaremos ClosedXML como referencia común en .NET, o puedes adaptarlo a la que ya uses.
using ClosedXML.Excel; 

namespace backend_claro.Infrastructure.Services;

public class LiquidacionService : ILiquidacionService
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<LiquidacionService> _logger;

    public LiquidacionService(IApplicationDbContext context, ILogger<LiquidacionService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<ImportarExcelResponse> ImportarExcelAsync(Stream archivoStream, int usuarioId)
    {
        var response = new ImportarExcelResponse();
        int numeroFila = 1; // Suponiendo que la fila 1 es la cabecera

        // 1. Validar que el usuario exista
        var usuario = await _context.Usuarios.FindAsync(usuarioId);
        if (usuario == null)
        {
            throw new KeyNotFoundException("El usuario autenticado no existe en el sistema para realizar la importación.");
        }

        try
        {
            using var workbook = new XLWorkbook(archivoStream);
            var worksheet = workbook.Worksheet(1); // Tomamos la primera hoja del Excel
            var rows = worksheet.RowsUsed().Skip(1); // Omitimos la cabecera

            foreach (var row in rows)
            {
                numeroFila++;
                response.TotalFilasProcesadas++;

                try
                {
                    // Ejemplo de lectura de celdas (ajusta los índices según la estructura de tu Excel)
                    // Columna A (1): SOT, Columna B (2): TipoOrden, etc.
                    long sot = row.Cell(1).GetValue<long>();
                    string tipoOrden = row.Cell(2).GetValue<string>();
                    string nombreSubTipoOrden = row.Cell(3).GetValue<string>();
                    string nombreTecnico = row.Cell(4).GetValue<string>();
                    decimal precioEditable = row.Cell(5).GetValue<decimal>();

                    // Validar si la SOT ya existe o reglas de negocio particulares
                    var liquidacionExistente = await _context.Liquidaciones
                        .FirstOrDefaultAsync(l => l.Sot == sot);

                    if (liquidacionExistente != null)
                    {
                        // Opcional: Actualizar o registrar como error de duplicidad
                        response.TotalConErrores++;
                        response.Errores.Add(new ErrorFilaImportacionDto
                        {
                            Fila = numeroFila,
                            Sot = sot,
                            Mensaje = $"La SOT {sot} ya se encuentra liquidada en el sistema."
                        });
                        continue;
                    }

                    // Crear la entidad Liquidación
                    var nuevaLiquidacion = new Liquidacion
                    {
                        Sot = sot,
                        TipoOrden = tipoOrden,
                        NombreSubTipoOrden = nombreSubTipoOrden,
                        NombreTecnico = nombreTecnico,
                        PrecioTotalEditable = precioEditable,
                        EstadoSistema = Estados.INGRESADA,
                        UsuarioId = usuarioId,
                        EstadoPago = EstadoPago.Pendiente,
                        FechaCreacion = DateTime.UtcNow,
                        FechaActualizacion = DateTime.UtcNow
                    };

                    // Aquí puedes agregar la lógica para leer y añadir los 'Detalles' de la liquidación
                    // si vienen en columnas adicionales o en otra pestaña.

                    _context.Liquidaciones.Add(nuevaLiquidacion);
                    response.TotalExitosas++;
                }
                catch (Exception ex)
                {
                    response.TotalConErrores++;
                    response.Errores.Add(new ErrorFilaImportacionDto
                    {
                        Fila = numeroFila,
                        Sot = null,
                        Mensaje = $"Error al procesar la fila: {ex.Message}"
                    });
                }
            }

            // Guardar cambios masivos en la base de datos
            await _context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error crítico al leer el archivo Excel de liquidaciones.");
            throw new InvalidOperationException("El formato del archivo Excel no es válido o está dañado.");
        }

        return response;
    }

    public async Task<IEnumerable<LiquidacionResponse>> ObtenerLiquidacionAsync()
    {
        var liquidaciones = await _context.Liquidaciones
            .Include(l => l.Detalles)
            .Include(l => l.Observaciones)
            .ToListAsync();

        return liquidaciones.Select(l => l.ToResponse());
    }

    public async Task<LiquidacionResponse?> ObtenerPorIdAsync(int id)
    {
        var liquidacion = await _context.Liquidaciones
            .Include(l => l.Detalles)
            .Include(l => l.Observaciones)
            .FirstOrDefaultAsync(l => l.LiquidacionId == id); // Asegurar coincidencia de propiedad

        return liquidacion?.ToResponse();
    }
}