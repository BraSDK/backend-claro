using backend_claro.Application.DTOs;
using Microsoft.AspNetCore.Http;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface IOrdenTrabajoService
{
    Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request, Rol rolUsuario, int cuentaId);
    Task EliminarOrdenAsync(int ordenId);
    Task<OrdenListaResponse> CrearOrdenTrabajoAsync(CrearOrdenTrabajoRequest request, Rol rolUsuario, int cuentaId);
    Task<OrdenListaResponse> EdicionRapidaAsync(int ordenId, EdicionRapidaRequest request);

    Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos);
    Task EliminarArchivoAsync(int ordenId, int archivoId);
}
