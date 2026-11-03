using backend_claro.Application.DTOs.Reportes;

namespace backend_claro.Application.Interfaces;

public interface IReporteService
{
    // Excel con las SOT auditadas como "Pagó" en el mes (por fecha de auditoría)
    Task<byte[]> PagosValidadosExcelAsync(ReporteFiltroDto filtro);
}
