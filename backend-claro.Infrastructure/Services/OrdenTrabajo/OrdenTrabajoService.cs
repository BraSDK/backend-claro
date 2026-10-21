using backend_claro.Application.DTOs;
using backend_claro.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Exceptions;
using backend_claro.Domain.Enums;
using Microsoft.AspNetCore.Http;
using backend_claro.Application.Mappings.Ordenes;

namespace backend_claro.Infrastructure.Services;

public class OrdenTrabajoService : IOrdenTrabajoService
{
    
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

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

        var rutas = ordenTrabajo.Archivos.Select(a => a.Src).ToList();
        _context.Ordenes.Remove(ordenTrabajo);
        await _context.SaveChangesAsync();

        // los archivos del disco se borran después de confirmar la BD: si el guardado falla, no se pierden
        foreach (var ruta in rutas)
            await _storage.EliminarArchivo(ruta);
    }

    public async Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request, Rol rolUsuario, int cuentaId)
    {
        var query = _context.Ordenes.AsNoTracking();

        // un técnico solo ve sus propias órdenes (su Usuario se ubica por la cuenta del token);
        // admin y almacén ven todas y pueden filtrar por técnico
        if (rolUsuario == Rol.TECNICO)
        {
            var usuarioId = await _context.Usuarios
                .Where(u => u.AuthId == cuentaId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
            query = query.Where(o => o.UsuarioId == usuarioId);
        }

        query = AplicarFiltros(query, request, filtrarTecnico: rolUsuario != Rol.TECNICO);
        var totalRegistros = await query.CountAsync();
        var listaOrdenes = await  query.AsNoTracking()
                                                  .Include( o => o.Archivos)
                                                  .OrderByDescending(l=> l.FechaCreacion)
                                                  .ThenByDescending( l => l.Sot)
                                                  .Skip((request.Pagina-1) * request.CanPagina)
                                                  .Take(request.CanPagina)
                                                  .ToListAsync();

        return new PagedResponse<OrdenListaResponse>
        {
            ListaGenerica = listaOrdenes.Select( l => l.ToResponse()).ToList(),
            NumeroPagina = request.Pagina,
            TamanoPagina = request.CanPagina,
            TotalRegistros = totalRegistros,
            TotalRegistrosPagina = listaOrdenes.Count()
        };
        

    }

    private static IQueryable<OrdenTrabajo> AplicarFiltros(IQueryable<OrdenTrabajo> ordenes, ListRequestOrdenesDto request, bool filtrarTecnico)
    {

        if (!string.IsNullOrWhiteSpace(request.Buscar))
        {
           var texto = request.Buscar.Trim();
           ordenes = ordenes.Where( o => o.Sot.ToString().Contains(texto));
        } 
        if(filtrarTecnico && request.TecnicoId.HasValue) ordenes = ordenes.Where( o => o.UsuarioId == request.TecnicoId);
        if(request.Desde.HasValue) 
        {
            var desde= DateTime.SpecifyKind(request.Desde.Value.Date, DateTimeKind.Utc);
            ordenes = ordenes.Where(o => o.FechaCreacion >= desde);
        }
        if(request.Hasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(request.Hasta.Value.Date.AddDays(1), DateTimeKind.Utc);
            ordenes = ordenes.Where( o => o.FechaCreacion < hasta);   // hasta = día siguiente 00:00 -> incluye todo el último día
        }
        return ordenes;
    }

    // Crea la orden y la devuelve (con su Id, para poder subirle las imágenes a continuación)
    public async Task<OrdenListaResponse> CrearOrdenTrabajoAsync(CrearOrdenTrabajoRequest request, Rol rolUsuario, int cuentaId)
    {
        if (request.Sot <= 0)
            throw new InvalidOperationException("La SOT debe ser un número mayor a 0.");

        // un técnico solo puede registrar órdenes a su propio nombre: no se confía en el UsuarioId que mande el front
        if (rolUsuario == Rol.TECNICO)
        {
            request.UsuarioId = await _context.Usuarios
                .Where(u => u.AuthId == cuentaId)
                .Select(u => u.Id)
                .FirstOrDefaultAsync();
        }

        if (await _context.Ordenes.AnyAsync(o => o.Sot == request.Sot))
            throw new InvalidOperationException("Ya existe esta orden de trabajo, edítela.");
        if (!await _context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId))
            throw new InvalidOperationException("El técnico no existe.");
        var orden = request.ToEntity();
        _context.Ordenes.Add(orden);
        await _context.SaveChangesAsync();
        return orden.ToResponse();
    }

    // Editar: solo se aplican los campos que llegan (null = no cambia). Devuelve la orden actualizada.
    public async Task<OrdenListaResponse> EdicionRapidaAsync(int ordenId, EdicionRapidaRequest request)
    {
        if (request.Sot is null && request.DocTecnico is null && request.TrabajoRealizado is null
            && request.EstadoCampo is null && request.UsuarioId is null)
            throw new InvalidOperationException("No se envió ningún cambio.");

        var orden = await _context.Ordenes
            .Include(o => o.Archivos)
            .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new NotFoundException("Orden de trabajo no encontrada");

        if (request.Sot.HasValue && request.Sot.Value != orden.Sot)
        {
            if (request.Sot.Value <= 0)
                throw new InvalidOperationException("La SOT debe ser un número mayor a 0.");
            if (await _context.Ordenes.AnyAsync(o => o.Sot == request.Sot.Value && o.OrdenTrabajoId != ordenId))
                throw new InvalidOperationException($"Ya existe otra orden con la SOT {request.Sot.Value}.");
            orden.Sot = request.Sot.Value;
        }

        if (request.DocTecnico is not null)
            orden.DocTecnico = request.DocTecnico.Trim();

        if (request.TrabajoRealizado is not null)
        {
            var trabajo = request.TrabajoRealizado.Trim();
            if (trabajo.Length > 150)
                throw new InvalidOperationException("El trabajo realizado no puede superar los 150 caracteres.");
            orden.TrabajoRealizado = trabajo;
        }

        if (request.EstadoCampo.HasValue)
        {
            if (!Enum.IsDefined(request.EstadoCampo.Value))
                throw new InvalidOperationException("El estado no es válido.");
            orden.EstadoCampo = request.EstadoCampo.Value;
        }

        if (request.UsuarioId.HasValue && request.UsuarioId.Value != orden.UsuarioId)
        {
            if (!await _context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId.Value))
                throw new InvalidOperationException("El técnico seleccionado no existe.");
            orden.UsuarioId = request.UsuarioId.Value;
        }

        await _context.SaveChangesAsync();   // FechaActualizacion la pone el SaveChanges del DbContext (IAuditable)
        return orden.ToResponse();
    }

    public async Task<List<ArchivoResponse>> AgregarArchivosAsync(int ordenId, List<IFormFile> archivos)
    {
        var orden = await _context.Ordenes.Include( o => o.Archivos).FirstOrDefaultAsync( o => o.OrdenTrabajoId == ordenId) ?? throw new NotFoundException("Orden de trabajo no encontrada");
        
        foreach (var item in archivos)
        {
            orden.AgregarArchivo(new OrdenTrabajoArchivo
            {
                NombreArchivo = item.FileName,
                Src =  await _storage.GestionarArchivo(item,"uploads"),
            });
        }
        await _context.SaveChangesAsync();

        return orden.Archivos.Select( o => o.ToResponse()).ToList();
    }

    public async Task EliminarArchivoAsync(int ordenId, int archivoId)
    {
        var orden = await _context.Ordenes.Include(o => o.Archivos)
            .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new NotFoundException("Orden de trabajo no encontrada");
        var archivo = orden.Archivos.FirstOrDefault(a => a.ArchivoId == archivoId)
            ?? throw new NotFoundException("Archivo no encontrado");

        _context.Archivos.Remove(archivo);
        await _context.SaveChangesAsync();
        await _storage.EliminarArchivo(archivo.Src); 
    }
    
}