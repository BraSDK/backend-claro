using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Conciliacion;

// Categoría de cada SOT en la conciliación (mientras no exista el reporte de Claro, sale de la auditoría)
public enum CategoriaConciliacion
{
    CUADRA = 0,        // auditada sin descuento: se paga completo
    DIFERENCIA = 1,    // auditada con descuento / penalidad
    NO_PAGADA = 2,     // auditada con multa: no se paga
    POR_AUDITAR = 3,   // todavía no auditada
}

public class ConciliacionRequest
{
    public int? Anio { get; set; }
    public int? Mes { get; set; }
    public CategoriaConciliacion? Categoria { get; set; }
    public string? Buscar { get; set; }
    public int? TecnicoId { get; set; }
    public bool? ConEvidencia { get; set; }
    public int? CategoriaServicio { get; set; }    // 0 HFC, 1 FTH, 2 MANTTO (del servicio principal)
    public bool? Apelacion { get; set; }
    public EstadoPago? EstadoPago { get; set; }
    public int Pagina { get; set; } = 1;
    public int CanPagina { get; set; } = 10;
}

public class ConciliacionResponse
{
    public int Anio { get; set; }
    public int Mes { get; set; }

    // KPIs del periodo (no dependen de los filtros de la tabla)
    public decimal TotalValorizado { get; set; }    // P. Total de todas las SOT
    public decimal TotalPagado { get; set; }        // lo que se paga de las auditadas
    public decimal DiferenciaTotal { get; set; }    // pagado - valorizado de las auditadas (negativo = descuentos)
    public int SotEvaluadas { get; set; }           // auditadas
    public int TotalSots { get; set; }
    public DateTime? UltimaAuditoria { get; set; }

    // totales en soles por estado de pago (suma del P. Total de cada SOT del periodo)
    public List<TotalEstadoPago> TotalesPorEstadoPago { get; set; } = new();

    // cantidad por categoría, para las pastillas de filtro
    public int Cuadra { get; set; }
    public int Diferencia { get; set; }
    public int NoPagada { get; set; }
    public int PorAuditar { get; set; }

    // tabla (con filtros y paginada)
    public List<FilaConciliacion> Filas { get; set; } = new();
    public int TotalFilas { get; set; }
    public int Pagina { get; set; }
    public int CanPagina { get; set; }
    public int TotalPaginas => CanPagina <= 0 ? 1 : Math.Max(1, (int)Math.Ceiling((double)TotalFilas / CanPagina));
}

public class FilaConciliacion
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public string Tecnico { get; set; } = string.Empty;
    public string TipoServicio { get; set; } = string.Empty;    // nombre del servicio principal
    public decimal Valorizado { get; set; }
    public decimal? Pagado { get; set; }                        // null = por auditar
    public decimal? Diferencia { get; set; }
    public CategoriaConciliacion Categoria { get; set; }
    public int CantidadImagenes { get; set; }
    public bool MarcadaApelacion { get; set; }
    public string? Motivo { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public string? MotivoNoPago { get; set; }
}

public class TotalEstadoPago
{
    public EstadoPago EstadoPago { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class MarcarApelacionRequest
{
    public List<int> OrdenIds { get; set; } = new();
    public bool Marcar { get; set; } = true;
}
