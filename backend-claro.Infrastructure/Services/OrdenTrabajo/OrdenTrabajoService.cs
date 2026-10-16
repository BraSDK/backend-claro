using backend_claro.Application.DTOs.OrdenTrabajo;
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

public partial class OrdenTrabajoService : IOrdenTrabajoService
{
    
    private readonly IApplicationDbContext _context;
    private readonly IFileStorageService _storage;

    // Inyectamos tu contexto de base de datos
    public OrdenTrabajoService(IApplicationDbContext context,IFileStorageService storage)
    {
        _context = context;
        _storage = storage;
    }

    // LISTAR OPERACIONES DE LISTAR: 


    // OPERACIONES DE CREAR
    public async Task<OrdenResponse> CrearPorAlmacenAsync(CrearOrdenRequest request)
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













    //

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
        ordenTrabajo.MarcarEditada();

        await _context.SaveChangesAsync();
       
    }

    public  async Task<DetalleResponse> AgregarDetalleAsync(int ordenId, CrearDetalleRequest request)
    {
        var  ordenTrabajo =await _context.Ordenes.Include( a => a.Detalles)
                                                 .FirstOrDefaultAsync( o => o.OrdenTrabajoId == ordenId ) ?? throw new NotFoundException("No se pudo encontrar orden de trabajao");
        // el precio viene en la petición (PrecioUnitario); el mapper ya calcula PrecioTotal = PrecioUnitario × Cantidad
        var detalleOrden = request.ToDetalleEntity();

        ordenTrabajo.Detalles.Add(detalleOrden);

        decimal Total = ordenTrabajo.Detalles.Sum( a => a.PrecioTotal );

        ordenTrabajo.PrecioTotal = Total;
        ordenTrabajo.MarcarEditada();

        await _context.SaveChangesAsync();

        return detalleOrden.ToResponse();
        
    }


    public async Task<OrdenDetalleResponse> EditarOrdenByTecnicoAsync(int ordenId, EditarOrdenRequest request)
    {
        //por si acaso
        var OrdenTrabajo = await _context.Ordenes.Include( o => o.Archivos)
                                                 .FirstOrDefaultAsync(a => a.OrdenTrabajoId == ordenId)??
                                                  throw new NotFoundException($"Orden {ordenId} no encontrada");
        if (request.TrabajoRealizado is not null)OrdenTrabajo.TrabajoRealizado = request.TrabajoRealizado;
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
        OrdenTrabajo.MarcarEditada();
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

        OrdenTrabajoEntity.MarcarEditada();
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
        OrdenTrabajo.MarcarEditada();

        await _context.SaveChangesAsync();
    }
    public async Task<PagedResponse<OrdenListaResponse>> ListarAsync(ListRequestOrdenesDto request, Rol rolUsuario, int cuentaId)
    {
        var query = _context.Ordenes.AsNoTracking();

        // Un técnico solo ve sus propias órdenes (su Usuario se ubica por la cuenta del token)
        if (rolUsuario == Rol.TECNICO)
        {
            var usuarioId = await _context.Usuarios
                .Where(u => u.AuthId == cuentaId)
                .Select(u => (int?)u.Id)
                .FirstOrDefaultAsync();
            query = query.Where(o => o.UsuarioId == usuarioId);
        }

        query = AplicarFiltros(query, request, filtrarTecnico: rolUsuario != Rol.TECNICO);

        // Sin resultados no es un error: se devuelve la página vacía
        var totalRegistros = await query.CountAsync();

        var listaOrdenesOrden = await query
            .OrderByDescending(o => o.FechaCreacion)
            .ThenByDescending(o => o.OrdenTrabajoId)
            .Skip((request.Pagina - 1) * request.CanPagina)
            .Take(request.CanPagina)
            .Select(ProyeccionLista)
            .ToListAsync();

        return new PagedResponse<OrdenListaResponse>
        {
            ListaOrdenes = listaOrdenesOrden,
            TotalRegistros = totalRegistros,
            TotalRegistrosPagina = listaOrdenesOrden.Count,
            TamanoPagina = request.CanPagina,
            NumeroPagina = request.Pagina
        };
    }

    // Auditoría de almacén: Sí pago / No pago.
    // Sí pago mantiene el descuento que tenga; No pago = multa (Claro paga S/ 0.00).
    public async Task<AuditoriaPagoResponse> AuditarPagoAsync(int ordenId, AuditoriaPagoRequest request, int cuentaId)
    {
        var orden = await _context.Ordenes.FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new KeyNotFoundException($"No se encontró la SOT con id {ordenId}.");

        var motivo = request.MotivoNoPago?.Trim();
        var observacion = string.IsNullOrWhiteSpace(request.ObservacionPago) ? null : request.ObservacionPago.Trim();

        switch (request.EstadoPago)
        {
            case EstadoPago.NoPago:
                if (string.IsNullOrEmpty(motivo))
                    throw new InvalidOperationException("Indica el motivo por el que no se paga la SOT.");
                orden.SinPago = true;
                orden.Descuento = orden.PrecioTotal ?? 0;
                orden.MotivoNoPago = motivo;
                break;

            case EstadoPago.SiPago:
                // si antes era multa, vuelve a pagarse completo; un descuento parcial se respeta
                if (orden.SinPago) orden.Descuento = 0;
                orden.SinPago = false;
                orden.MotivoNoPago = null;
                break;

            default:
                throw new InvalidOperationException("Elige Sí pago o No pago.");
        }

        orden.EstadoPago = request.EstadoPago;
        orden.ObservacionPago = observacion;
        orden.FechaAuditoria = DateTime.UtcNow;
        orden.AuditadoPorCuentaId = cuentaId;
        orden.MarcarLiquidada();   // la liquidación solo existe una vez auditada

        await _context.SaveChangesAsync();

        return new AuditoriaPagoResponse
        {
            OrdenId = orden.OrdenTrabajoId,
            EstadoPago = orden.EstadoPago,
            MotivoNoPago = orden.MotivoNoPago,
            ObservacionPago = orden.ObservacionPago,
            FechaAuditoria = orden.FechaAuditoria,
            MontoAPagar = orden.SinPago ? 0 : Math.Max(0, (orden.PrecioTotal ?? 0) - orden.Descuento),
        };
    }

    // Fila de la tabla de Órdenes (la usan el listado y la edición rápida, para que devuelvan lo mismo)
    private static readonly Expression<Func<OrdenTrabajo, OrdenListaResponse>> ProyeccionLista = a => new OrdenListaResponse
    {
        OrdenId = a.OrdenTrabajoId,
        Sot = a.Sot,
        Descripcion = a.TrabajoRealizado,
        Estado = a.EstadoSistema,
        Fecha = a.FechaCreacion,
        UsuarioId = a.UsuarioId,
        NombreUsuario = a.Usuario.NombreCompleto,
        PrecioTotal = a.PrecioTotal,
        MontoAPagar = a.SinPago ? 0 : (a.PrecioTotal ?? 0) - a.Descuento,
        Auditada = a.FechaAuditoria != null,
        EstadoPago = a.EstadoPago,
        MotivoNoPago = a.MotivoNoPago,
        CantidadImagenes = a.Archivos.Count,
        Costo = a.Detalles.Where(d => d.Tipo == TipoDetalle.SERVICIO).Sum(d => d.PrecioTotal),
        Adicional = a.Detalles.Where(d => d.Tipo == TipoDetalle.ADICIONAL).Sum(d => d.PrecioTotal),
        Drop150 = a.Detalles.Where(d => d.Tipo == TipoDetalle.DROP150).Sum(d => d.PrecioTotal),
        Drop200 = a.Detalles.Where(d => d.Tipo == TipoDetalle.DROP200).Sum(d => d.PrecioTotal),
        Drop300 = a.Detalles.Where(d => d.Tipo == TipoDetalle.DROP300).Sum(d => d.PrecioTotal),
    };

    // concepto por defecto cuando se crea una línea desde la tabla (mismos nombres que usa el importador)
    private static readonly Dictionary<TipoDetalle, string> ConceptoPorTipo = new()
    {
        [TipoDetalle.SERVICIO] = "SERVICIO",
        [TipoDetalle.ADICIONAL] = "ADICIONAL",
        [TipoDetalle.DROP150] = "DROP 150",
        [TipoDetalle.DROP200] = "DROP 220",
        [TipoDetalle.DROP300] = "DROP 300",
    };

    // Edición desde la tabla de Órdenes: un campo a la vez. Devuelve la fila actualizada.
    public async Task<OrdenListaResponse> EdicionRapidaAsync(int ordenId, EdicionRapidaRequest request)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new KeyNotFoundException($"No se encontró la orden {ordenId}");

        if (request.Descripcion is not null)
        {
            var texto = request.Descripcion.Trim();
            if (texto.Length > 500) throw new InvalidOperationException("Las observaciones no pueden superar los 500 caracteres.");
            orden.TrabajoRealizado = texto;
        }

        if (request.UsuarioId.HasValue && request.UsuarioId.Value != orden.UsuarioId)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Id == request.UsuarioId.Value);
            if (!existe) throw new InvalidOperationException("El técnico seleccionado no existe.");
            orden.UsuarioId = request.UsuarioId.Value;
        }

        if (request.TipoMonto.HasValue && request.Monto.HasValue)
            CambiarMonto(orden, request.TipoMonto.Value, request.Monto.Value);

        // un estado elegido a mano se respeta; cualquier otra edición pasa INGRESADA -> PROCESADO
        if (request.Estado.HasValue) orden.EstadoSistema = request.Estado.Value;
        else orden.MarcarEditada();

        await _context.SaveChangesAsync();

        return await _context.Ordenes.AsNoTracking()
            .Where(o => o.OrdenTrabajoId == ordenId)
            .Select(ProyeccionLista)
            .FirstAsync();
    }

    // Fija el monto de un tipo de línea: actualiza la línea existente, la crea o la quita (monto 0)
    private void CambiarMonto(OrdenTrabajo orden, TipoDetalle tipo, decimal monto)
    {
        if (monto < 0) throw new InvalidOperationException("El monto no puede ser negativo.");
        monto = Math.Round(monto, 2);

        var lineas = orden.Detalles.Where(d => d.Tipo == tipo).ToList();
        if (lineas.Count > 1)
            throw new InvalidOperationException($"La SOT tiene {lineas.Count} líneas de este tipo; edítalas desde \"Editar detalles\".");

        var linea = lineas.FirstOrDefault();
        if (monto == 0)
        {
            if (linea is not null)
            {
                orden.Detalles.Remove(linea);
                _context.Detalles.Remove(linea);
            }
        }
        else if (linea is null)
        {
            orden.Detalles.Add(new DetalleTrabajo
            {
                Concepto = ConceptoPorTipo[tipo],
                Tipo = tipo,
                Cantidad = 1,
                PrecioUnitario = monto,
                PrecioTotal = monto,
                OrdenTrabajoId = orden.OrdenTrabajoId,
            });
        }
        else
        {
            // se respeta la cantidad: el precio unitario se ajusta para que el total sea el monto ingresado
            linea.PrecioTotal = monto;
            linea.PrecioUnitario = linea.Cantidad > 0 ? Math.Round(monto / linea.Cantidad, 3) : monto;
            linea.FechaActualizacion = DateTime.UtcNow;
        }

        var nuevoTotal = orden.Detalles.Sum(d => d.PrecioTotal);
        if (orden.FechaAuditoria != null && !orden.SinPago && orden.Descuento > nuevoTotal)
            throw new InvalidOperationException($"El total (S/ {nuevoTotal:0.00}) quedaría por debajo del descuento auditado (S/ {orden.Descuento:0.00}). Ajusta primero la auditoría.");
        orden.PrecioTotal = nuevoTotal;
    }

    // Filtros del listado; también los usa la auditoría masiva para "Auditar todo"
    private static IQueryable<OrdenTrabajo> AplicarFiltros(IQueryable<OrdenTrabajo> query, ListRequestOrdenesDto request, bool filtrarTecnico)
    {
        if (filtrarTecnico && request.TecnicoId.HasValue)
            query = query.Where(o => o.UsuarioId == request.TecnicoId.Value);

        if (!string.IsNullOrWhiteSpace(request.Buscar))
        {
            var texto = request.Buscar.Trim();
            query = query.Where(o => o.Sot.ToString().Contains(texto));
        }

        if (request.Estado.HasValue)
            query = query.Where(o => o.EstadoSistema == request.Estado.Value);

        if (request.Auditada.HasValue)
            query = request.Auditada.Value
                ? query.Where(o => o.FechaAuditoria != null)
                : query.Where(o => o.FechaAuditoria == null);

        if (request.Desde.HasValue)
        {
            var desde = DateTime.SpecifyKind(request.Desde.Value.Date, DateTimeKind.Utc);
            query = query.Where(o => o.FechaCreacion >= desde);
        }

        if (request.Hasta.HasValue)
        {
            var hasta = DateTime.SpecifyKind(request.Hasta.Value.Date.AddDays(1), DateTimeKind.Utc);
            query = query.Where(o => o.FechaCreacion < hasta);
        }

        return query;
    }

    // Auditoría masiva sin descuento ni multa: solo toca las SOT que aún no están auditadas.
    // Con Confirmar = false solo cuenta (para mostrar la confirmación); con true las audita.
    public async Task<AuditarMasivoResponse> AuditarMasivoAsync(AuditarMasivoRequest request, int cuentaId)
    {
        IQueryable<OrdenTrabajo> query = _context.Ordenes;
        query = request.OrdenIds is { Count: > 0 }
            ? query.Where(o => request.OrdenIds.Contains(o.OrdenTrabajoId))          // las seleccionadas
            : AplicarFiltros(query, request.Filtros ?? new ListRequestOrdenesDto(), filtrarTecnico: true);   // "Auditar todo"

        var respuesta = new AuditarMasivoResponse
        {
            YaAuditadas = await query.CountAsync(o => o.FechaAuditoria != null),
        };

        var pendientes = query.Where(o => o.FechaAuditoria == null);
        respuesta.SinImagen = await pendientes.CountAsync(o => !o.Archivos.Any());

        if (!request.Confirmar)
        {
            respuesta.Auditadas = await pendientes.CountAsync();   // las que se auditarían
            return respuesta;
        }

        // Un solo UPDATE con "FechaAuditoria IS NULL" en el WHERE: la BD garantiza que nunca se pise
        // una SOT ya auditada, aunque la auditen a mano (p. ej. con multa) mientras corre esta operación.
        // Estado = LIQUIDADO es la misma regla de OrdenTrabajo.MarcarLiquidada().
        var ahora = DateTime.UtcNow;
        respuesta.Auditadas = await pendientes.ExecuteUpdateAsync(s => s
            .SetProperty(o => o.Descuento, 0m)
            .SetProperty(o => o.SinPago, false)
            .SetProperty(o => o.ObservacionAuditoria, (string?)null)
            .SetProperty(o => o.FechaAuditoria, ahora)
            .SetProperty(o => o.AuditadoPorCuentaId, cuentaId)
            .SetProperty(o => o.EstadoSistema, Estados.LIQUIDADO)
            .SetProperty(o => o.EstadoPago, EstadoPago.SiPago)
            .SetProperty(o => o.MotivoNoPago, (string?)null)
            .SetProperty(o => o.FechaActualizacion, ahora));
        return respuesta;
    }

