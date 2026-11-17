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

    // Qué adicional fue (para reportes). Null si es SERVICIO => precio base
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

    // Servicio del tarifario (solo el detalle de tipo SERVICIO)
    public int? CodigoServicio {get; set;}
    public Servicio? ServicioDetalle {get; set;}

    // Código del tarifario con el que se cobró: el del servicio o el del adicional (DECO, DROP...).
    // Se copia al guardar, igual que el precio, para que la liquidación no cambie si luego cambia el tarifario.
    public int? CodigoTarifario {get; set;}

    public DateTime FechaActualizacion { get ; set; } = DateTime.UtcNow;
    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;

    public void EditarDetalle( CostoAdicional? costoAdicional = null, string? concepto = null, decimal? precioUnitario= null, int? cantidad= null)
    {
        if(costoAdicional is not null)
        {
            CostoAdicionalId = costoAdicional.AdicionalId;
            Tipo = TipoDetalle.ADICIONAL;
            Concepto = costoAdicional.Nombre;
            PrecioUnitario = costoAdicional.Precio;
            CodigoTarifario = costoAdicional.Codigo;
            if (cantidad.HasValue) Cantidad = cantidad.Value;
            CalcularPrecioTotal();
            return;
        }
        
        if(concepto is not null) this.Concepto = concepto;
        if (precioUnitario.HasValue) PrecioUnitario = precioUnitario.Value;
        if (cantidad.HasValue) Cantidad = cantidad.Value;
        Tipo = TipoDetalle.SERVICIO;
        CalcularPrecioTotal();
    }


    // El costo de servicio sale del tarifario: código y precio del servicio, cantidad 1
    public void AsignarServicio(Servicio servicio)
    {
        Tipo = TipoDetalle.SERVICIO;
        Concepto = ConceptoServicio;
        CodigoServicio = servicio.Codigo;
        CodigoTarifario = servicio.Codigo;
        PrecioUnitario = servicio.Precio;
        Cantidad = 1;
        CalcularPrecioTotal();
    }

    // Sin código de servicio el costo es el que escribieron a mano: se quita el vínculo con el tarifario
    public void QuitarServicio()
    {
        CodigoServicio = null;
        CodigoTarifario = null;
    }

    public const string ConceptoServicio = "COSTO DE SERVICIO";

    public bool EsCostoDeServicio =>
        Tipo == TipoDetalle.SERVICIO && string.Equals(Concepto.Trim(), ConceptoServicio, StringComparison.OrdinalIgnoreCase);

    public void CalcularPrecioTotal()
    {
        this.PrecioTotal = this.Cantidad * this.PrecioUnitario;
    }
}
