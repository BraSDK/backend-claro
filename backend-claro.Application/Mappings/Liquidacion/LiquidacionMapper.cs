using backend_claro.Application.DTOs.Liquidaciones;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings.Liquidaciones;
public static class LiquidacionMapper
{
    public static Liquidacion ToEntity(this CrearLiquidacionRequest request) // Llega del front
    {
        return new Liquidacion
        {
            Sot = request.Sot,
            TipoOrden =  request.TipoOrden ,
            NombreSubTipoOrden = request.NombreSubTipoOrden,
            FechaProgramacion = request.FechaProgramacion,
            NombreTecnico = request.NombreTecnico ?? "sin tecnico",
            EstadoSistema = request.EstadoSistema,
            Detalles = request.Detalles.Select(d => d.ToEntity()).ToList(),
            EstadoSot =  request.EstadoSot,
            OrdenTrabajoId = request.OrdenTrabajoId,
            UsuarioId = request.UsuarioId,
            Observaciones = request.Observaciones.Select(o => o.ToEntity()).ToList(),
        };
    }
    public static ListarRequestLiquidacionesDto ToResponse(this Liquidacion request) //Sale de la bd
    {
        return new ListarRequestLiquidacionesDto
        {
            Sot =  (int) request.Sot,
            TipoOrden = request.TipoOrden,
            NombreSubTipoOrden = request.NombreSubTipoOrden,
            FechaProgramacion = request.FechaProgramacion,
            NombreTecnico = request.NombreTecnico,
            EstadoSistema = request.EstadoSistema,
            DetallesLiquidacion = request.Detalles.Select( d => d.ToResponse()).ToList(),
           // vacío = nada eliminado
            PrecioTotalEditable = request.PrecioTotalEditable
        };
    }

}