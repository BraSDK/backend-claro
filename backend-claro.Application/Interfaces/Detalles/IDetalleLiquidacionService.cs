using backend_claro.Application.DTOs.Detalles;

namespace backend_claro.Application.Interfaces;

public interface IDetalleLiquidacion
{
     Task CrearLiquidacionAsync(CrearDetalleRequest request,int cuentaId);
}