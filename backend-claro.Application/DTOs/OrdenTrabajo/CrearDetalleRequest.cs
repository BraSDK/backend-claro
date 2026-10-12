using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class CrearDetalleRequest
{
    public string Concepto { get; set; } = string.Empty;
    public decimal PrecioUnitario {get; set;}
    public int Cantidad { get; set; }
    public TipoDetalle Tipo { get; set; } = TipoDetalle.ADICIONAL;
}
