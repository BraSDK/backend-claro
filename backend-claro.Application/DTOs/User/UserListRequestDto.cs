using backend_claro.Domain.Enums;

namespace backend_claro.Application.DTOs.User;

public class UserListResponseDto
{
    public string BuscarNombreCompleto { get; set; } = string.Empty;
    public Rol? Rol { get; set; }
    public int Pagina { get; set; } = 1;
    public int CantidadPorPagina { get; set; } = 10;
}