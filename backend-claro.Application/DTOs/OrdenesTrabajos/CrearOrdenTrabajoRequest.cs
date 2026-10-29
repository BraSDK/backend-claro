using backend_claro.Domain.Enums;
using backend_claro.Application.DTOs;
namespace backend_claro.Application.DTOs;


public class CrearOrdenTrabajoRequest
{
    public int Sot {get; set;}
    public string DocTecnico {get; set;} = string.Empty;
    public string TrabajoRealizado {get;set;} = string.Empty; //Nombre del trabajo realizado
    public EstadoClaro EstadoCampo {get; set;} = EstadoClaro.COMPLETADO; //estado en sitema oracle de claro - cuando el tecnico lo sube es practicamente completado.

    //no es necesario ingresar la lista de archivos, ya que en el service se detalla el guardado, además por el mismo controller el swager saca la data
    public int UsuarioId {get; set;} //tecnico

}
