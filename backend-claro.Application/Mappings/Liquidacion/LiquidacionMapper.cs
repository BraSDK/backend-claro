using backend_claro.Application.DTOs.Liquidaciones;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings.Liquidaciones;
public static class LiquidacionMapper
{
    public static Liquidacion ToEntity(this CrearLiquidacionRequest request)
    {
        return new Liquidacion
        {
            Sot = request.Sot,
            TipoOrden =  request.TipoOrden ,
            NombreSubTipoOrden = request.NombreSubTipoOrden,
            FechaProgramacion = request.FechaProgramacion,
            NombreTecnico = request.NombreTecnico ?? "sin tecnico",
            EstadoSistema = request.EstadoSistema, 
            Detalles = request.Detalles,
            EstadoSot =  request.EstadoSot,
            OrdenTrabajoId = request.OrdenTrabajoId,
            OrdenTrabajo = request.OrdenTrabajo,
            UsuarioId = request.UsuarioId,
            Usuario = request.Usuario,
            Observaciones = request.Observaciones, 
        };
    }
    public static ListarRequestLiquidacionesDto ToResponse(this Liquidacion request)
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