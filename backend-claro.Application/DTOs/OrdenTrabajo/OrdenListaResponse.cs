using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;


public class OrdenListaResponse
{
    public int OrdenId { get; set; }
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Estados Estado { get; set; }
    public DateTime Fecha { get; set; }
    public int UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public decimal? PrecioTotal { get; set; }
    public decimal MontoAPagar { get; set; }      // PrecioTotal - Descuento (0 si es multa)
    public bool Auditada { get; set; }
    public EstadoPago EstadoPago { get; set; }
    public string? MotivoNoPago { get; set; }
    public int CantidadImagenes { get; set; }
}
