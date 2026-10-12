using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore.Internal;

namespace backend_claro.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    DbSet<Usuario> Usuarios { get; set; }
    DbSet<Servicio> Servicios { get; set; }
    DbSet<OrdenTrabajo> Ordenes {get; set;}
    DbSet<OrdenTrabajoArchivo> Archivos {get; set;}

    DbSet<Liquidacion> Liquidaciones {get;set;}
    DbSet<DetalleLiquidacion> Detalles {get; set;}
    DbSet<MotivoObservacion> motivos {get;set;} // catalogo de motivos para el autocompletado
    DbSet<ObservacionLiquidacion> Observaciones {get;set;}
    DbSet<CostoAdicional> Adicionales {get; set;}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}