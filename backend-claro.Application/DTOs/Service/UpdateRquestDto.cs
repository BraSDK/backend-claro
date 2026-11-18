using System.Text.Json.Serialization;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.DTOs.Service;

public class UpdateRequestDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; } = 0;
    // igual que en CreateRequestDto: el front manda la categoría como texto ("HFC")
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Servicio.CategoriaServicio Categoria { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Servicio.SubTipoServicio SubTipo { get; set; }
    // public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