public async Task<OrdenDetalleResponse> ObtenerPorIdAsync(int id, Rol rolUsuario)
{
    var orden = await _context.Ordenes
        .AsNoTracking()
        .Include(o => o.Archivos)
        .Include(o => o.Usuario)
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
            Descripcion = orden.TrabajoRealizado,
            Estado = orden.EstadoSistema,
            Detalles = orden.Detalles.Select(detalle => new DetalleResponse
            {
                DetalleId = detalle.DetalleTrabajoId,
                Concepto = detalle.Concepto,
                PrecioUnitario = detalle.PrecioUnitario,
                Cantidad = detalle.Cantidad,
                Tipo = detalle.Tipo,
                PrecioTotal = detalle.PrecioTotal
            }).ToList(),
            Imagenes = orden.Archivos.Select(MapearArchivo).ToList(),
            // Detalles y PrecioTotal quedan vacíos/default — el Técnico no los ve
        };
    }

    return orden.ToDetalleResponse(); // Admin/Almacén ven todo, tu extensión completa
}

    // Auditoría: se registra el descuento (o la multa) y su motivo. Se puede volver a auditar.
    public async Task<OrdenDetalleResponse> AuditarAsync(int ordenId, AuditarOrdenRequest request, int cuentaId)
    {
        var orden = await _context.Ordenes
            .Include(o => o.Archivos)
            .Include(o => o.Usuario)
            .Include(o => o.Detalles)
            .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
            ?? throw new KeyNotFoundException($"No se encontró la orden {ordenId}");

        var total = orden.PrecioTotal ?? 0;
        var observacion = request.Observacion?.Trim() ?? string.Empty;

        if (request.Descuento < 0)
            throw new InvalidOperationException("El descuento no puede ser negativo.");
        if (!request.SinPago && request.Descuento > total)
            throw new InvalidOperationException($"El descuento no puede ser mayor que el monto de la SOT (S/ {total:0.00}).");
        if ((request.SinPago || request.Descuento > 0) && observacion == string.Empty)
            throw new InvalidOperationException("Escribe en la observación el motivo del descuento o de la multa.");

        orden.SinPago = request.SinPago;
        orden.Descuento = request.SinPago ? total : Math.Round(request.Descuento, 2);
        orden.ObservacionAuditoria = observacion == string.Empty ? null : observacion;
        orden.FechaAuditoria = DateTime.UtcNow;
        orden.AuditadoPorCuentaId = cuentaId;
        orden.MarcarLiquidada(request.Estado);
        // estado de pago en sincronía: multa = No pago; si no, Sí pago (con el descuento que tenga)
        orden.EstadoPago = request.SinPago ? EstadoPago.NoPago : EstadoPago.SiPago;
        orden.MotivoNoPago = request.SinPago ? (orden.MotivoNoPago ?? "Otro") : null;

        await _context.SaveChangesAsync();
        return orden.ToDetalleResponse();
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
                AplicarCambio(item, detalleTemp);

            }
            else
            {
                ordenTrabajo.Detalles.Remove(item);
            }

        }
            ordenTrabajo.PrecioTotal = ordenTrabajo.Detalles.Sum(a => a.PrecioTotal);
            ordenTrabajo.MarcarEditada();

            await _context.SaveChangesAsync();
        
    }

    // Aplica los campos que vengan en la edición y recalcula el total con el precio del propio detalle
    // (nunca con el catálogo: el precio real es el que se registró, p. ej. el del Excel)
    private static void AplicarCambio(DetalleTrabajo detalle, DetalleEditar cambio)
    {
        if (cambio.Concepto is not null) detalle.Concepto = cambio.Concepto;
        if (cambio.PrecioUnitario.HasValue) detalle.PrecioUnitario = cambio.PrecioUnitario.Value;
        if (cambio.Cantidad.HasValue) detalle.Cantidad = cambio.Cantidad.Value;
        if (cambio.Tipo.HasValue) detalle.Tipo = cambio.Tipo.Value;
        detalle.PrecioTotal = detalle.PrecioUnitario * detalle.Cantidad;
        detalle.FechaActualizacion = DateTime.UtcNow;
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
                                        .Include(o => o.Archivos)
                                        .FirstOrDefaultAsync(o => o.OrdenTrabajoId == ordenId)
                                        ?? throw new NotFoundException($"Orden {ordenId} no encontrada");

                if (request.Sot.HasValue) orden.Sot = request.Sot.Value;
                if (request.Descripcion is not null) orden.TrabajoRealizado = request.Descripcion;
                if (request.Estado.HasValue) orden.EstadoSistema = request.Estado.Value;
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

                    AplicarCambio(detalle, cambio);
                }

                // ===== Detalles nuevos =====
                foreach (var nuevo in request.DetallesNuevos)
                {
                    var detalleNuevo = nuevo.ToDetalleEntity();   // Concepto, PrecioUnitario y PrecioTotal vienen de la petición
                    detalleNuevo.OrdenTrabajoId = ordenId;
                    orden.Detalles.Add(detalleNuevo);
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

                orden.PrecioTotal = orden.Detalles.Sum(d => d.PrecioTotal);
                orden.MarcarEditada();   // si quedó INGRESADA (o la eligieron así), al editar pasa a PROCESADO
                orden.FechaActualizacion = DateTime.UtcNow;

                await _context.SaveChangesAsync();
                return orden.ToDetalleResponse();

    }

    public async Task<int> EliminarOrdenesPorMesAsync(int anio, int mes)
    {
        if(mes < 1 || mes > 12)
            throw new ArgumentException("El mes debe estar entre 1 y 12");  
        // Usamos DateTimeKind.Utc si tu base de datos guarda fechas en UTC
        var fechaInicio = new DateTime(anio, mes, 1, 0, 0, 0, DateTimeKind.Utc);
        var fechaFin = fechaInicio.AddMonths(1);

        int registrosEliminados = await _context.Ordenes.Where(o => o.FechaCreacion >= fechaInicio && o.FechaCreacion < fechaFin).ExecuteDeleteAsync();
        return registrosEliminados;
    }

    private static ArchivoResponse MapearArchivo(OrdenTrabajoArchivo archivo) => new()
    {
        ArchivoId = archivo.ArchivoId,
        NombreArchivo = archivo.NombreArchivo,
        Src = archivo.Src
    };

}