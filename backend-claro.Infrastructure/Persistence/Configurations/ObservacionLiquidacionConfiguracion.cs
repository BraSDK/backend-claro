using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class ObservacionLiquidacionConfiguracion : IEntityTypeConfiguration<ObservacionLiquidacion>
{
    public void Configure(EntityTypeBuilder<ObservacionLiquidacion> builder)
    {
        builder.HasKey( o => o.ObservacionLiquidacionId);

        builder.HasOne( o => o.Usuario)
                .WithMany()
                .HasForeignKey( o => o.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne( o => o.Motivo )
                .WithMany()
                .HasForeignKey( o => o.MotivoObservacionId)
                .OnDelete(DeleteBehavior.Restrict);

        builder.Property( o => o.Texto).HasMaxLength(200);
        
    }
}