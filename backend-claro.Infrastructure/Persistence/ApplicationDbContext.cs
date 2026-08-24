using Microsoft.EntityFrameworkCore;
using backend_claro.Application.Interfaces;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence;

public class ApplicationDbContext : DbContext, IApplicationDbContext
                                                
{
    public ApplicationDbContext (DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {                
    }   

    public DbSet<CuentaUsuario> CuentaUsuarios { get; set; }
    public DbSet<Usuario> Usuarios { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configuracion de CuentaUsuario (Autenticación)
        modelBuilder.Entity<CuentaUsuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(150);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Configuracion de Usuarios (Perfil/Negocio) Y Relacion 1 a 1
        modelBuilder.Entity<Usuario>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);

            // Relacion 1 a 1: Un Usuario pertenece a una CuentaUsuario
            entity.HasOne(u => u.Cuenta)
                  .WithOne(c => c.Perfil)
                  .HasForeignKey<Usuario>(u => u.AuthId)
                  .OnDelete(DeleteBehavior.Cascade);
        });
    }  
}