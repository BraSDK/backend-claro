using System.ComponentModel.DataAnnotations;

namespace backend_claro.Domain.Entities;

public class OrdenTrabajoArchivo
{

    [Key]
    public int ArchivoId {get; set;} 
    public string NombreArchivo {get; set;} = string.Empty;
    [Required]
    public string Src{get; set;} = string.Empty;

    public int OrdenTrabajoId { get; set; } 
    public OrdenTrabajo OrdenT {get; set;} = null!;

}