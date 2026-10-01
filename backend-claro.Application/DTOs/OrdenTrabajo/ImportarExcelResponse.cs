namespace backend_claro.Application.DTOs.OrdenTrabajo;

// Resumen de la carga masiva de SOTs desde Excel
public class ImportarExcelResponse
{
    public int OrdenesCreadas { get; set; }
    public int OrdenesActualizadas { get; set; }      // SOT existentes a las que se reemplazaron los detalles
    public List<string> ServiciosCreados { get; set; } = new();
    public List<ErrorFilaImportacion> Errores { get; set; } = new();
}

public class ErrorFilaImportacion
{
    public int Fila { get; set; }       // número de fila en el Excel (la cabecera es la 1)
    public int? Sot { get; set; }
    public string Mensaje { get; set; } = string.Empty;
}
