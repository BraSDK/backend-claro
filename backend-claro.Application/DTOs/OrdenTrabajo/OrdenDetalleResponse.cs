using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class OrdenDetalleResponse
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public decimal? PrecioTotal { get; set; }
    public List<DetalleResponse> Detalles { get; set; } = new();
    public List<ArchivoResponse> Imagenes { get; set; } = new();
}
