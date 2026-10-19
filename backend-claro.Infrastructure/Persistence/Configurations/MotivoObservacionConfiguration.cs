using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class MotivoObservacionConfiguracion : IEntityTypeConfiguration<MotivoObservacion>
{
    public void Configure(EntityTypeBuilder<MotivoObservacion> builder)
    {
      
      builder.HasKey( m => m.MotivoObservacionId);
    }
}