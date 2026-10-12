using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class LiquidacionConfiguration : IEntityTypeConfiguration<Liquidacion>
{
    public void Configure(EntityTypeBuilder<Liquidacion> builder)
    {
        builder.HasKey( l => l.LiquidacionId);   
        builder.Property( l => l.Sot).IsRequired();
        builder.HasIndex(l => l.Sot).IsUnique();

        //relacion con orden trabajo
        builder.HasOne(l => l.OrdenTrabajo)
               .WithOne()
               .HasForeignKey<Liquidacion>(l => l.OrdenTrabajoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany( l => l.Observaciones)
               .WithOne( l => l.Liquidacion)
               .HasForeignKey(d => d.LiquidacionId)
               .OnDelete(DeleteBehavior.Cascade);

        
    }        
}