using backend_claro.Application.DTOs.Conciliacion;

namespace backend_claro.Application.Interfaces;

public interface IConciliacionService
{
    Task<ConciliacionResponse> ConciliarAsync(ConciliacionRequest request);
    Task<int> MarcarApelacionAsync(MarcarApelacionRequest request);

    // Excel con las SOT que no cuadran (diferencia, no pagada, por auditar)
    Task<byte[]> ExportarDiferenciasAsync(int anio, int mes);
}
