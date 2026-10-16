using backend_claro.Application.DTOs.Liquidacion;

namespace backend_claro.Application.Interfaces;

public interface ILiquidacionService
{
    Task<ImportarExcelResponse> ImportarExcelAsync(Stream archivo, int usuarioId);
    Task<IEnumerable<LiquidacionResponse>> ObtenerLiquidacionAsync(); //la manguera, solo la va dar cuando la pidas "IEnumerable"
    Task<LiquidacionResponse> ObtenerPorIdAsync(int id);
}
