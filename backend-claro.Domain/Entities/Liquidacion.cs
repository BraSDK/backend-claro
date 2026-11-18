using backend_claro.Domain.Interfaces;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Entities;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Collections.ObjectModel;
using backend_claro.Domain.Exceptions;
using System.Globalization;
namespace backend_claro.Domain.Entities;

public class Liquidacion : IAuditable
{
    //Data general de la sot
    public int LiquidacionId { get; set; }

    [Required]
    public long Sot { get; set; }
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;}
    public String? NombreTecnico {get; set;} = String.Empty;
    // DNI del técnico (DocumentoIdentidad de su usuario), igual que OrdenTrabajo.DocTecnico.
    // Lo pone el back cuando el nombre coincide con un técnico registrado; null si es un nombre libre.
    [MaxLength(20)]
    public String? DocTecnico {get; set;}
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA; //EStado en este sistema

    //Precios

    //codigo de servicio que se liquidó
    public int? ServicioCodigo {get; set;}
    public Servicio? Servicio {get; set;} //relacion con el servicio
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
    public int? UsuarioId {get; set;}
    public Usuario? Usuario { get; set; } = null!;
    public EstadoPago EstadoPago {get; set;} = EstadoPago.Pendiente;

    // Lo que Claro pagó por la SOT (del Excel de pagos o escrito a mano en la auditoría)
    [Column(TypeName = "decimal(18,2)")]
    public decimal? MontoPagado {get; set;}

    // Total que se cobra: el editado si lo hay, si no el calculado por el sistema
    public decimal TotalACobrar => PrecioTotalEditable ?? PrecioTotalReal ?? 0;

    // Se activa cuando editan la celda precioTotalReal con precioTotal Editable
    [Column(TypeName = "decimal(18,2)")]
    public decimal?  Descuento {get; set;} = 0;

    //Estados de la validacion
    public bool SinPago {get; set;} = false; // Informacion si pago o no
    public DateTime? FechaValidacion {get; set;}  // null = todavía no validada or gerencia
    public int? AuditadoPorCuentaId {get; set;} //usuario admin que lo validó

    [MaxLength(10)]
    public string? MesPago { get; set; }      // Ej: "202611"

    [MaxLength(10)]
    public string? SemanaPago { get; set; }   // Ej: "S1", "S2", "S3", "S4"
    public List<string> HistorialPeriodos { get; set; } = new();

    // Pago de Claro (lote) en el que se registró esta SOT. null = auditada pero todavía sin pago registrado
    public int? PagoClaroId {get; set;}
    public PagoClaro? PagoClaro {get; set;}

    // Hoja (mes) a la que pertenece: la asigna el back según FechaCreacion, el front no la manda
    public int HojaId {get; set;}
    public Hoja Hoja {get; set;} = null!;

    //OBSERVACIONES

    public ICollection<ObservacionLiquidacion> Observaciones {get; set;} = new List<ObservacionLiquidacion>();

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
    
    public void RecalcularPrecioTotalReal()
    {
        PrecioTotalReal = Detalles.Sum(d => d.PrecioTotal);
    }

    public void AgregarDetalles(DetalleLiquidacion detalle)
    {
        Detalles.Add(detalle);
        RecalcularPrecioTotalReal();
    }

    public void EliminarDetalle(int detalleId)
    {
        var detalle = Detalles.FirstOrDefault(d => d.DetalleLiquidacionId == detalleId)
            ?? throw new NotFoundException($"No existe el detalle {detalleId} en esta liquidación");

        Detalles.Remove(detalle);
        RecalcularPrecioTotalReal();
    }

    // Con código de servicio, el costo de servicio es un detalle con el código y el precio del tarifario
    // (se crea si la grilla no lo mandó). Sin código, se queda el costo escrito a mano, sin vínculo.
    public void SincronizarDetalleServicio(Servicio? servicio)
    {
        var detalle = Detalles.FirstOrDefault(d => d.EsCostoDeServicio);

        if (servicio is null)
        {
            detalle?.QuitarServicio();
        }
        else
        {
            if (detalle is null)
            {
                detalle = new DetalleLiquidacion();
                Detalles.Add(detalle);
            }
            detalle.AsignarServicio(servicio);
        }

        RecalcularPrecioTotalReal();
    }

    public void InsertarDiferencia()
    {
        if(PrecioTotalEditable == PrecioTotalReal)
        {
            Descuento = 0;
            return;
        }

        Descuento = PrecioTotalReal - PrecioTotalEditable;

    }

    // Liquidacion 
    public void AgregarObservacion(ObservacionLiquidacion observacion)
    {
        Observaciones.Add(observacion);
    }

    public void EditarObservacion(int id, int? motivoObservacionId, string? texto)
    {
        var observacion = Observaciones.FirstOrDefault(o => o.ObservacionLiquidacionId == id) ?? throw new NotFoundException("Observación no encontrada");
        observacion.EditarObservacion(motivoObservacionId, texto);
    }

    public void EliminarObservacion(int id)
    {
        var observacionElimnar = Observaciones.FirstOrDefault(o => o.ObservacionLiquidacionId == id) ?? throw new NotFoundException("No encontrado");
        Observaciones.Remove(observacionElimnar);
    }


}