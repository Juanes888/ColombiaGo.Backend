using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> builder)
    {
        builder.ToTable("roles");

        builder.Property(x => x.Nombre).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(200);

        builder.HasIndex(x => x.Nombre).IsUnique();
    }
}
