
using backend_claro.Domain.Entities;
public class RequestAdicionalListDto
{

    public int AdicionalId {get; set;}
    public string Codigo { get; set; } = String.Empty;   

    public string Nombre {get; set;} = String.Empty;

    public decimal Precio { get; set; } 

    public CostoAdicional.TipoAdicional Tipo { get; set; }

    public string? ColumnaExcel { get; set; } 

}
