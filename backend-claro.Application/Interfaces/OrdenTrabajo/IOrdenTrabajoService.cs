using backend_claro.Application.DTOs.OrdenTrabajo;
using Microsoft.AspNetCore.Http;

namespace backend_claro.Application.Interfaces;

public interface IOrdenTrabajoService
{
    // ============ LECTURA ============
    Task<List<OrdenListaResponse>> ListarAsync();
    Task<OrdenDetalleResponse> ObtenerPorIdAsync(int id);
    Task<OrdenDetalleResponse> ObtenerPorSotAsync(int sot);

    // ============ ORDEN  ============
    Task<OrdenResponse> CrearAsync(CrearOrdenRequest request);
    Task<OrdenDetalleResponse> EditarAsync(int ordenId, EditarOrdenRequest request);

    // ============ DETALLES  ============
    Task<DetalleResponse> AgregarDetalleAsync(int ordenId, CrearDetalleRequest request);
    Task EditarDetallesAsunc(int ordenId ,List<DetalleEditar> requestList);
    Task EliminarDetalleAsync(int ordenId, int detalleId);

    // ============ ARCHIVOS ============
    Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos);
    Task EliminarArchivoAsync(int ordenId, int archivoId);
}
