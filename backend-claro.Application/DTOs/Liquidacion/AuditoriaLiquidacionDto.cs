using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Liquidaciones;

// ===== Previsualizar: el front lee el Excel y manda solo las 2 columnas elegidas =====
public class FilaPagoExcel
{
    public long Sot { get; set; }
    public decimal MontoPagado { get; set; }
}

public class PrevisualizarAuditoriaRequest
{
    public List<FilaPagoExcel> Filas { get; set; } = new();
}

public class FilaAuditoriaPreview
{
    public long Sot { get; set; }
    public bool Existe { get; set; }                 // false = la SOT está en el Excel pero no en el sistema
    public string? NombreTecnico { get; set; }
    public decimal? PrecioTotalReal { get; set; }
    public decimal? PrecioTotalEditable { get; set; }
    public decimal TotalACobrar { get; set; }        // editado ?? calculado
    public decimal MontoPagado { get; set; }
    public decimal Diferencia { get; set; }          // pagado - total (negativo = pagaron menos)
    public int FilasEnExcel { get; set; }            // > 1 = se sumaron pagos de varias filas
    public EstadoPago EstadoActual { get; set; }
    public EstadoPago EstadoSugerido { get; set; }
}

public class PrevisualizarAuditoriaResponse
{
    public List<FilaAuditoriaPreview> Filas { get; set; } = new();
    public int Encontradas { get; set; }
    public int NoEncontradas { get; set; }
}

// ===== Auditar: guarda lo que se ve en la fila (grilla o Confirmar del Excel) =====
public class AuditarLiquidacionItem
{
    public long Sot { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public decimal? MontoPagado { get; set; }
    public decimal? PrecioTotalEditable { get; set; }   // null = se cobra el precio calculado
}

public class AuditarLiquidacionesRequest
{
    public List<AuditarLiquidacionItem> Items { get; set; } = new();
}

public class AuditarLiquidacionesResponse
{
    public int Actualizadas { get; set; }
    public List<long> NoEncontradas { get; set; } = new();
    public int NuevasImportadas { get; set; }
    public List<long> SotsNuevas { get; set; } = new();
    public List<long> EnPago { get; set; } = new();   // ya están en un pago de Claro registrado: no se cambian
    
}
