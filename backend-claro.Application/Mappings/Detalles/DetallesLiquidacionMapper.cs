using backend_claro.Application.DTOs;
using backend_claro.Application.DTOs.Detalles;
using backend_claro.Domain.Entities;

namespace backend_claro.Domain.Entities;
public static class DetalleLiquidacionMapper
{
    public static DetalleResponseDto ToResponse(this DetalleLiquidacion request)
    {
        return new DetalleResponseDto
        {
            DetalleLiquidacionId = request.DetalleLiquidacionId,
            // Qué adicional fue (para reportes). Null si es SERVICIO => precio base
            CostoAdicionalId = request.CostoAdicionalId,
            CostoAdicional = request.CostoAdicional,
            Concepto = request.Concepto,
            PrecioUnitario = request.PrecioUnitario,
            Cantidad  = request.Cantidad,
            PrecioTotal = request.PrecioTotal,
            Tipo = request.Tipo
        };
    }

    public static DetalleLiquidacion ToEntity(this CrearDetalleRequest request)
    {
        return new DetalleLiquidacion
        {
            DetalleLiquidacionId = request.DetalleLiquidacionId,
            // Qué adicional fue (para reportes). Null si es SERVICIO => precio base
            CostoAdicionalId = request.CostoAdicionalId,
            Concepto = request.Concepto,
            PrecioUnitario = request.PrecioUnitario,
            Cantidad  = request.Cantidad,
            PrecioTotal = request.PrecioTotal,
            Tipo = request.Tipo
        };
    }
}