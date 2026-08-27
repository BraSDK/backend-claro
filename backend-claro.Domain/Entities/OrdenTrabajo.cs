using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace backend_claro.Domain.Entities;

public class OrdenTrabajo
{
    public int OrdenTrabajoId {get; set;}

    [Required]
    public int Sot {get; set;}
    public string Descripcion {get;set;} = string.Empty;

    [Column(TypeName = "decimal(6,3)")]
    public decimal? PrecioTotal {get; set;}
    public bool? Estado {get; set;}

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;

    //FK
    public Usuario Usuario {get; set;} = null!;

}