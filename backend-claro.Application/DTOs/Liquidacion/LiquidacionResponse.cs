namespace backend_claro.Application.DTOs.Liquidacion;

public class LiquidacionResponse
{
    public int LiquidacionId { get; set; }
    public long Sot { get; set; }
    public string? TipoOrden { get; set;}
    public string NombreSubTipoOrden { get; set; } = string.Empty;
    public DateTime FechaProgramacion { get; set; }
    public string NombreTecnico { get; set; } = string.Empty;
    public string EstadoSistema { get; set; } = string.Empty;

    //Precios
    public decimal? PrecioTotalReal { get; set; }
    public decimal? PrecioTotalEditable { get; set; }
    public decimal? Descuento { get; set; }
    public string? EstadoSot { get; set; } = string.Empty;
    public string EstadoPago { get; set; } = string.Empty;

    // Relacion con fotos del tecnico (si las hay)
    public int? OrdenTrabajo { get; set; }

    // Relación con fotos del técnico (si las hay)
    public int? OrdenTrabajoId { get; set; }
    
    // Coleccion de detalles mapeados
    public ICollection<DetalleLiquidacionResponse> Detalles { get; set; } = new List<DetalleLiquidacionResponse>();
    
}