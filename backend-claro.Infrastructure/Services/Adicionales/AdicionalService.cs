using backend_claro.Application.DTOs.Adicional;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Exceptions;
using backend_claro.Application.Mappings;
using backend_claro.Application.DTOs;

namespace backend_claro.Infrastructure.Services;

public class AdicionalService : IAdicionales
{
    private readonly IApplicationDbContext _context;

    public AdicionalService(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task CrearAdicional(CrearAdicionalDto adicional)
    {
        var ObjetoAdicional = await _context.Adicionales.FirstOrDefaultAsync(a => a.Nombre == adicional.Nombre);

        if (ObjetoAdicional is not null) throw new AlredyExistException($"Ya existe un adicional registrado con este nombre");

        await _context.Adicionales.AddAsync(adicional.ToEntity());

        await _context.SaveChangesAsync();
    }

    public async Task EditarAdicional(int adicionalId,UpdateAdicionalDto request)
    {
        var adicionalObjeto = await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == adicionalId)?? throw new NotFoundException("objeto no existe");
        adicionalObjeto.Codigo = request.Codigo;
        adicionalObjeto.Nombre = request.Nombre;
        adicionalObjeto.Precio = request.Precio;
        adicionalObjeto.Tipo = request.Tipo;
        adicionalObjeto.ColumnaExcel = request.ColumnaExcel;
        await _context.SaveChangesAsync();
    }

    public async Task EliminarAdicional(int id)
    {
        var adicionalObjeto = await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == id) ?? throw new NotFoundException("No se puede eliminar este adicional ya que no existe");
        _context.Adicionales.Remove(adicionalObjeto);
        await _context.SaveChangesAsync();
    }

    public async Task<List<RequestAdicionalListDto>> ListarAdicional(Rol rolUsuario)
    {
        if (rolUsuario != 0)
        {
                    var listaAdicionales = await _context.Adicionales.Select(a => new RequestAdicionalListDto
                    {
                            AdicionalId = a.AdicionalId,
                            Codigo= a.Codigo,   
                            Nombre = a.Nombre,
                            Precio = a.Precio,
                            Tipo = a.Tipo,
                            ColumnaExcel = a.ColumnaExcel
                    }).ToListAsync();

                    return listaAdicionales;

        }

        throw new ForbiddenException("No tiene acceso a este recurso");
    }

    public async Task<RequestAdicionalDto> TraerById(int id)
    {

        var objetoSolicitado = await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == id)
                                                    ?? throw new NotFoundException("No se encontro el adicional solicitado");

        return objetoSolicitado.ToResponse();

    }
}
