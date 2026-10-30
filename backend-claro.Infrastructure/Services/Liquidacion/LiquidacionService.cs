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

        foreach( var i in request.DetallesNuevos)
        {
            liquidacion.AgregarDetalles(i.ToEntity());
        }

        foreach (var j in request.DetallesEditados)
        {
            var detalle = liquidacion.Detalles.FirstOrDefault(d => d.DetalleLiquidacionId == j.DetalleId) ?? throw new NotFoundException("detalle no encontrado");
            if (j.CostoAdicionalId.HasValue)
            {
                var costoAdicional = await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == j.CostoAdicionalId) ?? throw new NotFoundException("costo adicional no encontrado");
                detalle.EditarDetalle(costoAdicional,cantidad : j.Cantidad);
            }else{
            detalle.EditarDetalle(concepto: j.Concepto, precioUnitario: j.PrecioUnitario, cantidad: j.Cantidad);
            }
        }
        liquidacion.RecalcularPrecioTotalReal();

        foreach (var item in request.DetallesEliminados)
        {
            liquidacion.EliminarDetalle(item);
        }
        
        liquidacion.PrecioTotalEditable = request.PrecioTotalEditable;
        foreach (var item in request.ObservacionesNuevas)
        {
            liquidacion.AgregarObservacion(item.ToEntity());
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
        
        await _context.SaveChangesAsync();
    }

    public async Task EliminarLiquidacionAsync(long sot)
    {
        var liquidacion = await _context.Liquidaciones.FirstOrDefaultAsync( l => l.Sot == sot) ?? throw new NotFoundException("Liquidacion no encontrada");
        _context.Liquidaciones.Remove(liquidacion);
        await _context.SaveChangesAsync();
    }
    private static IQueryable<Liquidacion> AplicarFiltros(IQueryable<Liquidacion> query, ListParamsLiquidacionesDto request)
    {
        if (!string.IsNullOrWhiteSpace(request.Buscar))
        {
            var texto = request.Buscar.Trim();
            query = query.Where(l => l.Sot.ToString().Contains(texto));
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
}

