namespace backend_claro.Domain.Entities;

public class CuentaUsuario
{
    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Rol { get; set; } = string.Empty;    
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relacion de navegacion
    public Usuario Perfil { get; set; } = null!;
}