using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using backend_claro.Application.Mappings;
using backend_claro.Domain.Exceptions;
using backend_claro.Application.DTOs;
using backend_claro.Domain.Enums;
namespace backend_claro.Infrastructure.Services;

public class OrdenTrabajoService : IOrdenTrabajoService
{
    
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    // Inyectamos tu contexto de base de datos
    public OrdenTrabajoService(IApplicationDbContext context,IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    public async Task EliminarDetalleAsync(int ordenId, int detalleId)
    {
        var  ordenTrabajo = await _context.Ordenes.Include( o => o.Detalles)
                                                  .FirstOrDefaultAsync(o =>  o.OrdenTrabajoId == ordenId );
        if(ordenTrabajo is null)
        {
            throw new NotFoundException(" No existe orden de trabajo con tal detalle");
        }
        var detalle = ordenTrabajo.Detalles.FirstOrDefault(a => a.DetalleTrabajoId == detalleId) ?? throw new NotFoundException("No se pudo encontrar detalle a elmiminar");
        
        _context.Detalles.Remove(detalle);
        

        await _context.SaveChangesAsync();
       
    }

    public  async Task<DetalleResponse> AgregarDetalleAsync(int ordenId, CrearDetalleRequest request)
    {
        var  ordenTrabajo =await _context.Ordenes.Include( a => a.Detalles)
                                                 .FirstOrDefaultAsync( o => o.OrdenTrabajoId == ordenId ) ?? throw new NotFoundException("No se pudo encontrar orden de trabajao");
        var detalleOrden = request.ToDetalleEntity();
        var Servicio =await _context.Servicios.FirstOrDefaultAsync(s => s.Codigo == detalleOrden.ServicioCodigo);
        /*
        if(ordenTrabajo.Estado == Estados.LIQUIDADO)
        {
            throw new InvalidOperationException(" No se puede editar una orden de Trabajo liquidada");
        }
        */

        if(Servicio is null)
        {
            throw new NotFoundException("El servicio no existe");            
        }

        
        detalleOrden.PrecioTotal = Servicio.Precio * detalleOrden.Cantidad;
        
        ordenTrabajo.Detalles.Add(detalleOrden);

        decimal Total = ordenTrabajo.Detalles.Sum( a => a.PrecioTotal );

        ordenTrabajo.PrecioTotal = Total;

        await _context.SaveChangesAsync();

        return detalleOrden.ToResponse();
        
    }

    public async Task<OrdenResponse> CrearAsync(CrearOrdenRequest request)
    {
        var OTexiste = await _context.Ordenes.AnyAsync(o =>  o.Sot == request.Sot);

        if (OTexiste)
        {
            throw new Exception($"Esta Orden de trabajo con Sot {request.Sot} ya existe");
        }

        var UserCreator = await _context.Usuarios.FindAsync(request.UsuarioId);

        if (UserCreator == null)
        {
            throw new Exception("No se puede guardar Orden de trabajo sin Usuario creador");
        }

        var OrdenTrabajoEntity =request.ToEntity();
        OrdenTrabajoEntity.Usuario = UserCreator;
        foreach (FormFile img in request.Imagenes)
        {
            OrdenTrabajoEntity.Archivos.Add(new OrdenTrabajoArchivo
            {
                NombreArchivo = img.FileName,
                Src = await _storage.GestionarArchivo(img,"ordenes")
            }
            );
        }

        var newOrdenTrabajo = await _context.Ordenes.AddAsync(OrdenTrabajoEntity);
        
        int filasAfectadas = await _context.SaveChangesAsync();
         

        return OrdenTrabajoEntity.ToResponse();
    }
    public async Task<OrdenDetalleResponse> EditarOrdenByTecnicoAsync(int ordenId, EditarOrdenRequest request)
    {
        //por si acaso
        var OrdenTrabajo = await _context.Ordenes.Include( o => o.Archivos)
                                                 .FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId)??
                                                  throw new NotFoundException($"Orden {ordenId} no encontrada");
        if (request.Descripcion is not null)OrdenTrabajo.Descripcion = request.Descripcion;
        if (request.Sot.HasValue) OrdenTrabajo.Sot = request.Sot.Value;
        var rutasABorrar = new List<string>();
        if(request.ArchivosEliminados is { Count: > 0})
        {
                foreach (var archivoId in request.ArchivosEliminados)
                {
                    var archivo = OrdenTrabajo.Archivos.FirstOrDefault(a => a.ArchivoId == archivoId)
                        ?? throw new NotFoundException($"Archivo {archivoId} no pertenece a esta orden");

                    rutasABorrar.Add(archivo.Src);
                    OrdenTrabajo.Archivos.Remove(archivo);
                    _context.Archivos.Remove(archivo);
                }
        }

        
        if (request.ArchivosNuevos is { Count: > 0 })
        {
            foreach (var archivo in request.ArchivosNuevos)
            {
                if (archivo.Length == 0) continue;

                var url = await _storage.GestionarArchivo(archivo,"ordenes");
                OrdenTrabajo.Archivos.Add(new OrdenTrabajoArchivo
                {
                    NombreArchivo = archivo.FileName,
                    Src = url,
                    OrdenTrabajoId = ordenId
                });
            }
        }
        await _context.SaveChangesAsync();

        foreach (var ruta in rutasABorrar)
            await _storage.EliminarArchivo(ruta);

        return OrdenTrabajo.ToDetalleResponse();

    }



