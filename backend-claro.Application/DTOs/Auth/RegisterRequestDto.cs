using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Auth;

public class RegisterRequestDto
{
    public string Email { get; set;} = string.Empty;
    public string Password { get; set;} = string.Empty;
    public string NombreCompleto { get; set;} = string.Empty;
    public string DocumentoIdentidad { get; set;} = string.Empty;
    public Rol Rol { get; set; } =Rol.ALMACEN;
}
