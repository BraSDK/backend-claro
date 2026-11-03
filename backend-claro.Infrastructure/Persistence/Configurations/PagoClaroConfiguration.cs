using backend_claro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class PagoClaroConfiguration : IEntityTypeConfiguration<PagoClaro>
{
    public void Configure(EntityTypeBuilder<PagoClaro> builder)
    {
        builder.HasKey(p => p.PagoClaroId);
        builder.Property(p => p.Comprobante).IsRequired();
        builder.HasIndex(p => p.FechaPago);

        // si se borra el pago, sus SOT y multas vuelven a quedar pendientes (no se borran)
        builder.HasMany(p => p.Liquidaciones)
               .WithOne(l => l.PagoClaro)
               .HasForeignKey(l => l.PagoClaroId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(p => p.Multas)
               .WithOne(m => m.PagoClaro)
               .HasForeignKey(m => m.PagoClaroId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
