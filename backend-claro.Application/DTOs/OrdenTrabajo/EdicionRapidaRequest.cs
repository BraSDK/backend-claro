using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Edición desde la tabla de Órdenes: se manda solo el campo que cambió.
// Para los montos se indica el tipo de línea (costo, adicional o drop) y el nuevo monto (0 = quitar esa línea).
public class EdicionRapidaRequest
{
    public Estados? Estado { get; set; }
    public string? Descripcion { get; set; }
    public int? UsuarioId { get; set; }

    public TipoDetalle? TipoMonto { get; set; }
    public decimal? Monto { get; set; }
}
