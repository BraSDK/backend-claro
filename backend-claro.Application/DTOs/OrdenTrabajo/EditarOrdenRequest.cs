using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Los detalles y las imagenes se editan por sus propios endpoints granulares.
public class EditarOrdenRequest
{
    public int? Sot { get; set; }
    public string?Descripcion { get; set; } 
    public int? UsuarioId {get; set;}
    public List<int>? ArchivosEliminados { get; set; } 
    public List<IFormFile>? ArchivosNuevos { get; set; } 
    

}
