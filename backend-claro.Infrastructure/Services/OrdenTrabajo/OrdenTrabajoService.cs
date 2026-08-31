using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using System.Data.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Http;
using backend_claro.Application.DTOs;
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
    public Task<string> AsyncEdit(OrdenTrabajoDto request)
    {
        throw new NotImplementedException();
    }

    public async Task<List<ListViewDto>> AsyncListar()
    {
        

        var ordenes = await _context.Ordenes.AsNoTracking().Select( a => new ListViewDto {
            Sot = a.Sot, 
            Descripcion = a.Descripcion,
            Estado = a.Estado
            }).ToListAsync();

        if(ordenes.Count < 1)
        {
            throw new InvalidProgramException("No se pudo traer ninguna Orden de trabajo porque no hay ninguna");
        }

        
        return ordenes;
    }
            
        

    

    public async Task<OrdenTrabajo> AsyncRegister(OrdenTrabajoDto request)
    {
        var OTexiste = await _context.Ordenes.AnyAsync(o =>  o.Sot == request.Sot);

        if (OTexiste)
        {
            throw new Exception($"Esta Orden de trabajo con Sot {request.Sot} ya existe");
        }

        var UserCreator = await _context.Usuarios.FindAsync(request.UsuarioDto);

        if (UserCreator == null)
        {
            throw new Exception("No se puede guardar Orden de trabajo sin Usuario creador");
        }

        OrdenTrabajo ordenTrabajo = new OrdenTrabajo
        {
            Sot = request.Sot,
            Descripcion = request.Descripcion,
            Estado = request.EstadoOt,
            Usuario = UserCreator,

        };
        //creamos el objeto para procesar la imagen
        foreach (FormFile img in request.Imagenes)
        {
            ordenTrabajo.Archivos.Add(new OrdenTrabajoArchivo
            {
                NombreArchivo = img.FileName,
                Src = await _storage.GestionarArchivo(img,"ordenes")
            }
            );
        }

        // creamos los archivos;
        

        var newOrdenTrabajo = await _context.Ordenes.AddAsync(ordenTrabajo);
        
        int filasAfectadas = await _context.SaveChangesAsync();

        return ordenTrabajo;
    }

    public Task<string> AsyncUpdate(OrdenTrabajoDto request)
    {
        throw new NotImplementedException();
    }
}