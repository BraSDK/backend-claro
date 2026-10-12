using backend_claro.Domain.Entities;
public class UpdateAdicionalDto
{
    public string Nombre {get; set;} = String.Empty;

    public decimal Precio { get; set; } 

    public string ColumnaExcel {get; set;} = String.Empty;
    public CostoAdicional.TipoAdicional Tipo { get; set; }

}