using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Domain.Entities;
namespace backend_claro.Application.Mappings;

public static class OrdenTrabajoMapper
{

    //para devolver el estado creado
    public static OrdenTrabajo ToEntity(this CrearOrdenPorTecnicoRequest request) // se usa cuando llega información del front y hay que crear/actualizar la BD.
    {
        return new OrdenTrabajo
        {
            Sot = request.Sot,
            TrabajoRealizado = request.TrabajoRealizado,
            EstadoCampo = request.EstadoCampo
        };
    }

    // ============ SALIDA: Entidad -> DTO ============
    public static OrdenResponse ToResponse(this OrdenTrabajo orden) // se usa cuando sales de la BD y hay que devolver al front.
    {
        return new OrdenResponse
        {
            OrdenId = orden.OrdenTrabajoId,
            Sot = orden.Sot,
            TrabajoRealizado = orden.TrabajoRealizado,
            EstadoCampo  = orden.EstadoCampo ,
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
            //Descripcion = orden.Descripcion,
            //Estado = orden.Estado,
            //PrecioTotal = orden.PrecioTotal,
            UsuarioId = orden.UsuarioId,
            Fecha = orden.FechaCreacion,
            NombreUsuario = orden.Usuario?.NombreCompleto ?? string.Empty,
            //Descuento = orden.Descuento,
            //SinPago = orden.SinPago,
            //ObservacionAuditoria = orden.ObservacionAuditoria,
            //FechaAuditoria = orden.FechaAuditoria,
            //EstadoPago = orden.EstadoPago,
            //MotivoNoPago = orden.MotivoNoPago,
            //ObservacionPago = orden.ObservacionPago,
            //Detalles = orden.Detalles.Select(d => d.ToResponse()).ToList(),
            Imagenes = orden.Archivos.Select(a => a.ToResponse()).ToList()
        };
    }

    // Un detalle suelto (para el endpoint AgregarDetalle)
    public static DetalleResponse ToResponse(this DetalleLiquidacion  detalle)
    {
        return new DetalleResponse
        {
            DetalleId = detalle.DetalleLiquidacionId,
            Concepto = detalle.Concepto,
            PrecioUnitario = detalle.PrecioUnitario,
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
    public static DetalleLiquidacion ToDetalleEntity(this CrearDetalleRequest request)
    {
        return new DetalleLiquidacion
        {
            Concepto = request.Concepto,
            PrecioUnitario = request.PrecioUnitario,
            Cantidad = request.Cantidad,
            PrecioTotal = request.PrecioUnitario * request.Cantidad,
            Tipo = request.Tipo,

        };

    }
}
