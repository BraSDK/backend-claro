using backend_claro.Application.DTOs;
using backend_claro.Application.DTOs.OrdenTrabajo;
using Microsoft.AspNetCore.Http;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.Interfaces;

public interface IOrdenTrabajoService
{
    // ============ LECTURA ============
    Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request);
    Task<OrdenDetalleResponse> ObtenerPorIdAsync(int id,Rol UsuarioRol);
    Task<OrdenDetalleResponse> ObtenerPorSotAsync(int sot);


    // ============ ORDEN  ============
    Task<OrdenResponse> CrearAsync(CrearOrdenRequest request);
    Task<OrdenDetalleResponse> EditarOrdenByTecnicoAsync(int ordenId, EditarOrdenRequest request);
    Task EliminarOrdenAsync(int ordenId);
    Task<OrdenDetalleResponse> EditarCompletoAsync(int ordenId, EditarOrdenCompletaRequest request, Rol rolUsuario);

    // ============ DETALLES  ============
    Task<DetalleResponse> AgregarDetalleAsync(int ordenId, CrearDetalleRequest request);
    Task EditarDetallesAsync(int ordenId ,List<DetalleEditar> requestList);
    Task EliminarDetalleAsync(int ordenId, int detalleId);

    // ============ ARCHIVOS ============
    Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos);
    Task EliminarArchivoAsync(int ordenId, int archivoId);
}
