using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class ListRequestOrdenesDto
{
    public int Pagina {get;set;} =1;
    private const int  CanMaxPag = 50;
    private int _canPag = 15;
    public int CanPagina
    {
        get  => _canPag;
        set => _canPag = (value > CanMaxPag) ? CanMaxPag : (value <= 0 ? 10 : value);
    }

    // ===== Filtros (todos opcionales) =====
    public string? Buscar { get; set; }          // SOT (o parte de ella)
    public Estados? Estado { get; set; }
    public bool? Auditada { get; set; }          // true = auditadas, false = sin auditar
    public int? TecnicoId { get; set; }          // Usuario.Id; un TÉCNICO siempre ve solo las suyas
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }         // inclusive (se toma hasta el final del día)
}
