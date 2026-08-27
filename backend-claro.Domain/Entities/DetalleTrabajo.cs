namespace backend_claro.Domain.Entities;

public class DetalleTrabajo
{
    public enum TipoDetalle
    {
        SERVICIO = 0,
        DETALLE = 1,
        DROP = 2,
    }
    public int DetalleTrabajoId {get; set;}

    public OrdenTrabajo OrdenTrabajo {get; set;} = null!;

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;

    public Servicio Servicio {get; set; } = null!;
    public int Cantidad {get; set; } = 0;
    public decimal PrecioTotal {get; set;} 

    public TipoDetalle Tipo {get; set;} = TipoDetalle.DETALLE;
}
