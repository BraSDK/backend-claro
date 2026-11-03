using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.PagosClaro;

// Filtro por rango de fechas (inclusive). En pagos se usa la fecha de pago; en SOT pendientes, la fecha de auditoría.
public class RangoFechasDto
{
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}

// SOT auditada (Pagó / Incompleto / No pagó): lo que entra en un pago de Claro
public class SotPagoDto
{
    public long Sot { get; set; }
    public string NombreTecnico { get; set; } = string.Empty;
    public DateTime FechaProgramacion { get; set; }
    public decimal TotalACobrar { get; set; }
    public decimal MontoPagado { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public DateTime? FechaValidacion { get; set; }
}

public class MultaClaroDto
{
    public int MultaClaroId { get; set; }
    public DateTime Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? NombreTecnico { get; set; }
    public int? PagoClaroId { get; set; }          // null = pendiente de descontar
    public string? ComprobantePago { get; set; }
}

public class GuardarMultaRequest
{
    public DateTime Fecha { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public decimal Monto { get; set; }
    public string? NombreTecnico { get; set; }
}

public class ListarMultasRequest : RangoFechasDto
{
    public bool? SoloPendientes { get; set; }      // true = solo las que no están en ningún pago
}

public class CrearPagoClaroRequest
{
    public DateTime FechaPago { get; set; }
    public string Comprobante { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public List<long> Sots { get; set; } = new();
    public List<int> MultaIds { get; set; } = new();
}

public class PagoClaroResumenDto
{
    public int PagoClaroId { get; set; }
    public DateTime FechaPago { get; set; }
    public string Comprobante { get; set; } = string.Empty;
    public string? Observacion { get; set; }
    public int CantidadSots { get; set; }
    public int Pagadas { get; set; }
    public int Incompletas { get; set; }
    public int NoPagadas { get; set; }
    public decimal TotalACobrar { get; set; }      // lo que se esperaba cobrar por esas SOT
    public decimal MontoPagado { get; set; }       // lo que Claro pagó por esas SOT
    public decimal MontoMultas { get; set; }
    public decimal Neto { get; set; }              // pagado - multas
    public DateTime FechaCreacion { get; set; }
}

public class PagoClaroDetalleDto : PagoClaroResumenDto
{
    public List<SotPagoDto> SotsDetalle { get; set; } = new();
    public List<MultaClaroDto> MultasDetalle { get; set; } = new();
}
