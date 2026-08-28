using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.NombreCompleto).IsRequired().HasMaxLength(200);

        // Relacion 1 a 1: Un Usuario pertenece a una CuentaUsuario
        builder.HasOne(u => u.Cuenta)
               .WithOne(c => c.Perfil)
               .HasForeignKey<Usuario>(u => u.AuthId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}