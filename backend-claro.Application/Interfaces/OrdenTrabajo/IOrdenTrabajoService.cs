using backend_claro.Application.DTOs;
using backend_claro.Application.DTOs;
using Microsoft.AspNetCore.Http;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface IOrdenTrabajoService
{
    // ============ LECTURA ============
    Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request, Rol rolUsuario, int cuentaId);
    Task EliminarOrdenAsync(int ordenId);
    Task<OrdenListaResponse> EdicionRapidaAsync(int ordenId, EdicionRapidaRequest request);
    Task<AuditoriaPagoResponse> AuditarPagoAsync(int ordenId, AuditoriaPagoRequest request, int cuentaId);

    Task EliminarDetalleAsync(int ordenId, int detalleId);

    // ============ ARCHIVOS ============
    Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos);
    Task EliminarArchivoAsync(int ordenId, int archivoId);
}
