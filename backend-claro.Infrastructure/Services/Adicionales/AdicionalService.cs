using backend_claro.Application.DTOs.Adicional;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Exceptions;
using backend_claro.Application.Mappings;
using backend_claro.Application.DTOs;
using Microsoft.IdentityModel.Tokens;

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

        if (ObjetoAdicional is not null) throw new AlreadyExistException($"Ya existe un adicional registrado con este nombre");

        var ServicioAdicional = await _context.Servicios.FirstOrDefaultAsync( s => s.Codigo == adicional.Codigo);
        if (ServicioAdicional is not null)
        {
            adicional.Precio = ServicioAdicional.Precio;
        }
        else
        {
           throw new NotFoundException("No se puede crear el adicional ya que el codigo de ser real");
        }

        

        await _context.Adicionales.AddAsync(adicional.ToEntity());

        
    }

    public async Task EditarAdicional(int adicionalId,UpdateAdicionalDto request)
    {
        decimal precio = 0;
        
        var servicioAdicional =  await _context.Servicios.FirstOrDefaultAsync( a => a.Codigo == request.Codigo) ?? throw new NotFoundException("servicio no existe");
        
        var adicionalObjeto = await _context.Adicionales.FirstOrDefaultAsync(a => a.AdicionalId == adicionalId)?? throw new NotFoundException("objeto no existe");
        
        if (servicioAdicional is not null)
        {
            precio = servicioAdicional.Precio;
        }
        else
        {
            throw new NotFoundException("No se puede editar el adicional ya que el codigo de ser real");
        }
        adicionalObjeto.Nombre = request.Nombre;
        adicionalObjeto.Codigo =  request.Codigo;
        adicionalObjeto.Precio = precio;
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
        // antes decía rolUsuario != 0, que dejaba fuera justo al ADMIN (Rol.ADMIN = 0)
        if (rolUsuario != Rol.TECNICO)
        {
                    var listaAdicionales = await _context.Adicionales.Select(a => new RequestAdicionalListDto
                    {
                            AdicionalId = a.AdicionalId,
                            Nombre = a.Nombre,
                            Codigo = a.Codigo,
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
