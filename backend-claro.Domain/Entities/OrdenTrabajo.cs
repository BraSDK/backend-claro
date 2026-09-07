using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using backend_claro.Domain.Enums;

namespace backend_claro.Domain.Entities;

public class OrdenTrabajo
{
    public int OrdenTrabajoId {get; set;}

    [Required]
    public int Sot {get; set;}
    public string Descripcion {get;set;} = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal? PrecioTotal {get; set;}

    public Estados Estado {get; set;} = Estados.INGRESADA;

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;

    //FK
    public int UsuarioId {get; set;}
    public Usuario Usuario {get; set;} = null!;
    public ICollection<DetalleTrabajo> Detalles {get; set;} = new List<DetalleTrabajo>();

    public ICollection<OrdenTrabajoArchivo> Archivos {get; set;} = new List<OrdenTrabajoArchivo>();

}