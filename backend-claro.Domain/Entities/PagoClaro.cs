using System.ComponentModel.DataAnnotations;
using backend_claro.Domain.Interfaces;

namespace backend_claro.Domain.Entities;

// Un pago de Claro (lote): agrupa las SOT ya auditadas que cubre ese pago (Pagó, Incompleto, No pagó)
// y las multas generales que Claro descontó en él.
public class PagoClaro : IAuditable
{
    public int PagoClaroId { get; set; }

    public DateTime FechaPago { get; set; }

    [MaxLength(60)]
    public string Comprobante { get; set; } = string.Empty;   // n° de boleta / factura / operación

    [MaxLength(300)]
    public string? Observacion { get; set; }

    public int? RegistradoPorCuentaId { get; set; }

    public ICollection<Liquidacion> Liquidaciones { get; set; } = new List<Liquidacion>();
    public ICollection<MultaClaro> Multas { get; set; } = new List<MultaClaro>();

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
