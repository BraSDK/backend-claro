using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.User;

public class EditRequestDto
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Rol Rol { get; set; } = Rol.ALMACEN;
}