using System.Collections.ObjectModel;
using backend_claro.Application.DTOs;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
namespace backend_claro.Application;


public class DetailsDto
{
    public int OrdenId {get; set;}
    public int Sot {get; set;}
    public string Descripcion {get; set;} = string.Empty;
    public Estados Estado {get; set;}

    public ICollection<OrdenArchivoDto> Imagenes {get; set;}=new List<OrdenArchivoDto>();
    public ICollection<OrdenTrabajoDetalleDto> Detalles {get; set;} = new List<OrdenTrabajoDetalleDto>();

}