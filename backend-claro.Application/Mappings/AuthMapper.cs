using backend_claro.Application.DTOs.Auth;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Mappings;

public static class AuthMapper
{
    public static CuentaUsuario ToEntity(this RegisterRequestDto request)
    {
        return new CuentaUsuario
        {
            Email = request.Email,
            Rol = request.Rol,
            FechaRegistro = DateTime.UtcNow,
            Perfil = new Usuario
            {
                NombreCompleto = request.NombreCompleto,
                DocumentoIdentidad = request.DocumentoIdentidad,
                FechaActualizacion = DateTime.UtcNow
            }   
        };
    }
}