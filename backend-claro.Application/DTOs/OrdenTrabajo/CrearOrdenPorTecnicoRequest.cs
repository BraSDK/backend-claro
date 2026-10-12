using backend_claro.Domain.Enums;
using backend_claro.Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

public class CrearOrdenPorTecnicoRequest
{
    public int Sot { get; set; }
    public string TrabajoRealizado { get; set; } = string.Empty;
    public int UsuarioId { get; set; }
    public Estados EstadoSistema { get; set; } = Estados.INGRESADA;
    public EstadoClaro EstadoCampo {get; set;} = EstadoClaro.COMPLETADO;
    public List<IFormFile> Imagenes { get; set; } = new();
}
