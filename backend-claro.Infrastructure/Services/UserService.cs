using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using backend_claro.Application.Interfaces;
using backend_claro.Application.DTOs.User;
using backend_claro.Application.Mappings;
using backend_claro.Domain.Enums;

namespace backend_claro.Infrastructure.Services;

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

    public async Task<string> UpdateAsync(EditRequestDto request, string rolLogueado)
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

        //Verifica si hay un rol distinto en la base de datos
        if (request.Rol != usuarioExistente.Rol)
        {
            if (rolLogueado != nameof(Rol.ADMIN))
            {
                throw new Exception("Acceso denegado: No tienes permisos para cambiar el rol de la cuenta.");
            }

            //Si es admin, aplicamos el cambio
            usuarioExistente.Rol = request.Rol;
        }
        
        
        //Actualizamos el perfil asociado - Si no llega encontrarlo vacio, solo actuliza los datos existentes
        if (usuarioExistente.Perfil != null)
        {
            usuarioExistente.Perfil.NombreCompleto = request.NombreCompleto;
            usuarioExistente.Perfil.DocumentoIdentidad = request.DocumentoIdentidad;
            usuarioExistente.Perfil.FechaActualizacion = DateTime.UtcNow;
        }

        //Manejo de la contraseña
        if (!string.IsNullOrWhiteSpace(request.PasswordNew))
        {
            usuarioExistente.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.PasswordNew);
        }

        await _context.SaveChangesAsync();

        return "Usuario actualizado correctamente.";
    }

    // Trae un usuario por su Id (a pesar del nombre "List", devuelve uno solo)
    public async Task<UserResponseDto> ListAsync(int id)
    {
        var usuario = await _context.CuentaUsuarios
            .Include(c => c.Perfil)
            .Where(c => c.Id == id)
            .Select(c => new UserResponseDto
            {
                Id = c.Id,
                Email = c.Email,
                Rol = c.Rol,
                FechaRegistro = c.FechaRegistro,
                NombreCompleto = c.Perfil.NombreCompleto,
                DocumentoIdentidad = c.Perfil.DocumentoIdentidad
            })
            .FirstOrDefaultAsync();

        if (usuario is null)
            throw new KeyNotFoundException($"No existe el usuario con Id {id}.");

        return usuario;
    }

    // Elimina la cuenta por Id (el Perfil se borra en cascada)
    public async Task<string> DeleteAsync(int id)
    {
        var cuenta = await _context.CuentaUsuarios.FindAsync(id);

        if (cuenta is null)
            throw new KeyNotFoundException($"No existe el usuario con Id {id}.");

        _context.CuentaUsuarios.Remove(cuenta);
        await _context.SaveChangesAsync();

        return "Usuario eliminado correctamente.";
    }
}