    public async Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos)
    {
        var OrdenTrabajoEntity = await _context.Ordenes.FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId);
        if(OrdenTrabajoEntity is null)
        {
            throw new InvalidOperationException("No existe esa orden de trabajo");
        }

        foreach(FormFile img in archivos)
        {
            OrdenTrabajoEntity.Archivos.Add(new OrdenTrabajoArchivo
            {
                Src =await _storage.GestionarArchivo(img,"ordenes"),
                NombreArchivo = img.FileName
            });

        }

        await _context.SaveChangesAsync();
        var resultado = OrdenTrabajoEntity.Archivos.Select(a => new ArchivoResponse
        {
            ArchivoId = a.ArchivoId,
            Src = a.Src,
            NombreArchivo = a.NombreArchivo
        }).ToList();

        return  resultado;

    }

    public async Task EliminarArchivoAsync(int ordenId, int archivoId)
    {
        var OrdenTrabajo = await _context.Ordenes.Include(a => a.Archivos)
                                                 .FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId);

        if(OrdenTrabajo is null)
        {
            throw new KeyNotFoundException($"No existe o Orden de trabajo para esta imagen");
        }

        var ArchivoTrabajo = OrdenTrabajo.Archivos.FirstOrDefault(a => a.ArchivoId == archivoId);
        if(ArchivoTrabajo is null){
            throw new KeyNotFoundException("No existe este archivo o ya fue eliminado");
        }
        
        string src = ArchivoTrabajo.Src;

        string rutaEliminada =await _storage.EliminarArchivo(src);

        _context.Archivos.Remove(ArchivoTrabajo);

        OrdenTrabajo.Archivos.Remove(ArchivoTrabajo);
                
    }
    public async Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request)
    {
        var query = _context.Ordenes.AsNoTracking();

        if (request.FechaCreacion == default) 
        {
          query = query.Where( o => o.FechaCreacion > request.FechaCreacion);
        }
        else
        {
            query = query.Where( o => o.FechaCreacion > request.FechaCreacion);
        }
        

        var totalRegistros  =await query.CountAsync();
        if(totalRegistros == 0)
        {
            throw new NotFoundException("Sin registros encontrados");
        }

        var listaOrdenesOrden =await query.Skip((request.Pagina-1)*request.CanPagina)
                                     .Take(request.CanPagina)
                                     .Select(a => new OrdenListaResponse
                                     {
                                        OrdenId = a.OrdenTrabajoId,
                                        Sot = a.Sot,
                                        Descripcion = a.Descripcion,
                                        Estado = a.Estado
                                     }).ToListAsync();
                                      
        int registrosEnEstaPagina = listaOrdenesOrden.Count;                  

        return new PagedResponse<OrdenListaResponse>
        {
            ListaOrdenes = listaOrdenesOrden,
            TotalRegistros = totalRegistros,
            TotalRegistrosPagina = registrosEnEstaPagina,
            NumeroPagina = request.Pagina
        };
    }

