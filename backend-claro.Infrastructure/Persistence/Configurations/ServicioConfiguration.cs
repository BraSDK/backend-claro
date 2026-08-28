using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class ServicioConfiguration : IEntityTypeConfiguration<Servicio>
{
    public void Configure(EntityTypeBuilder<Servicio> builder)
    {
        builder.HasKey(s => s.Codigo);
        //builder.Property(s => s.Codigo).ValueGeneratedNever();
        builder.Property(s => s.Nombre).IsRequired().HasMaxLength(150);
        builder.Property(s => s.Precio).IsRequired().HasColumnType("decimal(18,2)");
        builder.HasIndex();
    }
}