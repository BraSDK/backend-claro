using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;
public class CrearArchivoRequest
{
    public string NombreArchivo {get; set;} = string.Empty;
    public string Src{get; set;} = string.Empty;
    public int OrdenTrabajoId { get; set; } 

}