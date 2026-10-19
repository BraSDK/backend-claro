using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Liquidaciones;

public class CrearLiquidacionRequest
{
    public int Sot {get; set;}
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;}
    public String? NombreTecnico {get; set;} = String.Empty;
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA;
    public ICollection<DetalleLiquidacion> Detalles {get; set;} = new List<DetalleLiquidacion>();
    public String? EstadoSot {get; set;} = String.Empty;
    public int? OrdenTrabajoId { get; set; } 
    public OrdenTrabajo? OrdenTrabajo { get; set; } 
        //Usuario que liquido la sot
    public int UsuarioId {get; set;}
    public Usuario Usuario { get; set; } = null!;
    public List<ObservacionLiquidacion> Observaciones = new List<ObservacionLiquidacion>();
}