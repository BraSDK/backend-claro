using backend_claro.Application.DTOs.PagosClaro;

namespace backend_claro.Application.Interfaces;

public interface IPagoClaroService
{
    // SOT ya auditadas (Pagó / Incompleto / No pagó) que todavía no están en ningún pago
    Task<List<SotPagoDto>> SotsPendientesAsync(RangoFechasDto rango);

    Task<List<PagoClaroResumenDto>> ListarPagosAsync(RangoFechasDto rango);
    Task<PagoClaroDetalleDto> ObtenerPagoAsync(int id);
    Task<int> CrearPagoAsync(CrearPagoClaroRequest request, int cuentaId);
    Task EliminarPagoAsync(int id);

    Task<List<MultaClaroDto>> ListarMultasAsync(ListarMultasRequest request);
    Task<int> CrearMultaAsync(GuardarMultaRequest request);
    Task EditarMultaAsync(int id, GuardarMultaRequest request);
    Task EliminarMultaAsync(int id);
}
