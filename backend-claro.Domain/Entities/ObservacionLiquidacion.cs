using System.ComponentModel.DataAnnotations;
using backend_claro.Domain.Enums;
namespace backend_claro.Domain.Entities;
public class ObservacionLiquidacion
{
    public int ObservacionLiquidacionId { get; set; }

    public int LiquidacionId { get; set; }
    public Liquidacion Liquidacion { get; set; } = null!;

    public TipoObservacion Tipo { get; set; }          // lo asigna el sistema, no el usuario

    public int? MotivoObservacionId { get; set; }      // opcional: elegido del autocompletado
    public MotivoObservacion? Motivo { get; set; }

    [MaxLength(500)]
    public string? Texto { get; set; }                 // opcional: lo que escribió libre

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime Fecha { get; set; } = DateTime.UtcNow;
}