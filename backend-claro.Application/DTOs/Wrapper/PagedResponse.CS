namespace backend_claro.Application.DTOs;

public class PagedResponse<T>
{
    public List<T> ListaOrdenes {get; set;} = new();
    public int TotalRegistros {get; set;}
    public int NumeroPagina {get; set;}
    public int TotalRegistrosPagina {get; set;}   // registros que trae ESTA página
    public int TamanoPagina {get; set;}           // registros por página pedidos

    // se divide por el tamaño de página (no por los registros de esta página) y nunca da 0
    public int TotalPaginasTotales => TamanoPagina <= 0
        ? 1
        : Math.Max(1, (int)Math.Ceiling((double)TotalRegistros / TamanoPagina));
}
