using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.User;
using backend_claro.Application.Mappings;

namespace backend_claro.Insfrastructure.Services;

public class UserService : IUserService
{
    private readonly IApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    //Inyectamos el contexto de la base de datos

    public UserService(IApplicationDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    public async Task<string> UpdateAsync(EditRequestDto request)
    {
        // 1. Verificar si el correo ya está registrado
        var usuarioExistente = await _context.CuentaUsuarios.Include(c => c.Perfil).FirstOrDefaultAsync(c => c.Id == request.Id);

        if (usuarioExistente == null)
        
            throw new Exception("Usuario no encontrado");
        
        // Llamamos al método Mapeado
        var correoEnUso = await _context.CuentaUsuarios.AnyAsync(c => c.Email == request.Email && c.Id != request.Id); //Tiene que validar tanto email y id pero que el id se unico

        if (correoEnUso)
            throw new Exception("El correo electrónico ya está en uso por otra cuenta.");

        // Mapeo Manual: Actulizamos SOLO las propiedades que cambian en la entidad rastreada
        usuarioExistente.Email = request.Email;
        usuarioExistente.Rol = request.Rol;

        //Actualizamos el perfil asociado - Si no llega encontrarlo vacio, solo actuliza los datos existentes
        if (usuarioExistente.Perfil != null)
        {
            usuarioExistente.Perfil.NombreCompleto = request.NombreCompleto;
            usuarioExistente.Perfil.DocumentoIdentidad = request.DocumentoIdentidad;
            usuarioExistente.Perfil.FechaActualizacion = DateTime.UtcNow;
        }

        //Manejo de la contraseña
        if (!string.IsNullOrWhiteSpace(request.PasswordHash))
        {
            usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordHash);
        }

        await _context.SaveChangesAsync();

        return "Usuario actualizado correctamente.";
    }
}

