using backend_claro.Application.DTOs;
using backend_claro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using Microsoft.AspNetCore.Http;
using backend_claro.Application.Mappings;
using backend_claro.Domain.Exceptions;
using backend_claro.Application.DTOs;
using backend_claro.Domain.Enums;
using System.Linq.Expressions;
namespace backend_claro.Infrastructure.Services;

public class OrdenTrabajoService 
{
    
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    // Inyectamos tu contexto de base de datos
    public OrdenTrabajoService(IApplicationDbContext context,IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
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

    private static ArchivoResponse MapearArchivo(OrdenTrabajoArchivo archivo) => new()
    {
        ArchivoId = archivo.ArchivoId,
        NombreArchivo = archivo.NombreArchivo,
        Src = archivo.Src
    };

}