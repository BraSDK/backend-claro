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
    public DbSet<OrdenTrabajo> Ordenes {get; set;}
    public DbSet<DetalleTrabajo> Detalles { get; set; }
    public DbSet<Servicio> Servicios { get; set; }
    public DbSet<OrdenTrabajoArchivo> Archivos { get; set; }

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
        modelBuilder.Entity<OrdenTrabajo>(entity =>
        {
            entity.HasOne(u => u.Usuario)
                  .WithMany()
                  .HasForeignKey(u => u.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            entity.HasMany(o => o.Detalles)          
                  .WithOne(d => d.OrdenTrabajo)      
                  .HasForeignKey(d => d.OrdenTrabajoId)  
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany( a => a.Archivos)
                  .WithOne(d => d.OrdenT)
                  .HasForeignKey(d => d.OrdenTrabajoId)
                  .OnDelete(DeleteBehavior.Cascade);
        
        }

        );

        modelBuilder.Entity<DetalleTrabajo>(entity =>
        {
            entity.HasOne(d => d.Servicio)
                  .WithMany()
                  .HasForeignKey(d => d.ServicioId)
                  .HasPrincipalKey(s => s.Codigo)   
                  .OnDelete(DeleteBehavior.Restrict);
        }
                
        );
    }  
}