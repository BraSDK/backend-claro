using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class OrdenDetalleResponse
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int? UsuarioId {get; set;}
    public Estados Estado { get; set; }
    public decimal? PrecioTotal { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;

    // Auditoría
    public decimal Descuento { get; set; }
    public bool SinPago { get; set; }
    public string? ObservacionAuditoria { get; set; }
    public DateTime? FechaAuditoria { get; set; }
    public bool Auditada => FechaAuditoria.HasValue;
    public EstadoPago EstadoPago { get; set; }
    public string? MotivoNoPago { get; set; }
    public string? ObservacionPago { get; set; }
    public decimal MontoAPagar => SinPago ? 0 : Math.Max(0, (PrecioTotal ?? 0) - Descuento);

    public List<DetalleResponse> Detalles { get; set; } = new();
    public List<ArchivoResponse> Imagenes { get; set; } = new();
}
