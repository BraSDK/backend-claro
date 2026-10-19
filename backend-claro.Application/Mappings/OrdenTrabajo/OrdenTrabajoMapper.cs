using backend_claro.Application.DTOs;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.Mappings.Ordenes;

public static class OrdenTrabajoMapper
{
    public static ArchivoResponse ToResponse(this OrdenTrabajoArchivo archivo)
    {
        return new ArchivoResponse
        {
            ArchivoId = archivo.ArchivoId,
            NombreArchivo = archivo.NombreArchivo,
            Src = archivo.Src
        };
    }
}
