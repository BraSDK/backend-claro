using backend_claro.Domain.Interfaces;
using backend_claro.Domain.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
namespace backend_claro.Domain.Entities;

public class Liquidacion : IAuditable
{
    //Data general de la sot
    public int LiquidacionId { get; set; }

    [Required]
    public long Sot { get; set; }
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;} = DateTime.UtcNow;
    public String NombreTecnico {get; set;} = String.Empty;
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA; //EStado en este sistema

    //Precios
    public ICollection<DetalleLiquidacion> Detalles {get; set;} = new List<DetalleLiquidacion>();

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioTotalReal {get; set;} //para guardar uno calculado todo lo liquidado
        
    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioTotalEditable {get; set;} //para guardar lo del campo editado en el excel

    public String? EstadoSot {get; set;} = String.Empty;

    // Relación OPCIONAL con lo que registró el técnico (para ver sus fotos)
    public int? OrdenTrabajoId { get; set; }
    public OrdenTrabajo? OrdenTrabajo { get; set; }

    //Usuario que liquido la sot
    public int UsuarioId {get; set;}
    public Usuario Usuario { get; set; } = null!;
    public EstadoPago EstadoPago {get; set;} = EstadoPago.Pendiente;

    // Se activa cuando editan la celda, trabajo con precioTotal Editable
    [Column(TypeName = "decimal(18,2)")]
    public decimal?  Descuento {get; set;} = 0;

    //Estados de la validacion
    public bool SinPago {get; set;} = false; // Informacion si pago o no
    public DateTime? FechaValidacion {get; set;}  // null = todavía no validada or gerencia
    public int? AuditadoPorCuentaId {get; set;} //usuario admin que lo validó
    //OBSERVACIONES

    public ICollection<ObservacionLiquidacion> Observaciones {get; set;} = new List<ObservacionLiquidacion>();

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

}