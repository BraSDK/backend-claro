namespace backend_claro.Application.DTOs.Auth;

// Datos del usuario autenticado (pantalla "Mi perfil")
public class PerfilResponseDto
{
    public int CuentaId { get; set; }
    public int? UsuarioId { get; set; }
    public string Email { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;
    public DateTime FechaRegistro { get; set; }
}

public class CambiarPasswordRequestDto
{
    public string PasswordActual { get; set; } = string.Empty;
    public string PasswordNueva { get; set; } = string.Empty;
}
