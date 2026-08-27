namespace backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

public class CuentaUsuario
{

    public int Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public Rol Rol { get; set; } = Rol.ALMACEN;    
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    // Relacion de navegacion
    public Usuario Perfil { get; set; } = null!;
}