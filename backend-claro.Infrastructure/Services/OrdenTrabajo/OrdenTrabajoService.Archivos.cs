using backend_claro.Application.DTOs.OrdenTrabajo;
using backend_claro.Application.Interfaces;
using backend_claro.Application.Mappings;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Enums;
using backend_claro.Domain.Exceptions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace backend_claro.Infrastructure.Services;

public partial class OrdenTrabajoService
{
    public async Task<OrdenResponse> CrearPorTecnicoAsync(CrearOrdenPorTecnicoRequest request)
    {
        var OTexiste = await _context.Ordenes.AnyAsync(o => o.Sot == request.Sot);

        if (OTexiste)
        {
            throw new Exception($"Esta Orden de trabajo con Sot {request.Sot} ya existe");
        }

        var UserCreator = await _context.Usuarios.FindAsync(request.UsuarioId);

        if (UserCreator == null)
        {
            throw new Exception("No se puede guardar Orden de trabajo sin Usuario creador");
        }

        var OrdenTrabajoEntity = request.ToEntity();
        OrdenTrabajoEntity.Usuario = UserCreator;
        
        // Cambiamos 'FormFile' por 'IFormFile' para evitar el error de casting en tiempo de ejecución
        foreach (IFormFile img in request.Imagenes)
        {
            if (img.Length > 0)
            {
                OrdenTrabajoEntity.Archivos.Add(new OrdenTrabajoArchivo
                {
                    NombreArchivo = img.FileName,
                    Src = await _storage.GestionarArchivo(img, "ordenes")
                });
            }
        }

        await _context.Ordenes.AddAsync(OrdenTrabajoEntity);
        await _context.SaveChangesAsync();

        return OrdenTrabajoEntity.ToResponse();
    }
}