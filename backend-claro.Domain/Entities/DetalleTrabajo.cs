using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_claro.Domain.Enums;
namespace backend_claro.Domain.Entities;

public class DetalleTrabajo
{
    [Key]
    public int DetalleTrabajoId {get; set;}

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;

    public int ServicioId {get;set;}
    public Servicio Servicio {get; set; } = null!;
    public int Cantidad {get; set; } = 0;

    [Column(TypeName = "decimal(18,2)")]
    public decimal PrecioTotal {get; set;}

    public int OrdenTrabajoId { get; set; } 
    public OrdenTrabajo OrdenTrabajo {get; set;} = null!;


    public TipoDetalle Tipo {get; set;} = TipoDetalle.DETALLE;
}
