using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Dashboard;

// Resumen del mes que pinta la página Dashboard del front (mismos nombres que dashboard.types.ts)
public class DashboardResumenDto
{
    public int Anio { get; set; }
    public int Mes { get; set; }
    public int TotalSots { get; set; }
    public decimal MontoTotal { get; set; }       // lo calculado por el sistema (PrecioTotalReal)
    public decimal MontoAPagar { get; set; }      // lo que Claro paga tras la auditoría
    public int SotsAuditadas { get; set; }
    public int SotsSinImagen { get; set; }        // liquidaciones sin fotos del técnico
    public int TotalSotsMesAnterior { get; set; }
    public decimal MontoTotalMesAnterior { get; set; }
    public List<EstadoResumenDto> PorEstado { get; set; } = new();
    public List<TecnicoResumenDto> PorTecnico { get; set; } = new();
    public List<ServicioResumenDto> PorServicio { get; set; } = new();
    public List<TipoDetalleResumenDto> PorTipoDetalle { get; set; } = new();
    public List<UltimaSotDto> UltimasSots { get; set; } = new();
}

public class EstadoResumenDto
{
    public Estados Estado { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class TecnicoResumenDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class ServicioResumenDto
{
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class TipoDetalleResumenDto
{
    public TipoDetalle Tipo { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class UltimaSotDto
{
    public int OrdenId { get; set; }
    public long Sot { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public decimal? PrecioTotal { get; set; }
}

public class TecnicoOpcionDto
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
