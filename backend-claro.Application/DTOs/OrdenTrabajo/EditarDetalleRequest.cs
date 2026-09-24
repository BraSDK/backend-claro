using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;


public class DetalleEditar
{
    public int DetalleId { get; set; }
    public int? ServicioCodigo { get; set; }
    public int? Cantidad { get; set; }
    public TipoDetalle? Tipo { get; set; }
}
