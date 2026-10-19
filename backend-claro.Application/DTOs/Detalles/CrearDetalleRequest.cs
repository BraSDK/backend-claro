using backend_claro.Domain.Enums;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.DTOs.Detalles;

public class CrearDetalleRequest
{
     public int DetalleLiquidacionId {get; set;}

    // Qué adicional fue (para reportes). Null si es SERVICIO => precio base
    public int? CostoAdicionalId { get; set; }
    public CostoAdicional? CostoAdicional { get; set; }

    public string Concepto { get; set; } = string.Empty; // en servicio piede guardarse , pero en adicional si se guarda el nombre en sub tipo orden

    public decimal PrecioUnitario {get; set;} 
    public int Cantidad {get; set; } = 1;

    public decimal PrecioTotal {get; set;}

    public TipoDetalle Tipo {get; set;} 
    
}
