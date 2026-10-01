using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

// PATCH api/OrdenTrabajo/{id}/auditoria-pago
public class AuditoriaPagoRequest
{
    public EstadoPago EstadoPago { get; set; }
    public string? MotivoNoPago { get; set; }      // obligatorio si EstadoPago = NoPago
    public string? ObservacionPago { get; set; }
}

public class AuditoriaPagoResponse
{
    public int OrdenId { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public string? MotivoNoPago { get; set; }
    public string? ObservacionPago { get; set; }
    public DateTime? FechaAuditoria { get; set; }
    public decimal MontoAPagar { get; set; }
}
