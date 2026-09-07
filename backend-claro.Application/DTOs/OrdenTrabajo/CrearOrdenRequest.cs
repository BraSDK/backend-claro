using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Http;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class CrearOrdenRequest
{
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public Estados Estado { get; set; } = Estados.INGRESADA;
    public List<IFormFile> Imagenes { get; set; } = new();
}
