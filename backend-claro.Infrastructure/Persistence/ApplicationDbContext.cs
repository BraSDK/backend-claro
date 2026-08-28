using System.Reflection; // Requerido para leer el ensamblado (Assembly)
using Microsoft.EntityFrameworkCore;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options) : base(options) {}   

    public DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Busca todas las clases IEntityTypeConfiguration en este proyecto y las aplica automáticamente
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }  
}