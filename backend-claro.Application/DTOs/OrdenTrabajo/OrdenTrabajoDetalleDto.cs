using backend_claro.Domain.Enums;
namespace backend_claro.Application.DTOs;

public class OrdenTrabajoDetalleDto
{
    public int Cantidad {get; set;}
    public decimal PrecioTotal {get; set;}
    public TipoDetalle Tipo {get; set;} = TipoDetalle.DETALLE;

}