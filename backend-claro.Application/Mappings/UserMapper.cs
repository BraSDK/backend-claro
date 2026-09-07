using backend_claro.Application.DTOs.User;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings;

public static class UserMapper
{

    public static IQueryable<UserResponseDto> ToResponseDto(this IQueryable<CuentaUsuario> query)
    {
        return query.Select(u => new UserResponseDto
        {
            Id = u.Id,
            Email = u.Email,
            Rol = u.Rol,
            FechaRegistro = u.FechaRegistro
        });
    }

    public static CuentaUsuario ToEntity( this EditRequestDto request)
    {
        return new CuentaUsuario
        {
            Email = request.Email,
            Rol = request.Rol,
            Perfil = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                DocumentoIdentidad = request.DocumentoIdentidad,
                FechaActualizacion = DateTime.UtcNow,
            }
        };
    }
} 