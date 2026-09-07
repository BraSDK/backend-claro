using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Los detalles y las imagenes se editan por sus propios endpoints granulares.
public class EditarOrdenRequest
{
    public int Sot { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public Estados Estado { get; set; }
}
