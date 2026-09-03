namespace backend_claro.Application.DTOs.Service;

public class ServiceResponseDto
{
    public int Codigo { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public string Categoria { get; set; } = string.Empty;
}