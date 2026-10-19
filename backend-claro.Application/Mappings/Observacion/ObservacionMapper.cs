using backend_claro.Application.DTOs.Liquidaciones;
using backend_claro.Application.DTOs.Observaciones;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings.Liquidaciones;
public static class ObservacionMapper
{
    public static ObservacionLiquidacion ToEntity(this CrearObservacionDto request)
    {
        return new ObservacionLiquidacion
        {
            LiquidacionId = request.LiquidacionId,
            Tipo = request.Tipo,     // lo asigna el sistema, no el usuario
            Texto = request.TextoObservacion,
            UsuarioId = request.UsuarioId
        };
    }
}