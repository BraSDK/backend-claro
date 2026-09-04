using backend_claro.Domain.Entities;

namespace backend_claro.Application.DTOs.Service;

public class UpdateRequestDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; } = 0;
    public Servicio.CategoriaServicio Categoria { get; set; }
    // public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
