using backend_claro.Application.DTOs.Dashboard;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const int CantidadUltimas = 8;
    private const int CantidadServicios = 8;

    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    // Admin y almacén: producción del mes según las liquidaciones (por fecha de programación).
    // Técnico: solo sus órdenes del mes (por fecha de registro), sin montos.
    public async Task<DashboardResumenDto> ResumenAsync(int anio, int mes, Rol rolUsuario, int cuentaId)
    {
        var desde = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var hasta = desde.AddMonths(1);
        var desdeAnterior = desde.AddMonths(-1);

        return rolUsuario == Rol.TECNICO
            ? await ResumenTecnicoAsync(anio, mes, cuentaId, desde, hasta, desdeAnterior)
            : await ResumenGeneralAsync(anio, mes, desde, hasta, desdeAnterior);
    }

    public async Task<List<TecnicoOpcionDto>> TecnicosAsync()
    {
        return await _context.Usuarios.AsNoTracking()
            .Where(u => u.Cuenta.Rol == Rol.TECNICO)
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new TecnicoOpcionDto { UsuarioId = u.Id, Nombre = u.NombreCompleto })
            .ToListAsync();
    }

    private async Task<DashboardResumenDto> ResumenGeneralAsync(int anio, int mes, DateTime desde, DateTime hasta, DateTime desdeAnterior)
    {
        var liquidaciones = await _context.Liquidaciones.AsNoTracking()
            .Where(l => l.FechaProgramacion >= desde && l.FechaProgramacion < hasta)
            .Select(l => new
            {
                l.NombreTecnico,
                TecnicoId = l.OrdenTrabajo != null ? (int?)l.OrdenTrabajo.UsuarioId : null,
                l.EstadoSistema,
                l.EstadoPago,
                l.PrecioTotalReal,
                l.PrecioTotalEditable,
                l.MontoPagado,
                Auditada = l.FechaValidacion != null,
                TieneImagen = l.OrdenTrabajo != null && l.OrdenTrabajo.Archivos.Any(),
                Detalles = l.Detalles.Select(d => new { d.Tipo, d.Concepto, d.Cantidad, d.PrecioTotal }).ToList(),
            })
            .ToListAsync();

        var anterior = await _context.Liquidaciones.AsNoTracking()
            .Where(l => l.FechaProgramacion >= desdeAnterior && l.FechaProgramacion < desde)
            .GroupBy(_ => 1)
            .Select(g => new { Cantidad = g.Count(), Monto = g.Sum(l => l.PrecioTotalReal ?? 0) })
            .FirstOrDefaultAsync();

        // mismo criterio que Liquidacion.TotalACobrar (no se puede usar en la consulta porque no está mapeada)
        decimal TotalACobrar(decimal? editable, decimal? real) => editable ?? real ?? 0;

        var detalles = liquidaciones.SelectMany(l => l.Detalles).ToList();

        return new DashboardResumenDto
        {
            Anio = anio,
            Mes = mes,
            TotalSots = liquidaciones.Count,
            MontoTotal = liquidaciones.Sum(l => l.PrecioTotalReal ?? 0),
            // no pagó = 0; si ya se sabe cuánto pagó, eso; si no, lo que se espera cobrar
            MontoAPagar = liquidaciones.Sum(l => l.EstadoPago == EstadoPago.NoPagara ? 0
                : l.MontoPagado ?? TotalACobrar(l.PrecioTotalEditable, l.PrecioTotalReal)),
            SotsAuditadas = liquidaciones.Count(l => l.Auditada),
            SotsSinImagen = liquidaciones.Count(l => !l.TieneImagen),
            TotalSotsMesAnterior = anterior?.Cantidad ?? 0,
            MontoTotalMesAnterior = anterior?.Monto ?? 0,
            PorEstado = liquidaciones
                .GroupBy(l => l.EstadoSistema)
                .OrderBy(g => g.Key)
                .Select(g => new EstadoResumenDto
                {
                    Estado = g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(l => TotalACobrar(l.PrecioTotalEditable, l.PrecioTotalReal)),
                })
                .ToList(),
            // la liquidación guarda el técnico como texto; el id sale de su orden si está enlazada
            PorTecnico = liquidaciones
                .GroupBy(l => l.NombreTecnico.Trim())
                .Select((g, i) => new TecnicoResumenDto
                {
                    UsuarioId = g.Select(l => l.TecnicoId).FirstOrDefault(id => id != null) ?? -(i + 1),
                    Nombre = string.IsNullOrEmpty(g.Key) ? "Sin técnico" : g.Key,
                    Cantidad = g.Count(),
                    Monto = g.Sum(l => TotalACobrar(l.PrecioTotalEditable, l.PrecioTotalReal)),
                })
                .OrderByDescending(t => t.Monto)
                .ToList(),
            PorServicio = detalles
                .Where(d => d.Tipo == TipoDetalle.SERVICIO)
                .GroupBy(d => d.Concepto.Trim())
                .Select(g => new ServicioResumenDto { Nombre = g.Key, Cantidad = g.Count(), Monto = g.Sum(d => d.PrecioTotal) })
                .OrderByDescending(s => s.Cantidad)
                .Take(CantidadServicios)
                .ToList(),
            PorTipoDetalle = detalles
                .GroupBy(d => d.Tipo)
                .OrderBy(g => g.Key)
                .Select(g => new TipoDetalleResumenDto { Tipo = g.Key, Cantidad = g.Sum(d => d.Cantidad), Monto = g.Sum(d => d.PrecioTotal) })
                .ToList(),
            UltimasSots = await UltimasOrdenesAsync(desde, hasta, usuarioId: null),
        };
    }

    private async Task<DashboardResumenDto> ResumenTecnicoAsync(int anio, int mes, int cuentaId, DateTime desde, DateTime hasta, DateTime desdeAnterior)
    {
        var usuarioId = await _context.Usuarios
            .Where(u => u.AuthId == cuentaId)
            .Select(u => (int?)u.Id)
            .FirstOrDefaultAsync();

        var ordenes = await _context.Ordenes.AsNoTracking()
            .Where(o => o.UsuarioId == usuarioId && o.FechaCreacion >= desde && o.FechaCreacion < hasta)
            .Select(o => new { o.Sot, o.TrabajoRealizado })
            .ToListAsync();

        var totalAnterior = await _context.Ordenes
            .CountAsync(o => o.UsuarioId == usuarioId && o.FechaCreacion >= desdeAnterior && o.FechaCreacion < desde);

        var estados = await EstadosPorSotAsync(ordenes.Select(o => (long)o.Sot).ToList());

        return new DashboardResumenDto
        {
            Anio = anio,
            Mes = mes,
            TotalSots = ordenes.Count,
            TotalSotsMesAnterior = totalAnterior,
            // una orden sin liquidación todavía cuenta como INGRESADA
            PorEstado = ordenes
                .GroupBy(o => estados.TryGetValue(o.Sot, out var e) ? e.Estado : Estados.INGRESADA)
                .OrderBy(g => g.Key)
                .Select(g => new EstadoResumenDto { Estado = g.Key, Cantidad = g.Count() })
                .ToList(),
            PorServicio = ordenes
                .Where(o => !string.IsNullOrWhiteSpace(o.TrabajoRealizado))
                .GroupBy(o => o.TrabajoRealizado.Trim())
                .Select(g => new ServicioResumenDto { Nombre = g.Key, Cantidad = g.Count() })
                .OrderByDescending(s => s.Cantidad)
                .Take(CantidadServicios)
                .ToList(),
            UltimasSots = await UltimasOrdenesAsync(desde, hasta, usuarioId),
        };
    }

    // Últimas órdenes registradas en el mes, con el estado y precio de su liquidación si ya la tienen
    private async Task<List<UltimaSotDto>> UltimasOrdenesAsync(DateTime desde, DateTime hasta, int? usuarioId)
    {
        var query = _context.Ordenes.AsNoTracking()
            .Where(o => o.FechaCreacion >= desde && o.FechaCreacion < hasta);
        if (usuarioId.HasValue) query = query.Where(o => o.UsuarioId == usuarioId);

        var ultimas = await query
            .OrderByDescending(o => o.FechaCreacion)
            .Take(CantidadUltimas)
            .Select(o => new { o.OrdenTrabajoId, o.Sot, o.FechaCreacion, Nombre = o.Usuario.NombreCompleto })
            .ToListAsync();

        var estados = await EstadosPorSotAsync(ultimas.Select(o => (long)o.Sot).ToList());

        return ultimas.Select(o =>
        {
            var liquidacion = estados.GetValueOrDefault(o.Sot);
            return new UltimaSotDto
            {
                OrdenId = o.OrdenTrabajoId,
                Sot = o.Sot,
                Fecha = o.FechaCreacion,
                NombreUsuario = o.Nombre,
                Estado = liquidacion?.Estado ?? Estados.INGRESADA,
                PrecioTotal = liquidacion?.Precio,
            };
        }).ToList();
    }

    private record EstadoLiquidacion(Estados Estado, decimal? Precio);

    // SOT → estado y total a cobrar de su liquidación
    private async Task<Dictionary<long, EstadoLiquidacion>> EstadosPorSotAsync(List<long> sots)
    {
        var filas = await _context.Liquidaciones.AsNoTracking()
            .Where(l => sots.Contains(l.Sot))
            .Select(l => new { l.Sot, l.EstadoSistema, Precio = l.PrecioTotalEditable ?? l.PrecioTotalReal })
            .ToListAsync();

        return filas
            .GroupBy(l => l.Sot)
            .ToDictionary(g => g.Key, g => new EstadoLiquidacion(g.First().EstadoSistema, g.First().Precio));
    }
}
