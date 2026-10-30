using System.ComponentModel.DataAnnotations;
using backend_claro.Domain.Interfaces;

namespace backend_claro.Domain.Entities;

public class OrdenTrabajoArchivo : IAuditable
{
    [Key]
    public int ArchivoId {get; set;} 
    public string NombreArchivo {get; set;} = string.Empty;
    [Required]
    public string Src{get; set;} = string.Empty;

    public int OrdenTrabajoId { get; set; } 
    public OrdenTrabajo OrdenT {get; set;} = null!;
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;
}