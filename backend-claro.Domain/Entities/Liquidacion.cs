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
    [Column(TypeName = "timestamp without time zone")]
    public DateTime FechaProgramacion {get; set;}
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

    // Se activa cuando editan la celda precioTotalReal con precioTotal Editable
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