using Microsoft.EntityFrameworkCore;
using backend_claro.Domain.Entities;

namespace backend_claro.Application.Interfaces;

public interface IApplicationDbContext
{
    DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    DbSet<Usuario> Usuarios { get; set; }
    DbSet<OrdenTrabajo> Ordenes {get; set;}

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}