
using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class OrdenTrabajoConfiguration : IEntityTypeConfiguration<OrdenTrabajo>
{
    public void Configure(EntityTypeBuilder<OrdenTrabajo> builder)
    {
        
            builder.HasOne(u => u.Usuario)
                  .WithMany()
                  .HasForeignKey(u => u.UsuarioId)
                  .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasMany(o => o.Detalles)          
                  .WithOne(d => d.OrdenTrabajo)      
                  .HasForeignKey(d => d.OrdenTrabajoId)  
                  .OnDelete(DeleteBehavior.Cascade);

            // se busca y se cruza mucho por SOT (listado, conciliación, importación)
            builder.HasIndex(o => o.Sot);

            builder.HasMany( a => a.Archivos)
                  .WithOne(d => d.OrdenT)
                  .HasForeignKey(d => d.OrdenTrabajoId)
                  .OnDelete(DeleteBehavior.Cascade);
        
        

        
    }
}