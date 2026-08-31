using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;


public class ListViewDto
{
    public int Sot {get; set;}
    public string Descripcion {get; set;}=string.Empty;
    public Estados Estado {get; set;}
    

}