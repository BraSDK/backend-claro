using System.ComponentModel.DataAnnotations;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Interfaces;
namespace backend_claro.Domain.Entities;
public class MotivoObservacion : IAuditable
{
    public int MotivoObservacionId { get; set; }

    public TipoObservacion Tipo { get; set; }

    [Required, MaxLength(150)]
    public string Descripcion { get; set; } = string.Empty;

    public bool Activo { get; set; } = true;          // se desactiva, nunca se borra

    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}