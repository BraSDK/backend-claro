using backend_claro.Application.DTOs.Liquidacion;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings;

public static class LiquidacionMapper
{
    public static LiquidacionResponse ToResponse(this Liquidacion liquidacion) //se usa cuando sales de la BD y hay que devolver al front.
    {
        return new LiquidacionResponse
        {
            LiquidacionId = liquidacion.LiquidacionId,
            Sot = liquidacion.Sot,
            TipoOrden = liquidacion.TipoOrden,
            NombreSubTipoOrden = liquidacion.NombreSubTipoOrden,
            FechaProgramacion = liquidacion.FechaProgramacion,
            NombreTecnico = liquidacion.NombreTecnico,
            EstadoSistema = liquidacion.EstadoSistema.ToString(),
            PrecioTotalReal = liquidacion.PrecioTotalReal,
            PrecioTotalEditable = liquidacion.PrecioTotalEditable,
            Descuento = liquidacion.Descuento,
            EstadoSot = liquidacion.EstadoSot,
            EstadoPago = liquidacion.EstadoPago.ToString(),
            OrdenTrabajoId = liquidacion.OrdenTrabajoId,
            Detalles = liquidacion.Detalles?.Select(d => new DetalleLiquidacionResponse
            {
                DetalleLiquidacionId = d.DetalleLiquidacionId,
                Concepto = d.Concepto,
                PrecioUnitario = d.PrecioUnitario,
                Cantidad = d.Cantidad,
                PrecioTotal = d.PrecioTotal,
                Tipo = d.Tipo.ToString()

            }).ToList() ?? new List<DetalleLiquidacionResponse>()
        };
    } 
}