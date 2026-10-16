namespace backend_claro.Application.DTOs.Liquidacion;

public class ImportarExcelResponse 
{
    public int TotalFilasProcesadas { get; set; }
    public int TotalExitosas { get; set; }
    public int TotalConErrores { get; set; }
    public List<ErrorFilaImportacionDto> Errores { get; set; } = new ();
}

public class ErrorFilaImportacionDto
{
    public int Fila { get; set; }
    public long? Sot { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}