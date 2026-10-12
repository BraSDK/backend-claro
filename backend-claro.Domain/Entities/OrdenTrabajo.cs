using System.ComponentModel.DataAnnotations;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Interfaces;
namespace backend_claro.Domain.Entities;

public class OrdenTrabajo : IAuditable
{
    public int OrdenTrabajoId {get; set;}

    [Required]
    public int Sot {get; set;}

    public string DocTecnico {get; set;} = string.Empty;

    [MaxLength(150)] 
    public string TrabajoRealizado {get;set;} = string.Empty; //Nombre del trabajo realizado

    public EstadoClaro EstadoCampo {get; set;} = EstadoClaro.COMPLETADO; //estado en sitema oracle de claro - cuando el tecnico lo sube es practicamente completado.
    public ICollection<OrdenTrabajoArchivo> Archivos {get; set;} = new List<OrdenTrabajoArchivo>(); // archivos

    //FK
    public int UsuarioId {get; set;}
    public Usuario Usuario {get; set;} = null!;

    public DateTime FechaCreacion {get; set;} = DateTime.UtcNow;
    public DateTime FechaActualizacion {get; set;} = DateTime.UtcNow;


}