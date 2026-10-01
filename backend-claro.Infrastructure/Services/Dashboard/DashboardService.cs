using Microsoft.EntityFrameworkCore;
using backend_claro.Application.DTOs.Dashboard;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Infrastructure.Services;

public class DashboardService : IDashboardService
{
    private const int TOP_SERVICIOS = 10;
    private const int ULTIMAS_SOTS = 6;

    private readonly IApplicationDbContext _context;

    public DashboardService(IApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> ResumenMensualAsync(int anio, int mes, Rol rolUsuario, int cuentaId)
    {
        var inicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fin = inicio.AddMonths(1);
        var inicioAnterior = inicio.AddMonths(-1);

        // 1. Órdenes visibles para el usuario
        IQueryable<OrdenTrabajo> ordenes = _context.Ordenes.AsNoTracking();
        if (rolUsuario == Rol.TECNICO)
        {
            var usuarioId = await _context.Usuarios
                .Where(u => u.AuthId == cuentaId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
            ordenes = ordenes.Where(o => o.UsuarioId == usuarioId);
        }

        var delMes = ordenes.Where(o => o.FechaCreacion >= inicio && o.FechaCreacion < fin);
        var delMesAnterior = ordenes.Where(o => o.FechaCreacion >= inicioAnterior && o.FechaCreacion < inicio);

        // los detalles de las órdenes del mes
        var detallesDelMes = _context.Detalles.AsNoTracking()
            .Where(d => delMes.Select(o => o.OrdenTrabajoId).Contains(d.OrdenTrabajoId));

        var respuesta = new DashboardResponse
        {
            Anio = anio,
            Mes = mes,
            TotalSots = await delMes.CountAsync(),
            MontoTotal = await delMes.SumAsync(o => o.PrecioTotal ?? 0),
            MontoAPagar = await delMes.SumAsync(o => o.SinPago ? 0 : (o.PrecioTotal ?? 0) - o.Descuento),
            SotsAuditadas = await delMes.CountAsync(o => o.FechaAuditoria != null),
            SotsSinImagen = await delMes.CountAsync(o => !o.Archivos.Any()),
            TotalSotsMesAnterior = await delMesAnterior.CountAsync(),
            MontoTotalMesAnterior = await delMesAnterior.SumAsync(o => o.PrecioTotal ?? 0),
        };

        // 2. Por estado
        respuesta.PorEstado = await delMes
            .GroupBy(o => o.Estado)
            .Select(g => new EstadoResumen
            {
                Estado = g.Key,
                Cantidad = g.Count(),
                Monto = g.Sum(o => o.PrecioTotal ?? 0),
            })
            .ToListAsync();

        // 3. Por técnico
        respuesta.PorTecnico = await delMes
            .GroupBy(o => new { o.UsuarioId, o.Usuario.NombreCompleto })
            .Select(g => new TecnicoResumen
            {
                UsuarioId = g.Key.UsuarioId,
                Nombre = g.Key.NombreCompleto,
                Cantidad = g.Count(),
                Monto = g.Sum(o => o.PrecioTotal ?? 0),
            })
            .OrderByDescending(t => t.Monto)
            .ToListAsync();

        // 4. Por servicio principal (los más realizados)
        respuesta.PorServicio = await detallesDelMes
            .Where(d => d.Tipo == TipoDetalle.SERVICIO)
            .GroupBy(d => new { d.ServicioCodigo, d.Servicio.Nombre })
            .Select(g => new ServicioResumen
            {
                Codigo = g.Key.ServicioCodigo,
                Nombre = g.Key.Nombre,
                Cantidad = g.Sum(d => d.Cantidad),
                Monto = g.Sum(d => d.PrecioTotal),
            })
            .OrderByDescending(s => s.Cantidad)
            .Take(TOP_SERVICIOS)
            .ToListAsync();

        // 5. Por tipo de detalle: cuánto suman servicio, adicionales y cada drop
        respuesta.PorTipoDetalle = await detallesDelMes
            .GroupBy(d => d.Tipo)
            .Select(g => new TipoDetalleResumen
            {
                Tipo = g.Key,
                Cantidad = g.Sum(d => d.Cantidad),
                Monto = g.Sum(d => d.PrecioTotal),
            })
            .OrderBy(t => t.Tipo)
            .ToListAsync();

        // 6. Últimas SOTs (de cualquier fecha, para acceso rápido)
        respuesta.UltimasSots = await ordenes
            .OrderByDescending(o => o.FechaCreacion)
            .ThenByDescending(o => o.OrdenTrabajoId)
            .Take(ULTIMAS_SOTS)
            .Select(o => new OrdenListaResponseDashboard
            {
                OrdenId = o.OrdenTrabajoId,
                Sot = o.Sot,
                Fecha = o.FechaCreacion,
                NombreUsuario = o.Usuario.NombreCompleto,
                Estado = o.Estado,
                PrecioTotal = o.PrecioTotal,
            })
            .ToListAsync();

        return respuesta;
    }

    public async Task<List<TecnicoOpcion>> ListarTecnicosAsync()
    {
        // técnicos registrados + cualquier usuario que ya tenga SOTs (p. ej. asignadas por la importación)
        return await _context.Usuarios.AsNoTracking()
            .Where(u => u.Cuenta.Rol == Rol.TECNICO || _context.Ordenes.Any(o => o.UsuarioId == u.Id))
            .OrderBy(u => u.NombreCompleto)
            .Select(u => new TecnicoOpcion { UsuarioId = u.Id, Nombre = u.NombreCompleto })
            .ToListAsync();
    }
}
