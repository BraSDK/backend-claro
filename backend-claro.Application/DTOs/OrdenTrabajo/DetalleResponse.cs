using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;


public class DetalleResponse
{
    public int DetalleId { get; set; }
    public int ServicioId { get; set; }
    public string NombreServicio { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal PrecioTotal { get; set; }
    public TipoDetalle Tipo { get; set; }
}
