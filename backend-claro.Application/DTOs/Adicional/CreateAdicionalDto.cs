using backend_claro.Domain.Entities;

namespace backend_claro.Application.DTOs.Adicional;

public class CrearAdicionalDto
{

    public string Nombre {get; set;} = String.Empty;

    public decimal Precio { get; set; } 

    public CostoAdicional.TipoAdicional Tipo { get; set; }

    public string? ColumnaExcel { get; set; }
}