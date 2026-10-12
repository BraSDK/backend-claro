using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Interfaces;
namespace backend_claro.Domain.Entities;

public class DetalleLiquidacion:IAuditable
{
    [Key]
    public int DetalleLiquidacionId {get; set;}
    //dueño del detalle
    public int LiquidacionId { get; set; } 
    public Liquidacion Liquidacion {get; set;} = null!;

    // Qué adicional fue (para reportes). Null si es SERVICIO.
    public int? CostoAdicionalId { get; set; }
    public CostoAdicional? CostoAdicional { get; set; }
    [Required, MaxLength(150)]
    public string Concepto { get; set; } = string.Empty; // en servicio piede guardarse , pero en adicional si se guarda el nombre en sub tipo orden

    [Column(TypeName = "decimal(18,3)")]
    public decimal PrecioUnitario {get; set;} 
    public int Cantidad {get; set; } = 1;

    [Column(TypeName = "decimal(18,3)")]
    public decimal PrecioTotal {get; set;}

    public TipoDetalle Tipo {get; set;} 
    
    public DateTime FechaActualizacion { get ; set; } = DateTime.UtcNow;
    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;
}
