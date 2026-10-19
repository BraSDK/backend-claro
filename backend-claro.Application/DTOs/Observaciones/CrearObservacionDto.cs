using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
namespace backend_claro.Application.DTOs.Observaciones;

public class CrearObservacionDto
{
    public int LiquidacionId { get; set; }
    public Liquidacion Liquidacion { get; set; } = null!;
    public TipoObservacion Tipo { get; set; } // lo asigna el sistema, no el usuario
    public int? MotivoObservacionId { get; set; } // opcional: elegido del autocompletado
    public MotivoObservacion? Motivo { get; set; }
    public string? TextoObservacion { get; set; } // opcional: lo que escribió libre
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

}