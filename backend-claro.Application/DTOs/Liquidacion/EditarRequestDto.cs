using backend_claro.Application.DTOs.Detalles;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Entities;
using backend_claro.Application.DTOs.Observaciones;

namespace backend_claro.Application.DTOs.Liquidaciones;

public class EditarLiquidacionRequestDto
{
    public int LiquidacionId {get; set;}
    public int Sot {get; set;}
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;}
    public String? NombreTecnico {get; set;} = String.Empty;
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA;
    public List<CrearDetalleRequest> DetallesNuevos { get; set; } = new(); // vacío = nada editado nada nuevo
    public List<EditarDetalleRequest> DetallesEditados { get; set; } = new();       // vacío = nada editado
    public List<int> DetallesEliminados { get; set; } = new();               // vacío = nada eliminado
    public decimal? PrecioTotalEditable {get; set;} //para guardar lo del campo editado en el excel
    public String? EstadoSot {get; set;} = String.Empty;
    public EstadoPago EstadoPago {get; set;} = EstadoPago.Pendiente;
    public List<CrearObservacionDto> ObservacionesNuevas {get; set;} = new List<CrearObservacionDto>();
    public List<EditarObservacionDto> ObservacionesEditadas {get; set;} = new List<EditarObservacionDto>();
    public List<int> ObservacionesEliminadas {get; set;} = new List<int>();
}