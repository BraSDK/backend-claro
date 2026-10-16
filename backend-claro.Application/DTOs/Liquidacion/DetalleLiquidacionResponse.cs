namespace backend_claro.Application.DTOs.Liquidacion;

public class DetalleLiquidacionResponse
{
    public int DetalleLiquidacionId { get; set; }
    public string Concepto { get; set; } = string.Empty;
    public decimal PrecioUnitario { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioTotal { get; set; }
    public string Tipo { get; set; } = string.Empty; // Servicio o Adicional    
}