public async Task<OrdenDetalleResponse> ObtenerPorIdAsync(int id, Rol rolUsuario)
{
    var orden = await _context.Ordenes
        .AsNoTracking()
        .Include(o => o.Archivos)
        .Include(o => o.Detalles)
        .FirstOrDefaultAsync(o => o.OrdenTrabajoId == id)
        ?? throw new NotFoundException($"No se encontró la orden {id}");

    if (rolUsuario == Rol.TECNICO)
    {
        return new OrdenDetalleResponse
        {
            OrdenId = orden.OrdenTrabajoId,
            UsuarioId = orden.UsuarioId,
            Sot = orden.Sot,
            Descripcion = orden.Descripcion,
            Estado = orden.Estado,
            Imagenes = orden.Archivos.Select(MapearArchivo).ToList(),
            // Detalles y PrecioTotal quedan vacíos/default — el Técnico no los ve
        };
    }

    return orden.ToDetalleResponse(); // Admin/Almacén ven todo, tu extensión completa
}


    public async Task<OrdenDetalleResponse> ObtenerPorSotAsync(int sot)
    {
        var OrdenDetalleRespuesta = await _context.Ordenes.AsNoTracking()
                                                          .Include(a => a.Archivos)
                                                          .Include(d => d.Detalles)
                                                          .FirstOrDefaultAsync(a => a.OrdenTrabajoId == sot);
        if(OrdenDetalleRespuesta is null)
        {
            throw new KeyNotFoundException($"No se encontró la orden con ID {sot}");
        }

        return OrdenDetalleRespuesta.ToDetalleResponse();   
    }

    public async Task EditarDetallesAsync(int ordenId, List<DetalleEditar> requestList)
    {
        var ordenTrabajo = await _context.Ordenes.Include(a => a.Detalles)
                                           .FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId) ?? throw new InvalidOperationException("No se pudo encontrar");

    

        foreach (var item in ordenTrabajo.Detalles.ToList())
        {
            var detalleTemp = requestList.Find(a => a.DetalleId == item.DetalleTrabajoId);
            if (detalleTemp is not null)
            {
                item.Cantidad = detalleTemp.Cantidad ?? item.Cantidad;
                item.Tipo = detalleTemp.Tipo ?? item.Tipo;
                item.ServicioCodigo = detalleTemp.ServicioCodigo ?? item.ServicioCodigo;
                var ServicioConsulta = _context.Servicios.Find( detalleTemp.ServicioCodigo) ?? throw new InvalidOperationException("No existe tal servicio");
                item.PrecioTotal = item.Cantidad * ServicioConsulta.Precio;

            }
            else
            {
                ordenTrabajo.Detalles.Remove(item);
            }

        }
            ordenTrabajo.PrecioTotal = ordenTrabajo.Detalles.Sum(a => a.PrecioTotal);

            await _context.SaveChangesAsync();
        
    }

    public async Task EliminarOrdenAsync(int ordenId)
    {
        var ordenTrabajo = await _context.Ordenes.Include( o => o.Archivos)
            .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new NotFoundException("Orden de trabajo no encontrada");   
        Console.WriteLine(ordenTrabajo.Archivos + "Eliminado");
        foreach (var item in ordenTrabajo.Archivos)
        {
           Console.WriteLine(item.Src + "Eliminado");
           await _storage.EliminarArchivo(item.Src);
        }
        _context.Ordenes.Remove(ordenTrabajo);
        await _context.SaveChangesAsync();
    }

    public async Task<OrdenDetalleResponse> EditarCompletoAsync(int ordenId, EditarOrdenCompletaRequest request, Rol rolUsuario)
    {

                var orden = await _context.Ordenes
                                        .Include(o => o.Detalles)
                                            .ThenInclude(d => d.Servicio)
                                        .Include(o => o.Archivos)
                                        .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
                                        ?? throw new NotFoundException($"Orden {ordenId} no encontrada");

                if (request.Sot.HasValue) orden.Sot = request.Sot.Value;
                if (request.Descripcion is not null) orden.Descripcion = request.Descripcion;
                if (request.Estado.HasValue) orden.Estado = request.Estado.Value;
                var tocaDetalles = request.DetallesNuevos.Any()
                    || request.DetallesEditados.Any()
                    || request.DetallesEliminados.Any();

                foreach (var detalleId in request.DetallesEliminados)
                {
                    var detalle = orden.Detalles.FirstOrDefault(d => d.DetalleTrabajoId == detalleId)
                        ?? throw new NotFoundException($"Detalle {detalleId} no pertenece a esta orden");

                    orden.Detalles.Remove(detalle);
                    _context.Detalles.Remove(detalle);
                }
                //detalles ditados
                foreach (var cambio in request.DetallesEditados)
                {
                    var detalle = orden.Detalles.FirstOrDefault(d => d.DetalleTrabajoId == cambio.DetalleId)
                        ?? throw new NotFoundException($"Detalle {cambio.DetalleId} no pertenece a esta orden");

                    var huboCambioDeServicio = cambio.ServicioCodigo.HasValue && cambio.ServicioCodigo.Value != detalle.ServicioCodigo;
                    var huboCambioDeCantidad = cambio.Cantidad.HasValue && cambio.Cantidad.Value != detalle.Cantidad;

                    if (huboCambioDeServicio)
                    {
                        var nuevoServicio = await _context.Servicios.FindAsync(cambio.ServicioCodigo!.Value)
                            ?? throw new NotFoundException($"Servicio {cambio.ServicioCodigo} no encontrado");

                        detalle.ServicioCodigo = nuevoServicio.Codigo;
                        detalle.Servicio = nuevoServicio;
                    }

                    if (cambio.Cantidad.HasValue) detalle.Cantidad = cambio.Cantidad.Value;
                    if (cambio.Tipo.HasValue) detalle.Tipo = cambio.Tipo.Value;

                    if (huboCambioDeServicio || huboCambioDeCantidad)
                        detalle.PrecioTotal = detalle.Servicio.Precio * detalle.Cantidad;

                    detalle.FechaActualizacion = DateTime.UtcNow;
                }

                // ===== Detalles nuevos =====
                foreach (var nuevo in request.DetallesNuevos)
                {
                    var servicio = await _context.Servicios.FindAsync(nuevo.ServicioId)
                        ?? throw new NotFoundException($"Servicio {nuevo.ServicioId} no encontrado");

                    orden.Detalles.Add(new DetalleTrabajo
                    {
                        ServicioCodigo = servicio.Codigo,
                        Servicio = servicio,
                        Cantidad = nuevo.Cantidad,
                        PrecioTotal = servicio.Precio * nuevo.Cantidad,
                        Tipo = nuevo.Tipo,
                        OrdenTrabajoId = ordenId
                    });
                }

                // ===== Archivos eliminados =====
                foreach (var archivoId in request.ArchivosEliminados)
                {
                    var archivo = orden.Archivos.FirstOrDefault(a => a.ArchivoId == archivoId)
                        ?? throw new NotFoundException($"Archivo {archivoId} no pertenece a esta orden");

                    await _storage.EliminarArchivo(archivo.Src);
                    orden.Archivos.Remove(archivo);
                    _context.Archivos.Remove(archivo);
                }

                orden.PrecioTotal = _context.Detalles.Sum( o => o.PrecioTotal);
                orden.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return orden.ToDetalleResponse();

    }

    private static ArchivoResponse MapearArchivo(OrdenTrabajoArchivo archivo) => new()
    {
        ArchivoId = archivo.ArchivoId,
        NombreArchivo = archivo.NombreArchivo,
        Src = archivo.Src
    };
}