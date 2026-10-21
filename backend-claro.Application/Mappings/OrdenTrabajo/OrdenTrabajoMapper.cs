using backend_claro.Application.DTOs;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.Mappings.Ordenes;

public static class OrdenTrabajoMapper
{
    public static OrdenTrabajo ToEntity (this CrearOrdenTrabajoRequest orden)
    {
        return new OrdenTrabajo
        {
            Sot =  orden.Sot,
            DocTecnico = orden.DocTecnico,
            TrabajoRealizado = orden.TrabajoRealizado,
            UsuarioId = orden.UsuarioId,
            EstadoCampo = orden.EstadoCampo,
            Archivos = orden.Archivos.Select( a => a.ToEntity()).ToList(),
        };
    }
    public static OrdenListaResponse ToResponse (this OrdenTrabajo orden)
    {
        return new OrdenListaResponse
        {
            Sot = orden.Sot,
            DocTecnico = orden.DocTecnico,
            EstadoCampo = orden.EstadoCampo,
            TrabajoRealizado = orden.TrabajoRealizado,
            OrdenTrabajoId = orden.OrdenTrabajoId,
            UsuarioId  = orden.UsuarioId,
            Archivos = orden.Archivos.Select( a => a.ToResponse()).ToList()
        };
    }
    
    public static ArchivoResponse ToResponse(this OrdenTrabajoArchivo archivo)
    {
        return new ArchivoResponse
        {
            ArchivoId = archivo.ArchivoId,
            NombreArchivo = archivo.NombreArchivo,
            Src = archivo.Src
        };
    }

    public static OrdenTrabajoArchivo ToEntity(this CrearArchivoRequest archivo)
    {
        return new OrdenTrabajoArchivo
        {
            NombreArchivo = archivo.NombreArchivo,
            Src = archivo.Src,
            OrdenTrabajoId = archivo.OrdenTrabajoId
        };
    }
}
