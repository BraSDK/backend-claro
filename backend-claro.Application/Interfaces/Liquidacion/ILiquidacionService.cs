using backend_claro.Domain.Entities;
using backend_claro.Application.DTOs.Liquidaciones;
using backend_claro.Application.DTOs;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface ILiquidacion
{
    Task CrearLiquidacionAsync(CrearLiquidacionRequest request,int cuentaId);
    Task<PagedResponse<ListarRequestLiquidacionesDto>> ListarAsync(ListParamsLiquidacionesDto request,Rol rolUsuario,int cuentaId);
    Task EditarLiquidacionAsync(EditarLiquidacionRequestDto request,int cuentaId);
}