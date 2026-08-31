using backend_claro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;




public class OrdenTrabajoDto
{
    public int Sot {get; set;}
    public string Descripcion {get;set;} = string.Empty;

    public int UsuarioDto {get; set;}

    public Estados EstadoOt {get; set;} = Estados.INGRESADA;
    public List<IFormFile> Imagenes {get; set;} = new List<IFormFile>();
    
}



