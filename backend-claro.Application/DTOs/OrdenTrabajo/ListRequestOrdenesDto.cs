namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class ListRequestOrdenesDto
{
    public int Pagina {get;set;} =1;
    private const int  CanMaxPag = 50;
    private int _canPag = 15;
    public int CanPagina
    {
        get  => _canPag;
        set => _canPag = (value > CanMaxPag) ? CanMaxPag : (value <= 0 ? 10 : value);
    }
    public DateTime FechaCreacion {get; set;}

}