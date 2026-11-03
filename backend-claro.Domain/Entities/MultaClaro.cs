using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_claro.Domain.Interfaces;

namespace backend_claro.Domain.Entities;

// Multa general de Claro (no va ligada a una SOT). Se descuenta en el pago (lote) donde se incluya.
public class MultaClaro : IAuditable
{
    public int MultaClaroId { get; set; }

    public DateTime Fecha { get; set; }

    [MaxLength(200)]
    public string Motivo { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Monto { get; set; }

    [MaxLength(150)]
    public string? NombreTecnico { get; set; }   // opcional: a quién se le atribuye

    public int? PagoClaroId { get; set; }        // null = todavía no se descontó en ningún pago
    public PagoClaro? PagoClaro { get; set; }

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}
