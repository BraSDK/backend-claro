using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;

public class OrdenResponse
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public int UsuarioId { get; set; }
    public List<ArchivoResponse> Imagenes { get; set; } = new();
    public EstadoAlmacen? EstadoAlmacen {get;set;}
    public string? Observacion {get; set;} 
}
