using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.Dashboard;

// Resumen de un mes para el dashboard (las fechas se toman de FechaCreacion de la SOT)
public class DashboardResponse
{
    public int Anio { get; set; }
    public int Mes { get; set; }

    public int TotalSots { get; set; }
    public decimal MontoTotal { get; set; }
    public decimal MontoAPagar { get; set; }          // lo que Claro paga de verdad: total - descuentos - multas
    public int SotsAuditadas { get; set; }
    public int SotsSinImagen { get; set; }
    public int TotalSotsMesAnterior { get; set; }
    public decimal MontoTotalMesAnterior { get; set; }

    public List<EstadoResumen> PorEstado { get; set; } = new();
    public List<TecnicoResumen> PorTecnico { get; set; } = new();
    public List<ServicioResumen> PorServicio { get; set; } = new();   // solo el servicio principal (Tipo SERVICIO)
    public List<TipoDetalleResumen> PorTipoDetalle { get; set; } = new(); // servicio, adicional y cada drop
    public List<OrdenListaResponseDashboard> UltimasSots { get; set; } = new();
}

public class EstadoResumen
{
    public Estados Estado { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class TecnicoResumen
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class ServicioResumen
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class TipoDetalleResumen
{
    public TipoDetalle Tipo { get; set; }
    public int Cantidad { get; set; }
    public decimal Monto { get; set; }
}

public class OrdenListaResponseDashboard
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public DateTime Fecha { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public decimal? PrecioTotal { get; set; }
}

public class TecnicoOpcion
{
    public int UsuarioId { get; set; }
    public string Nombre { get; set; } = string.Empty;
}
