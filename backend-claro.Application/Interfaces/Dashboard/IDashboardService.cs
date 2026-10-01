using backend_claro.Application.DTOs.Dashboard;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface IDashboardService
{
    // Un TÉCNICO solo ve sus propias SOTs; ADMIN y ALMACÉN ven todas
    Task<DashboardResponse> ResumenMensualAsync(int anio, int mes, Rol rolUsuario, int cuentaId);

    // Usuarios con rol TÉCNICO, para los filtros
    Task<List<TecnicoOpcion>> ListarTecnicosAsync();
}
