using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using backend_claro.Domain.Entities;

namespace backend_claro.Infrastructure.Persistence.Configurations;

public class CuentaUsuarioConfiguration : IEntityTypeConfiguration<CuentaUsuario>
{
    public void Configure(EntityTypeBuilder<CuentaUsuario> builder)
    {
        builder.HasKey(e => e.Id);   
        builder.Property(e => e.Email).IsRequired().HasMaxLength(150);
        builder.HasIndex(e => e.Email).IsUnique();
    }        
}