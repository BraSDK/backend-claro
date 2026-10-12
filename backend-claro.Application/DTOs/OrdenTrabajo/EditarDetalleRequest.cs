using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;


public class DetalleEditar
{
    public int DetalleId { get; set; }
    public string? Concepto { get; set; }
    public decimal? PrecioUnitario { get; set; }
    public int? Cantidad { get; set; }
    public TipoDetalle? Tipo { get; set; }
}
