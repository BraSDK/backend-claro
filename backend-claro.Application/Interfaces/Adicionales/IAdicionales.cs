using backend_claro.Application.DTOs.Adicional;
using backend_claro.Application.DTOs;
using backend_claro.Domain.Enums;
namespace backend_claro.Application.Interfaces;

public interface IAdicionales
{
    Task CrearAdicional (CrearAdicionalDto adicional);

    Task EditarAdicional (int adicionalId,UpdateAdicionalDto request);
    Task EliminarAdicional (int id);
    Task<List<RequestAdicionalListDto>> ListarAdicional ( Rol rolUsuario);
    Task<RequestAdicionalDto> TraerById(int id);
}