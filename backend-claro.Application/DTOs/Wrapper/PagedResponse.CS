namespace backend_claro.Application.DTOs;

public class PagedResponse<T>
{
    public List<T> ListaOrdenes {get; set;} = new();
    public int TotalRegistros {get; set;}
    public int NumeroPagina {get; set;}
    public int TotalRegistrosPagina {get; set;}

    public int TotalPaginasTotales => (int)Math.Ceiling((double)TotalRegistros/TotalRegistrosPagina);
}