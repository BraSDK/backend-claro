using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;


public class OrdenListaResponse
{
    public int OrdenTrabajoId {get; set;}
    public int Sot {get; set;}
    public string DocTecnico {get; set;} = string.Empty;
    public string TrabajoRealizado {get;set;} = string.Empty; 
    public EstadoClaro EstadoCampo {get; set;} 
    public List<ArchivoResponse> Archivos {get; set;} = new List<ArchivoResponse>(); // archivos -imagenes
    public int UsuarioId {get; set;} 
}
