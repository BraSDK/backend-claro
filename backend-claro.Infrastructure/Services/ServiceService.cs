using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.Service;
using backend_claro.Application.Mappings;

namespace backend_claro.Infrastructure.Services;

public class ServiceService : IServiceService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    //Inyecciones del contexto de la Base de datos
    public ServiceService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }   

    public async Task<object> ListAsync(ListRequestDto request)
    {
        var query = _context.Servicios.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.BuscarNombre))
            query = query.Where(s => s.Nombre.ToLower().Contains(request.BuscarNombre.ToLower()));

        if (request.Categoria.HasValue)
            query = query.Where(s => s.Categoria == request.Categoria.Value);

        var totalRegistros = await query.CountAsync();

        var lista = await query
            .Skip((request.Pagina - 1) * request.CantidadPorPagina)
            .Take(request.CantidadPorPagina)
            .ToResponseDto() // MapperService
            .ToListAsync();

        return new 
        { 
            TotalRegistros = totalRegistros, 
            TotalPaginas = (int)Math.Ceiling(totalRegistros / (double)request.CantidadPorPagina),
            Datos = lista 
        };
    }

    public async Task<string> CreateAsync(CreateRequestDto request)
    {

        // Validar que el código no exista previamente
        var existe = await _context.Servicios.AnyAsync(s => s.Codigo == request.Codigo);
        if (existe) throw new Exception($"Ya existe un servicio con el código {request.Codigo}");

        // Validar que el nombre no se repita (opcional pero recomendado)
        var nombreExiste = await _context.Servicios.AnyAsync(s => s.Nombre.ToLower() == request.Nombre.ToLower());
        if (nombreExiste) throw new Exception("Ya existe un servicio con ese nombre.");

        var nuevoServicio = request.ToEntity();
        _context.Servicios.Add(nuevoServicio);
        await _context.SaveChangesAsync();

        return "Servicio registrado correctamente";
    }

    public async Task<string> UpdateAsync(UpdateRequestDto request)
    {
        var servicio = await _context.Servicios.FindAsync(request.Codigo);
        if (servicio == null) throw new Exception("Servicio no encontrado.");

        var nombreEnUso = await _context.Servicios
            .AnyAsync(s => s.Nombre.ToLower() == request.Nombre.ToLower() && s.Codigo != request.Codigo);
        if (nombreEnUso) throw new Exception("El nombre ingresado ya está en uso por otro servicio.");

        // 3. ¡El Mapper hace el trabajo! 
        // Delega toda la asignación manual a tu capa de Mapping
        request.UpdateEntity(servicio);

        await _context.SaveChangesAsync();

        return "Servicio actualizado correctamente";
    }

    public async Task<string> DeleteAsync(int codigo)
    {
        var servicio = await _context.Servicios.FindAsync(codigo);

        if ( servicio == null)
        {
            throw new ("El servicio no existe o se encuentra eliminado");
        }

        _context.Servicios.Remove(servicio);
        await _context.SaveChangesAsync();

        return "El servicio eliminado correctamente";
    }
}