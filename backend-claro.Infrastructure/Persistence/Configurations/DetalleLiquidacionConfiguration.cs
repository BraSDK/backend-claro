using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class DetalleTrabajoConfiguration : IEntityTypeConfiguration<DetalleLiquidacion>
{
    public void Configure(EntityTypeBuilder<DetalleLiquidacion> builder)
    {
        // El detalle ya no depende del catálogo de servicios: guarda su propio Concepto y PrecioUnitario
        builder.Property(d => d.Concepto).IsRequired().HasMaxLength(150);
        builder.HasKey(d => d.DetalleLiquidacionId);     //tiene primariKey

        builder.Property(d => d.PrecioUnitario).HasPrecision(18, 2);       // antes [Column(TypeName = "decimal(18,2)")]
        builder.Property(d => d.PrecioTotal).HasPrecision(18, 2);     


        //RELACION CON LA LIQUIDACION
        builder.HasOne(d => d.Liquidacion)
               .WithMany(l => l.Detalles)
               .HasForeignKey( d => d.LiquidacionId )
               .OnDelete(DeleteBehavior.Restrict);


        builder.HasOne(d => d.CostoAdicional)
               .WithMany()
               .HasForeignKey(d => d.CostoAdicionalId)
               .OnDelete(DeleteBehavior.Restrict);

        
               
        

    }
}