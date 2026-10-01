using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Resultado de auditar una SOT: descuento/penalización y su motivo
public class AuditarOrdenRequest
{
    public decimal Descuento { get; set; }              // en soles; se ignora si SinPago = true
    public bool SinPago { get; set; }                   // multa: no se paga nada
    public string Observacion { get; set; } = string.Empty;
    public Estados? Estado { get; set; }                // opcional: cambiar el estado al auditar
}
