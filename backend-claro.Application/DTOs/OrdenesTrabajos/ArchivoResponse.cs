namespace backend_claro.Application.DTOs;

public class ArchivoResponse
{
    public int ArchivoId { get; set; }
    public string NombreArchivo { get; set; } = string.Empty;
    public string Src { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
}
