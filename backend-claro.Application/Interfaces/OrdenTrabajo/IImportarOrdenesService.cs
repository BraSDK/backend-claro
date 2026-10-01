using backend_claro.Application.DTOs.OrdenTrabajo;

namespace backend_claro.Application.Interfaces;

public interface IImportarOrdenesService
{
    // Lee el Excel (no lo guarda) y crea/actualiza las SOT con sus detalles
    Task<ImportarExcelResponse> ImportarExcelAsync(Stream archivo);
}
