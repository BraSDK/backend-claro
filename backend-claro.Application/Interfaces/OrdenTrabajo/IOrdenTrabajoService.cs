using backend_claro.Application.DTOs;
using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.Interfaces;

public interface IOrdenTrabajoService 
{
    public Task<OrdenTrabajo> AsyncRegister(OrdenTrabajoDto request);
    public Task<List<ListViewDto>> AsyncListar();
    public Task<DetailsDto> AsyncObtenerPorSot(int sot);
    public Task<DetailsDto> AsyncObtenerPorId(int id);
    public Task<string> AsyncEdit(OrdenTrabajoDto request);
    public Task<string> AsyncUpdate(OrdenTrabajoDto request);
}
