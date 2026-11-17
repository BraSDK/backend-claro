using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;

public class ValidarImgRequest
{
    public string? Observacion {get; set;} 
    public EstadoAlmacen EstadoAlmacen {get; set;} 
}