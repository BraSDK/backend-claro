using backend_claro.Application.DTOs.Liquidaciones;
using backend_claro.Application.DTOs;
using backend_claro.Application.Interfaces;
using backend_claro.Application.Mappings.Liquidaciones;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public class LiquidacionService : ILiquidacion
{
    private readonly IApplicationDbContext _context;
    public LiquidacionService(IApplicationDbContext context,IFileStorageService storage)
    {
        _context = context;
    }
    public async Task<PagedResponse<ListarRequestLiquidacionesDto>> ListarAsync(ListParamsLiquidacionesDto request, Rol rolUsuario, int cuentaId)
    {
        var query = _context.Liquidaciones.AsNoTracking(); // Ya trae todas las liquidaciones
    
        query = AplicarFiltros(query,request);

        var totalRegistros = await query.CountAsync();

        var listaLiquidaciones  = await query.AsNoTracking()
                                             .Include( l => l.Detalles)
                                             .Include( l => l.Observaciones)
                                             .OrderByDescending(l=> l.FechaCreacion)
                                             .ThenByDescending( l => l.Sot)
                                             .Skip((request.Pagina-1) * request.CanPagina)
                                             .Take(request.CanPagina)
                                             .ToListAsync();

        return new PagedResponse<ListarRequestLiquidacionesDto>
        {
            ListaGenerica = listaLiquidaciones.Select( l => l.ToResponse()).ToList(),
            NumeroPagina = request.Pagina,
            TamanoPagina = request.CanPagina,
            TotalRegistros = totalRegistros,
            TotalRegistrosPagina = listaLiquidaciones.Count()
        };
                                            
    }
    
    public async Task CrearLiquidacionAsync(CrearLiquidacionRequest request, int cuentaId)
    {
        if (await _context.Liquidaciones.AnyAsync(l => l.Sot == request.Sot))
        {
            throw new AlreadyExistException("Ya existe una sot liquidada con esta sot");
        }
        var liquidacion = request.ToEntity();
        var catalogo = await CatalogoAdicionalesAsync();
        foreach (var detalle in liquidacion.Detalles) AplicarPrecio(detalle, catalogo);
        liquidacion.RecalcularPrecioTotalReal();   // antes quedaba en null al crear y la auditoría no tenía con qué comparar

        // quien escribe la observación es el usuario del token, no el que mande el front
        var usuarioId = await UsuarioDeCuentaAsync(cuentaId);
        foreach (var observacion in liquidacion.Observaciones) observacion.UsuarioId = usuarioId ?? observacion.UsuarioId;
        AplicarObservacionNoPago(liquidacion, teniaNoPago: false);
        _context.Liquidaciones.Add(liquidacion);
        await _context.SaveChangesAsync();
    }

    public async Task EditarLiquidacionAsync(EditarLiquidacionRequestDto request, int cuentaId)
    {
        var liquidacion = await _context.Liquidaciones.Include(l => l.Detalles)
                                                      .Include( l => l.Observaciones)
                                                      .FirstOrDefaultAsync(l => l.Sot == request.Sot); // la SOT es única (índice único): es la clave para editar

        if(liquidacion is  null)
        {
            throw new InvalidOperationException("No existe ");
        }

        // La SOT no se reasigna: es la clave con la que se encontró el registro
        liquidacion.TipoOrden = request.TipoOrden;
        liquidacion.NombreSubTipoOrden = request.NombreSubTipoOrden;
        liquidacion.FechaProgramacion = DateTime.SpecifyKind(request.FechaProgramacion.Date, DateTimeKind.Utc); // Npgsql (timestamptz) solo acepta UTC
        liquidacion.NombreTecnico = request.NombreTecnico ?? "sin tecnico";
        liquidacion.EstadoSistema = request.EstadoSistema;

        var catalogo = await CatalogoAdicionalesAsync();

        foreach( var i in request.DetallesNuevos)
        {
            var nuevo = i.ToEntity();
            AplicarPrecio(nuevo, catalogo);
            liquidacion.AgregarDetalles(nuevo);
        }

        foreach (var j in request.DetallesEditados)
        {
            var detalle = liquidacion.Detalles.FirstOrDefault(d => d.DetalleLiquidacionId == j.DetalleId) ?? throw new NotFoundException("detalle no encontrado");
            // el adicional sale del id si lo mandan; si no, del catálogo por el nombre del concepto (DECO, DROP 150...)
            var costoAdicional = j.CostoAdicionalId.HasValue
                ? await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == j.CostoAdicionalId) ?? throw new NotFoundException("costo adicional no encontrado")
                : catalogo.GetValueOrDefault(NormalizarConcepto(j.Concepto ?? detalle.Concepto));

            if (costoAdicional is not null)
                detalle.EditarDetalle(costoAdicional, cantidad: j.Cantidad);
            else
                detalle.EditarDetalle(concepto: j.Concepto, precioUnitario: j.PrecioUnitario, cantidad: j.Cantidad);
        }
        liquidacion.RecalcularPrecioTotalReal();

        foreach (var item in request.DetallesEliminados)
        {
            liquidacion.EliminarDetalle(item);
        }
        
        liquidacion.PrecioTotalEditable = request.PrecioTotalEditable;
        var teniaNoPago = TieneObservacionNoPago(liquidacion);
        var usuarioObservacion = await UsuarioDeCuentaAsync(cuentaId);
        foreach (var item in request.ObservacionesNuevas)
        {
            var observacion = item.ToEntity();
            observacion.UsuarioId = usuarioObservacion ?? observacion.UsuarioId;
            liquidacion.AgregarObservacion(observacion);
        }

        foreach (var item in request.ObservacionesEditadas)
        {
            if (item.MotivoObservacionId.HasValue && !await _context.motivos.AnyAsync(m => m.MotivoObservacionId == item.MotivoObservacionId))
            {
                throw new NotFoundException("Motivo de observación no encontrado");
            }
            liquidacion.EditarObservacion(item.ObservacionLiquidacionId, item.MotivoObservacionId, item.Texto);
        }

        foreach( var index in request.ObservacionesEliminadas)
        {
            liquidacion.EliminarObservacion(index);
        }
        AplicarObservacionNoPago(liquidacion, teniaNoPago);
        
        await _context.SaveChangesAsync();
    }

    public async Task EliminarLiquidacionAsync(int id)
    {
        var liquidacion = await _context.Liquidaciones.FirstOrDefaultAsync( l => l.Sot == id) ?? throw new NotFoundException("Liquidacion no encontrada"); // id = SOT
        _context.Liquidaciones.Remove(liquidacion);
        await _context.SaveChangesAsync();
    }
    // Cruza las filas del Excel de Claro con las liquidaciones por SOT. No guarda nada.
    public async Task<PrevisualizarAuditoriaResponse> PrevisualizarAuditoriaAsync(PrevisualizarAuditoriaRequest request)
    {
        // una SOT puede venir en varias filas (pagos parciales): se suman
        var pagos = request.Filas
            .Where(f => f.Sot > 0)
            .GroupBy(f => f.Sot)
            .ToDictionary(g => g.Key, g => (Monto: g.Sum(f => f.MontoPagado), Filas: g.Count()));

        var sots = pagos.Keys.ToList();
        var liquidaciones = await _context.Liquidaciones.AsNoTracking()
                                          .Where(l => sots.Contains(l.Sot))
                                          .ToDictionaryAsync(l => l.Sot);

        var filas = pagos.Select(p =>
        {
            liquidaciones.TryGetValue(p.Key, out var l);
            var total = l?.TotalACobrar ?? 0;
            return new FilaAuditoriaPreview
            {
                Sot = p.Key,
                Existe = l is not null,
                NombreTecnico = l?.NombreTecnico,
                PrecioTotalReal = l?.PrecioTotalReal,
                PrecioTotalEditable = l?.PrecioTotalEditable,
                TotalACobrar = total,
                MontoPagado = p.Value.Monto,
                Diferencia = p.Value.Monto - total,
                FilasEnExcel = p.Value.Filas,
                EstadoActual = l?.EstadoPago ?? EstadoPago.Pendiente,
                EstadoSugerido = SugerirEstado(p.Value.Monto, total),
            };
        })
        .OrderBy(f => f.Existe).ThenBy(f => f.Sot)   // primero las que no existen, para que se vean
        .ToList();

        return new PrevisualizarAuditoriaResponse
        {
            Filas = filas,
            Encontradas = filas.Count(f => f.Existe),
            NoEncontradas = filas.Count(f => !f.Existe),
        };
    }

    // Guarda estado, monto pagado y precio editado. Lo usan la grilla (edición / pegado masivo) y el Confirmar del Excel.
    public async Task<AuditarLiquidacionesResponse> AuditarAsync(AuditarLiquidacionesRequest request, int cuentaId)
    {
        var sots = request.Items.Select(i => i.Sot).Distinct().ToList();
        var liquidaciones = await _context.Liquidaciones
                                          .Where(l => sots.Contains(l.Sot))
                                          .ToDictionaryAsync(l => l.Sot);

        var noEncontradas = new List<long>();
        foreach (var item in request.Items)
        {
            if (!liquidaciones.TryGetValue(item.Sot, out var l)) { noEncontradas.Add(item.Sot); continue; }

            l.EstadoPago = item.EstadoPago;
            l.MontoPagado = item.MontoPagado;
            l.PrecioTotalEditable = item.PrecioTotalEditable;   // null = se cobra el calculado
            l.SinPago = item.EstadoPago == EstadoPago.NoPagara;
            if (l.PrecioTotalEditable.HasValue) l.InsertarDiferencia(); else l.Descuento = 0;

            // Pendiente = sin auditar; cualquier otro estado deja registro de quién y cuándo
            l.FechaValidacion = item.EstadoPago == EstadoPago.Pendiente ? null : DateTime.UtcNow;
            l.AuditadoPorCuentaId = item.EstadoPago == EstadoPago.Pendiente ? null : cuentaId;
            l.FechaActualizacion = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return new AuditarLiquidacionesResponse { Actualizadas = request.Items.Count - noEncontradas.Count, NoEncontradas = noEncontradas };
    }

    private static EstadoPago SugerirEstado(decimal pagado, decimal total)
    {
        if (pagado <= 0) return EstadoPago.NoPagara;
        if (pagado >= total - 0.01m) return EstadoPago.SiPago;   // tolerancia de redondeo
        return EstadoPago.Incompleto;
    }

    private static IQueryable<Liquidacion> AplicarFiltros(IQueryable<Liquidacion> query, ListParamsLiquidacionesDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Buscar))
        {
            var texto = request.Buscar.Trim();
            query = query.Where(l => l.Sot.ToString().Contains(texto));
        }

        if (request.EstadoPago.HasValue)
        {
            query = query.Where(l => l.EstadoPago == request.EstadoPago);
        }

        if (request.Auditada.HasValue)
        {
            query = request.Auditada.Value ? query.Where(l => l.FechaValidacion != null) : query.Where(l => l.FechaValidacion == null);
        }

        if (request.Desde.HasValue)
        {
            var desde = DateTime.SpecifyKind(request.Desde.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.FechaCreacion >= desde);
        }

        if (request.Hasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(request.Hasta.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(o => o.FechaCreacion < hasta);
        }

        return query;
    }



    // ===== Precios de los adicionales =====
    // El precio de DECO, REPETIDOR y los DROP no lo decide la grilla: sale del catálogo (api/Adicional).
    // El costo de servicio no está en el catálogo y se queda con el precio que escribieron.

    private static string NormalizarConcepto(string? concepto) => (concepto ?? string.Empty).Trim().ToUpperInvariant();

    private async Task<Dictionary<string, CostoAdicional>> CatalogoAdicionalesAsync()
    {
        var adicionales = await _context.Adicionales.ToListAsync();
        return adicionales
            .GroupBy(a => NormalizarConcepto(a.Nombre))
            .ToDictionary(g => g.Key, g => g.First());
    }

    private static void AplicarPrecio(DetalleLiquidacion detalle, Dictionary<string, CostoAdicional> catalogo)
    {
        if (catalogo.TryGetValue(NormalizarConcepto(detalle.Concepto), out var adicional))
            detalle.EditarDetalle(adicional, cantidad: detalle.Cantidad);
        else
            detalle.CalcularPrecioTotal();   // no se confía en el PrecioTotal que manda el front
    }

    // ===== Observación de no pago =====
    // Si la SOT tiene una observación "no pago" con texto, no se cobra: queda como NoPagara.
    // Si la borran, ya no hay motivo de no pago: vuelve a Pendiente para auditarla otra vez.
    private static void AplicarObservacionNoPago(Liquidacion liquidacion, bool teniaNoPago)
    {
        if (TieneObservacionNoPago(liquidacion))
        {
            liquidacion.EstadoPago = EstadoPago.NoPagara;
            liquidacion.SinPago = true;
        }
        else if (teniaNoPago && liquidacion.EstadoPago == EstadoPago.NoPagara)
        {
            liquidacion.EstadoPago = EstadoPago.Pendiente;
            liquidacion.SinPago = false;
            liquidacion.FechaValidacion = null;
            liquidacion.AuditadoPorCuentaId = null;
        }
    }

    private static bool TieneObservacionNoPago(Liquidacion liquidacion) =>
        liquidacion.Observaciones.Any(o => o.Tipo == TipoObservacion.NO_PAGO && !string.IsNullOrWhiteSpace(o.Texto));

    private async Task<int?> UsuarioDeCuentaAsync(int cuentaId) =>
        await _context.Usuarios.Where(u => u.AuthId == cuentaId).Select(u => (int?)u.Id).FirstOrDefaultAsync();
}

