using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    DbSet<Usuario> Usuarios { get; set; }
    DbSet<Servicio> Servicios { get; set; }
    DbSet<OrdenTrabajo> Ordenes {get; set;}
    DbSet<DetalleTrabajo> Detalles {get; set;}

    DbSet<OrdenTrabajoArchivo> Archivos {get; set;}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}