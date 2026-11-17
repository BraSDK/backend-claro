using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class HojaConfiguration : IEntityTypeConfiguration<Hoja>
{
    public void Configure(EntityTypeBuilder<Hoja> builder)
    {
        builder.HasKey(h => h.HojaId);
        builder.Property(h => h.Nombre).IsRequired().HasMaxLength(Hoja.LargoMaximoNombre);

        // una sola hoja por mes
        builder.HasIndex(h => new { h.Anio, h.Mes }).IsUnique();

        // Restrict: una hoja con liquidaciones no se puede borrar (se perderían sus SOT)
        builder.HasMany(h => h.Liquidaciones)
               .WithOne(l => l.Hoja)
               .HasForeignKey(l => l.HojaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
