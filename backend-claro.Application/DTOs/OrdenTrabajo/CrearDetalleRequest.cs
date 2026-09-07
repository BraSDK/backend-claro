using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class CrearDetalleRequest
{
    public int ServicioId { get; set; }
    public int Cantidad { get; set; }
    public TipoDetalle Tipo { get; set; } = TipoDetalle.DETALLE;
}
