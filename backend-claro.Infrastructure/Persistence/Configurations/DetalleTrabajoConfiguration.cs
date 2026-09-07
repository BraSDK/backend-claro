using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class DetalleTrabajoConfiguration : IEntityTypeConfiguration<DetalleTrabajo>
{
    public void Configure(EntityTypeBuilder<DetalleTrabajo> builder)
    {
        builder.HasOne(d => d.Servicio)
                  .WithMany()
                  .HasForeignKey(d => d.ServicioId)
                  .HasPrincipalKey(s => s.Codigo)   
                  .OnDelete(DeleteBehavior.Restrict);
    }
}