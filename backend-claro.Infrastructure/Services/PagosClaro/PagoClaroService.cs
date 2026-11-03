using backend_claro.Application.DTOs.PagosClaro;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public class PagoClaroService : IPagoClaroService
{
    // estados de auditoría que entran en un pago (Pendiente y Observado todavía no están decididos)
    private static readonly EstadoPago[] EstadosPagables = { EstadoPago.SiPago, EstadoPago.Incompleto, EstadoPago.NoPagara };

    private readonly IApplicationDbContext _context;

    public PagoClaroService(IApplicationDbContext context)
    {
        _context = context;
    }

    // ===== SOT pendientes de registrar en un pago =====

    public async Task<List<SotPagoDto>> SotsPendientesAsync(RangoFechasDto rango)
    {
        var (desde, hasta) = Rango(rango);
        var query = _context.Liquidaciones.AsNoTracking()
            .Where(l => l.PagoClaroId == null && EstadosPagables.Contains(l.EstadoPago));
        if (desde.HasValue) query = query.Where(l => l.FechaValidacion >= desde);
        if (hasta.HasValue) query = query.Where(l => l.FechaValidacion < hasta);

        var filas = await query.OrderBy(l => l.FechaValidacion).ThenBy(l => l.Sot).Select(ProyectarSot()).ToListAsync();
        return filas.Select(AjustarPagado).ToList();
    }

    // ===== Pagos (lotes) =====

    public async Task<List<PagoClaroResumenDto>> ListarPagosAsync(RangoFechasDto rango)
    {
        var (desde, hasta) = Rango(rango);
        var query = _context.PagosClaro.AsNoTracking();
        if (desde.HasValue) query = query.Where(p => p.FechaPago >= desde);
        if (hasta.HasValue) query = query.Where(p => p.FechaPago < hasta);

        var pagos = await query
            .OrderByDescending(p => p.FechaPago).ThenByDescending(p => p.PagoClaroId)
            .Select(p => new
            {
                p.PagoClaroId, p.FechaPago, p.Comprobante, p.Observacion, p.FechaCreacion,
                Sots = p.Liquidaciones.Select(l => new SotPagoDto
                {
                    Sot = l.Sot,
                    TotalACobrar = l.PrecioTotalEditable ?? l.PrecioTotalReal ?? 0,
                    MontoPagado = l.MontoPagado ?? 0,
                    EstadoPago = l.EstadoPago,
                }).ToList(),
                MontoMultas = p.Multas.Sum(m => (decimal?)m.Monto) ?? 0,
            })
            .ToListAsync();

        return pagos.Select(p => Resumir(new PagoClaroResumenDto
        {
            PagoClaroId = p.PagoClaroId,
            FechaPago = p.FechaPago,
            Comprobante = p.Comprobante,
            Observacion = p.Observacion,
            FechaCreacion = p.FechaCreacion,
            MontoMultas = p.MontoMultas,
        }, p.Sots.Select(AjustarPagado).ToList())).ToList();
    }

    public async Task<PagoClaroDetalleDto> ObtenerPagoAsync(int id)
    {
        var pago = await _context.PagosClaro.AsNoTracking().FirstOrDefaultAsync(p => p.PagoClaroId == id)
            ?? throw new NotFoundException("Pago no encontrado");

        var sots = (await _context.Liquidaciones.AsNoTracking()
            .Where(l => l.PagoClaroId == id)
            .OrderBy(l => l.Sot)
            .Select(ProyectarSot())
            .ToListAsync()).Select(AjustarPagado).ToList();

        var multas = await _context.MultasClaro.AsNoTracking()
            .Where(m => m.PagoClaroId == id)
            .OrderBy(m => m.Fecha)
            .Select(ProyectarMulta())
            .ToListAsync();

        var detalle = new PagoClaroDetalleDto
        {
            PagoClaroId = pago.PagoClaroId,
            FechaPago = pago.FechaPago,
            Comprobante = pago.Comprobante,
            Observacion = pago.Observacion,
            FechaCreacion = pago.FechaCreacion,
            MontoMultas = multas.Sum(m => m.Monto),
            SotsDetalle = sots,
            MultasDetalle = multas,
        };
        Resumir(detalle, sots);
        return detalle;
    }

    public async Task<int> CrearPagoAsync(CrearPagoClaroRequest request, int cuentaId)
    {
        if (string.IsNullOrWhiteSpace(request.Comprobante))
            throw new InvalidOperationException("Ingresa el n° de comprobante del pago");
        var sots = request.Sots.Distinct().ToList();
        if (sots.Count == 0)
            throw new InvalidOperationException("Elige al menos una SOT auditada");

        var liquidaciones = await _context.Liquidaciones.Where(l => sots.Contains(l.Sot)).ToListAsync();

        // cada SOT tiene que existir, estar auditada (Pagó / Incompleto / No pagó) y no estar ya en otro pago
        var noExisten = sots.Except(liquidaciones.Select(l => l.Sot)).ToList();
        var sinAuditar = liquidaciones.Where(l => !EstadosPagables.Contains(l.EstadoPago)).Select(l => l.Sot).ToList();
        var yaPagadas = liquidaciones.Where(l => l.PagoClaroId != null).Select(l => l.Sot).ToList();
        var errores = new List<string>();
        if (noExisten.Count > 0) errores.Add($"no existen: {string.Join(", ", noExisten)}");
        if (sinAuditar.Count > 0) errores.Add($"sin auditar (Pendiente u Observado): {string.Join(", ", sinAuditar)}");
        if (yaPagadas.Count > 0) errores.Add($"ya están en otro pago: {string.Join(", ", yaPagadas)}");
        if (errores.Count > 0) throw new InvalidOperationException("No se puede registrar el pago. SOT " + string.Join("; ", errores));

        var multaIds = request.MultaIds.Distinct().ToList();
        var multas = await _context.MultasClaro.Where(m => multaIds.Contains(m.MultaClaroId)).ToListAsync();
        if (multas.Count != multaIds.Count) throw new NotFoundException("Alguna multa elegida no existe");
        if (multas.Any(m => m.PagoClaroId != null)) throw new InvalidOperationException("Alguna multa elegida ya está descontada en otro pago");

        var pago = new PagoClaro
        {
            FechaPago = Fecha(request.FechaPago),
            Comprobante = request.Comprobante.Trim(),
            Observacion = string.IsNullOrWhiteSpace(request.Observacion) ? null : request.Observacion.Trim(),
            RegistradoPorCuentaId = cuentaId,
        };
        foreach (var l in liquidaciones) pago.Liquidaciones.Add(l);
        foreach (var m in multas) pago.Multas.Add(m);

        _context.PagosClaro.Add(pago);
        await _context.SaveChangesAsync();
        return pago.PagoClaroId;
    }

    // Borra el registro del pago: sus SOT y multas vuelven a quedar pendientes (la auditoría no cambia)
    public async Task EliminarPagoAsync(int id)
    {
        var pago = await _context.PagosClaro
            .Include(p => p.Liquidaciones)
            .Include(p => p.Multas)
            .FirstOrDefaultAsync(p => p.PagoClaroId == id)
            ?? throw new NotFoundException("Pago no encontrado");

        foreach (var l in pago.Liquidaciones) l.PagoClaroId = null;
        foreach (var m in pago.Multas) m.PagoClaroId = null;
        _context.PagosClaro.Remove(pago);
        await _context.SaveChangesAsync();
    }

    // ===== Multas generales =====

    public async Task<List<MultaClaroDto>> ListarMultasAsync(ListarMultasRequest request)
    {
        var (desde, hasta) = Rango(request);
        var query = _context.MultasClaro.AsNoTracking();
        if (desde.HasValue) query = query.Where(m => m.Fecha >= desde);
        if (hasta.HasValue) query = query.Where(m => m.Fecha < hasta);
        if (request.SoloPendientes == true) query = query.Where(m => m.PagoClaroId == null);

        return await query.OrderByDescending(m => m.Fecha).ThenByDescending(m => m.MultaClaroId)
            .Select(ProyectarMulta())
            .ToListAsync();
    }

    public async Task<int> CrearMultaAsync(GuardarMultaRequest request)
    {
        Validar(request);
        var multa = new MultaClaro();
        Copiar(request, multa);
        _context.MultasClaro.Add(multa);
        await _context.SaveChangesAsync();
        return multa.MultaClaroId;
    }

    public async Task EditarMultaAsync(int id, GuardarMultaRequest request)
    {
        Validar(request);
        var multa = await _context.MultasClaro.FirstOrDefaultAsync(m => m.MultaClaroId == id)
            ?? throw new NotFoundException("Multa no encontrada");
        Copiar(request, multa);
        await _context.SaveChangesAsync();
    }

    public async Task EliminarMultaAsync(int id)
    {
        var multa = await _context.MultasClaro.FirstOrDefaultAsync(m => m.MultaClaroId == id)
            ?? throw new NotFoundException("Multa no encontrada");
        if (multa.PagoClaroId != null)
            throw new InvalidOperationException("La multa está descontada en un pago: primero elimina ese pago");
        _context.MultasClaro.Remove(multa);
        await _context.SaveChangesAsync();
    }

    // ===== Ayudas =====

    private static System.Linq.Expressions.Expression<Func<Liquidacion, SotPagoDto>> ProyectarSot() => l => new SotPagoDto
    {
        Sot = l.Sot,
        NombreTecnico = l.NombreTecnico,
        FechaProgramacion = l.FechaProgramacion,
        TotalACobrar = l.PrecioTotalEditable ?? l.PrecioTotalReal ?? 0,
        MontoPagado = l.MontoPagado ?? -1,   // -1 = no se escribió el monto en la auditoría (se completa en AjustarPagado)
        EstadoPago = l.EstadoPago,
        FechaValidacion = l.FechaValidacion,
    };

    private static System.Linq.Expressions.Expression<Func<MultaClaro, MultaClaroDto>> ProyectarMulta() => m => new MultaClaroDto
    {
        MultaClaroId = m.MultaClaroId,
        Fecha = m.Fecha,
        Motivo = m.Motivo,
        Monto = m.Monto,
        NombreTecnico = m.NombreTecnico,
        PagoClaroId = m.PagoClaroId,
        ComprobantePago = m.PagoClaro != null ? m.PagoClaro.Comprobante : null,
    };

    // Lo que se cuenta como pagado: No pagó = 0; si en la auditoría no se escribió el monto,
    // "Pagó" cuenta el total a cobrar e "Incompleto" cuenta 0
    private static SotPagoDto AjustarPagado(SotPagoDto s)
    {
        if (s.EstadoPago == EstadoPago.NoPagara) s.MontoPagado = 0;
        else if (s.MontoPagado < 0) s.MontoPagado = s.EstadoPago == EstadoPago.SiPago ? s.TotalACobrar : 0;
        return s;
    }

    private static T Resumir<T>(T pago, List<SotPagoDto> sots) where T : PagoClaroResumenDto
    {
        pago.CantidadSots = sots.Count;
        pago.Pagadas = sots.Count(s => s.EstadoPago == EstadoPago.SiPago);
        pago.Incompletas = sots.Count(s => s.EstadoPago == EstadoPago.Incompleto);
        pago.NoPagadas = sots.Count(s => s.EstadoPago == EstadoPago.NoPagara);
        pago.TotalACobrar = sots.Sum(s => s.TotalACobrar);
        pago.MontoPagado = sots.Sum(s => s.MontoPagado);
        pago.Neto = pago.MontoPagado - pago.MontoMultas;
        return pago;
    }

    private static void Validar(GuardarMultaRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Motivo)) throw new InvalidOperationException("Escribe el motivo de la multa");
        if (request.Monto <= 0) throw new InvalidOperationException("El monto de la multa debe ser mayor a 0");
    }

    private static void Copiar(GuardarMultaRequest request, MultaClaro multa)
    {
        multa.Fecha = Fecha(request.Fecha);
        multa.Motivo = request.Motivo.Trim();
        multa.Monto = request.Monto;
        multa.NombreTecnico = string.IsNullOrWhiteSpace(request.NombreTecnico) ? null : request.NombreTecnico.Trim();
    }

    // Npgsql (timestamptz) solo acepta UTC: se guarda el día a medianoche UTC
    private static DateTime Fecha(DateTime fecha) =>
        DateTime.SpecifyKind((fecha == default ? DateTime.UtcNow : fecha).Date, DateTimeKind.Utc);

    // "hasta" es inclusive: se compara contra el día siguiente
    private static (DateTime? desde, DateTime? hasta) Rango(RangoFechasDto r) => (
        r.Desde.HasValue ? Fecha(r.Desde.Value) : null,
        r.Hasta.HasValue ? Fecha(r.Hasta.Value).AddDays(1) : null);
}
