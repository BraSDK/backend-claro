using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Interfaces;
namespace backend_claro.Domain.Entities;

public class OrdenTrabajo : IAuditable
{
    public int OrdenTrabajoId {get; set;}

    [Required]
    public int Sot {get; set;}
    public string Descripcion {get;set;} = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioTotal {get; set;}

    public Estados Estado {get; set;} = Estados.INGRESADA;

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;
    public DateTime FechaActualizacion {get; set;} = DateTime.UtcNow;

    // ===== Auditoría (la llena almacén/admin al revisar la SOT) =====
    // Monto a pagar = SinPago ? 0 : PrecioTotal - Descuento
    [Column(TypeName = "decimal(18,2)")]
    public decimal Descuento {get; set;} = 0;
    public string? ObservacionAuditoria {get; set;}
    public bool SinPago {get; set;} = false;            // multa: Claro no paga nada por esta SOT
    public DateTime? FechaAuditoria {get; set;}         // null = todavía no auditada
    public int? AuditadoPorCuentaId {get; set;}

    // ===== Estado de pago (auditoría de almacén) =====
    public EstadoPago EstadoPago {get; set;} = EstadoPago.Pendiente;
    public string? MotivoNoPago {get; set;}             // obligatorio cuando EstadoPago = NoPago
    public string? ObservacionPago {get; set;}

    // ===== Conciliación: SOT incluida en la apelación a Claro =====
    public bool MarcadaApelacion {get; set;} = false;
    public DateTime? FechaApelacion {get; set;}

    //FK
    public int UsuarioId {get; set;}
    public Usuario Usuario {get; set;} = null!;
    public ICollection<DetalleTrabajo> Detalles {get; set;} = new List<DetalleTrabajo>();

    public ICollection<OrdenTrabajoArchivo> Archivos {get; set;} = new List<OrdenTrabajoArchivo>();

    // ===== Estados automáticos =====
    // INGRESADA -> PROCESADO al editarla (datos, detalles o imágenes). No baja una LIQUIDADA.
    public void MarcarEditada()
    {
        if (Estado == Estados.INGRESADA) Estado = Estados.PROCESADO;
    }

    // Al auditarla queda LIQUIDADA, salvo que se elija otro estado a mano
    public void MarcarLiquidada(Estados? estadoManual = null)
    {
        Estado = estadoManual ?? Estados.LIQUIDADO;
    }

}