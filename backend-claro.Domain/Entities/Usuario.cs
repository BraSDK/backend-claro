namespace backend_claro.Domain.Entities;

public class Usuario
{
  
    public int Id { get; set; } // se entiende que el framework lo indetifica por la palabra id 
    public int AuthId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string DocumentoIdentidad { get; set;} = string.Empty;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    // Relacion de navegación inversa
    public CuentaUsuario Cuenta { get; set;} = null!;
}