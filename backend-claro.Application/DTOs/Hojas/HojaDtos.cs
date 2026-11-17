namespace backend_claro.Application.DTOs.Hojas;

// Una pestaña de la grilla de liquidaciones
public class HojaDto
{
    public int HojaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int CantidadLiquidaciones { get; set; }
}

public class RenombrarHojaDto
{
    public string Nombre { get; set; } = string.Empty;
}
