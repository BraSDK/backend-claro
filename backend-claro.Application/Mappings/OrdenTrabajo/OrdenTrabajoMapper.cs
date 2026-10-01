using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.Mappings;

public static class OrdenTrabajoMapper
{

    
    public static OrdenTrabajo ToEntity(this CrearOrdenRequest request)
    {
        return new OrdenTrabajo
        {
            Sot = request.Sot,
            Descripcion = request.Descripcion,
            Estado = request.Estado
        };
    }

    // ============ SALIDA: Entidad -> DTO ============
    public static OrdenResponse ToResponse(this OrdenTrabajo orden)
    {
        return new OrdenResponse
        {
            OrdenId = orden.OrdenTrabajoId,
            Sot = orden.Sot,
            Descripcion = orden.Descripcion,
            Estado = orden.Estado,
            UsuarioId = orden.UsuarioId,
            Imagenes = orden.Archivos.Select(a => a.ToResponse()).ToList()
        };
    }

    // Orden con detalles e imagenes (para el detalle completo)
    public static OrdenDetalleResponse ToDetalleResponse(this OrdenTrabajo orden)
    {
        return new OrdenDetalleResponse
        {
            OrdenId = orden.OrdenTrabajoId,
            Sot = orden.Sot,
            Descripcion = orden.Descripcion,
            Estado = orden.Estado,
            PrecioTotal = orden.PrecioTotal,
            UsuarioId = orden.UsuarioId,
            Fecha = orden.FechaCreacion,
            NombreUsuario = orden.Usuario?.NombreCompleto ?? string.Empty,
            Descuento = orden.Descuento,
            SinPago = orden.SinPago,
            ObservacionAuditoria = orden.ObservacionAuditoria,
            FechaAuditoria = orden.FechaAuditoria,
            EstadoPago = orden.EstadoPago,
            MotivoNoPago = orden.MotivoNoPago,
            ObservacionPago = orden.ObservacionPago,
            Detalles = orden.Detalles.Select(d => d.ToResponse()).ToList(),
            Imagenes = orden.Archivos.Select(a => a.ToResponse()).ToList()
        };
    }

    // Un detalle suelto (para el endpoint AgregarDetalle)
    public static DetalleResponse ToResponse(this DetalleTrabajo detalle)
    {
        return new DetalleResponse
        {
            DetalleId = detalle.DetalleTrabajoId,
            ServicioId = detalle.ServicioCodigo,
            NombreServicio = detalle.Servicio?.Nombre ?? string.Empty,
            Cantidad = detalle.Cantidad,
            PrecioTotal = detalle.PrecioTotal,
            Tipo = detalle.Tipo
        };
    }

    // Un archivo suelto (para el endpoint AgregarArchivos)
    public static ArchivoResponse ToResponse(this OrdenTrabajoArchivo archivo)
    {
        return new ArchivoResponse
        {
            ArchivoId = archivo.ArchivoId,
            NombreArchivo = archivo.NombreArchivo,
            Src = archivo.Src
        };
    }
    public static DetalleTrabajo ToDetalleEntity(this CrearDetalleRequest request)
    {
        return new DetalleTrabajo
        {
               
            ServicioCodigo = request.ServicioId,
            Cantidad = request.Cantidad, 
            Tipo = request.Tipo,

        };

    }
}
