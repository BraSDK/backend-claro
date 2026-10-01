namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Auditoría masiva sin descuento ni multa.
// Con OrdenIds se auditan esas SOT; sin OrdenIds se auditan todas las que cumplan los Filtros ("Auditar todo").
public class AuditarMasivoRequest
{
    public List<int>? OrdenIds { get; set; }
    public ListRequestOrdenesDto? Filtros { get; set; }
    public bool Confirmar { get; set; }      // false = solo contar, true = auditar
}

public class AuditarMasivoResponse
{
    public int Auditadas { get; set; }       // auditadas (o que se auditarían si Confirmar = false)
    public int YaAuditadas { get; set; }     // se omiten para no pisar descuentos o multas
    public int SinImagen { get; set; }       // de las pendientes, cuántas no tienen imagen del técnico
}
