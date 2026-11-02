using backend_claro.Domain.Enums;
using backend_claro.Domain.Entities;
using backend_claro.Application.DTOs.Detalles;

namespace backend_claro.Application.DTOs.Liquidaciones;

public class ListarRequestLiquidacionesDto
{
    public int liquidacionId {get; set;}
    public int Sot {get; set;}
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;}
    public String? NombreTecnico {get; set;} = String.Empty;
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA;
    public List<DetalleResponseDto> DetallesLiquidacion { get; set; } = new();
           // vacío = nada eliminado
    public decimal? PrecioTotalEditable {get; set;} //para guardar lo del campo editado en el excel
    public decimal? PrecioTotalReal {get; set;}      // calculado por el sistema (suma de detalles)
    public decimal? MontoPagado {get; set;}          // lo que pagó Claro
    public EstadoPago EstadoPago {get; set;}
    public DateTime? FechaValidacion {get; set;}     // null = sin auditar
    public List<ObservacionResponseDto> Observaciones {get; set;} = new();
}

public class ObservacionResponseDto
{
    public int ObservacionLiquidacionId {get; set;}
    public TipoObservacion Tipo {get; set;}
    public string? Texto {get; set;}
}