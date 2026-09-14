using backend_claro.Application.DTOs.Dashboard;

namespace backend_claro.Application.Interfaces;
public interface IDashboardService
{
    Task <DashboardResumenDto> ObtenerResumenAsync();
}

