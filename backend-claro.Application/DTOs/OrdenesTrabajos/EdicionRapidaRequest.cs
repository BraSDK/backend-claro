using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs;

// Edición de una orden de trabajo: se manda solo lo que cambia (lo que llega en null no se toca).
// Sirve tanto para editar una celda en la tabla como para guardar el formulario completo.
public class EdicionRapidaRequest
{
    public int? Sot { get; set; }
    public string? DocTecnico { get; set; }
    public string? TrabajoRealizado { get; set; }
    public EstadoClaro? EstadoCampo { get; set; }
    public int? UsuarioId { get; set; }   // técnico
}
