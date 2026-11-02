using backend_claro.Application.DTOs.Dashboard;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardResumenDto> ResumenAsync(int anio, int mes, Rol rolUsuario, int cuentaId);
    Task<List<TecnicoOpcionDto>> TecnicosAsync();
}
