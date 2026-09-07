using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using System.Data.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using backend_claro.Application.DTOs;
using backend_claro.Application;
using backend_claro.Application.Mappings;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.AspNetCore.Mvc.Abstractions;
using System.IO.Compression;
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




    public Task EliminarDetalleAsync(int ordenId, int detalleId)
    {
        var  ordenTrabajo =  _context.Ordenes.FirstOrDefault(o =>  o.OrdenTrabajoId == ordenId );
        if(ordenTrabajo is null)
        {
            throw new InvalidOperationException(" Este detalle n puede ser elimnado");
        }
        var detalle = ordenTrabajo.Detalles.FirstOrDefault(a => a.DetalleTrabajoId == detalleId);

        if(detalle is not null)
        {
             _context.Detalles.Remove(detalle);
        }

        _context.SaveChangesAsync();

        return Task.CompletedTask;

       
    }




    public  Task<DetalleResponse> AgregarDetalleAsync(int ordenId, CrearDetalleRequest request)
    {
        var  ordenTrabajo =  _context.Ordenes.Find( ordenId ) ?? throw new InvalidOperationException("No se pudo encontrar orden de trabajao");
        var detalleOrden = request.ToDetalleEntity();
        var Servicio = _context.Servicios.FirstOrDefault(s => s.Codigo == detalleOrden.ServicioId);

        if(Servicio is null)
        {
            throw new InvalidOperationException("El servicio no existe");            
        }

        detalleOrden.PrecioTotal = Servicio.Precio * detalleOrden.Cantidad;
        
        ordenTrabajo.Detalles.Add(detalleOrden);

        decimal Total = ordenTrabajo.Detalles.Sum( a => a.PrecioTotal );

        ordenTrabajo.PrecioTotal = Total;

        _context.SaveChangesAsync();

        return Task.FromResult(detalleOrden.ToResponse());
        
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
    public async Task<OrdenDetalleResponse> EditarAsync(int ordenId, EditarOrdenRequest request)
    {
        //por si acaso
        var OrdenTrabajo = await _context.Ordenes.FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId);
        if(OrdenTrabajo is null)
        {
            throw new KeyNotFoundException("No se encontraron órdenes de trabajo ára editar.");
        }
        OrdenTrabajo.Descripcion = request.Descripcion;
        OrdenTrabajo.Estado = request.Estado;
        OrdenTrabajo.Sot = request.Sot;
        
        var OrdenTrabajoActualizada = _context.Ordenes.Update(OrdenTrabajo);
        var rows = _context.SaveChangesAsync();

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


    // Retorna para tablas
    public async Task<List<OrdenListaResponse>> ListarAsync()
    {
        var listaOrdenes = _context.Ordenes.AsNoTracking()
                              .Select(a => new OrdenListaResponse
                              {
                                  OrdenId = a.OrdenTrabajoId,
                                  Sot = a.Sot,
                                  Descripcion = a.Descripcion,
                                  Estado = a.Estado
                              }).ToList();

        return listaOrdenes;
    }

    public async Task<OrdenDetalleResponse> ObtenerPorIdAsync(int id)
    {
        var OrdenDetalleRespuesta = await _context.Ordenes.AsNoTracking()
                                                          .Include(a => a.Archivos)
                                                          .Include(d => d.Detalles)
                                                          .FirstOrDefaultAsync(a => a.OrdenTrabajoId == id);
        if(OrdenDetalleRespuesta is null)
        {
            throw new KeyNotFoundException($"No se encontró la orden con ID {id}");
        }

        return OrdenDetalleRespuesta.ToDetalleResponse();                 
                                                          
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

    public async Task EditarDetallesAsunc(int ordenId, List<DetalleEditar> requestList)
    {
        var ordenTrabajo = await _context.Ordenes.Include(a => a.Detalles)
                                           .FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId) ?? throw new InvalidOperationException("No se pudo encontrar");

    

        foreach (var item in ordenTrabajo.Detalles.ToList())
        {
            var detalleTemp = requestList.Find(a => a.DetalleId == item.DetalleTrabajoId);
            if (detalleTemp is not null)
            {
                item.Cantidad = detalleTemp.Cantidad;
                item.Tipo = detalleTemp.Tipo;
                item.ServicioId = detalleTemp.ServicioId;
                var ServicioConsulta = _context.Servicios.Find( detalleTemp.ServicioId) ?? throw new InvalidOperationException("No existe tal servicio");
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

}