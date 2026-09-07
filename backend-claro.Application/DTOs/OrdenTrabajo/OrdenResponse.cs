using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class OrdenResponse
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public int UsuarioId { get; set; }
    public List<ArchivoResponse> Imagenes { get; set; } = new();
}
