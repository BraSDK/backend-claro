using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Liquidaciones;

public class ListParamsLiquidacionesDto
{
    public int Pagina {get;set;} =1;
    // La grilla carga la hoja (mes) completa para hacer scroll como en Sheets: antes el tope era 50 y se perdían filas.
    // 2000 deja margen sobre un mes grande (~400-1000 SOT) sin permitir pedir la tabla entera sin límite.
    private const int  CanMaxPag = 2000;
    private int _canPag = 15;
    public int CanPagina
    {
        get  => _canPag;
        set => _canPag = (value > CanMaxPag) ? CanMaxPag : (value <= 0 ? 10 : value);
    }
    // ===== Filtros (todos opcionales) =====
    public int? HojaId { get; set; }             // pestaña (mes) de la grilla
    public string? Buscar { get; set; }          // SOT (o parte de ella)
    public Estados? Estado { get; set; }
    public bool? Auditada { get; set; }          // true = auditadas, false = sin auditar
    public string? MesPago { get; set; }
    public string? SemanaPago { get; set; }
    public EstadoPago? EstadoPago { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }         // inclusive (se toma hasta el final del día)
}
