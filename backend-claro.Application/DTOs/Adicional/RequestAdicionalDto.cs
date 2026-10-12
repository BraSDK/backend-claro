using backend_claro.Domain.Entities;
public class RequestAdicionalDto
{

    public string Nombre {get; set;} = String.Empty;

    public decimal Precio { get; set; } 

    public CostoAdicional.TipoAdicional Tipo { get; set; }

    public string? ColumnaExcel { get; set; }
}