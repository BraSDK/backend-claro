using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Domain.Enums;
public class EditarOrdenCompletaRequest
{
    public int? Sot { get; set; }             
    public string? Descripcion { get; set; }    
    public Estados? Estado { get; set; }        

    public List<CrearDetalleRequest> DetallesNuevos { get; set; } = new();   // vacío = nada nuevo
    public List<DetalleEditar> DetallesEditados { get; set; } = new();       // vacío = nada editado
    public List<int> DetallesEliminados { get; set; } = new();               // vacío = nada eliminado

    public List<int> ArchivosEliminados { get; set; } = new();               // vacío = nada eliminado
}