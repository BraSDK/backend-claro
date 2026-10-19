using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Liquidaciones;

public class ResponseLiquidacionRequestDto
{
    public int Sot {get; set;}
    public String? TipoOrden {get; set;}
    public String NombreSubTipoOrden {get; set;} = String.Empty;
    public DateTime FechaProgramacion {get; set;}
    public String? NombreTecnico {get; set;} = String.Empty;
    public Estados EstadoSistema {get; set;} = Estados.INGRESADA;
    public List<DetalleLiquidacion> Detalles {get; set;} = new List<DetalleLiquidacion>();
    public decimal? PrecioTotalEditable {get; set;} //para guardar lo del campo editado en el excel
}