using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
namespace backend_claro.Application;

public class EditDto
{
    public int OrdenId {get; set;}
    public int Sot {get; set;}
    public string Descripcion {get; set;} = string.Empty;
    public Estados Estado {get; set;}

    public ICollection<OrdenTrabajoArchivo> Imagenes {get; set;}=new List<OrdenTrabajoArchivo>();
    public ICollection<DetalleTrabajo> Detalles {get; set;} = new List<DetalleTrabajo>();

}
