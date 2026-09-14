using System.Reflection; // Requerido para leer el ensamblado (Assembly)
using Microsoft.EntityFrameworkCore;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;
using backend_claro.Domain.Interfaces;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace backend_claro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options) {}   

    public DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<OrdenTrabajo> Ordenes {get; set;}
    public DbSet<DetalleTrabajo> Detalles { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<OrdenTrabajoArchivo> Archivos { get; set; }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var entries = ChangeTracker.Entries<IAuditable>();

        foreach (var entry in entries)
        {

            if (entry.State == EntityState.Added)
            {
                entry.Entity.FechaCreacion = DateTime.UtcNow;
                entry.Entity.FechaActualizacion = DateTime.UtcNow;
            }else if(entry.State == EntityState.Modified){
                entry.Entity.FechaActualizacion =DateTime.UtcNow;
                entry.Property(nameof(IAuditable.FechaCreacion)).IsModified = false;
            }

        }

        return await base.SaveChangesAsync(cancellationToken);
    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Busca todas las clases IEntityTypeConfiguration en este proyecto y las aplica automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }  
}