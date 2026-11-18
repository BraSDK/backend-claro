using System.Text.Json.Serialization;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.DTOs.Service;

public class CreateRequestDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; } = 0;
    // El front manda "HFC" / "FTH" / "MANTTO" (igual que lo devuelve el listado); sin el converter
    // esperaba el número y todo registro respondía 400. Acepta texto o número; solo afecta a estas propiedades.
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Servicio.CategoriaServicio Categoria { get; set; }
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public Servicio.SubTipoServicio SubTipoServicio {get; set;}
}