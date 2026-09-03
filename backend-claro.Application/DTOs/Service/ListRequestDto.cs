using backend_claro.Domain.Entities;

namespace backend_claro.Application.DTOs.Service;

public class ListRequestDto
{
    public string? BuscarNombre { get; set; }
    public Servicio.CategoriaServicio? Categoria { get; set; }
    //Paginacion por defecto
    public int Pagina { get; set;} = 1;
    public int CantidadPorPagina { get; set;} = 10;
}