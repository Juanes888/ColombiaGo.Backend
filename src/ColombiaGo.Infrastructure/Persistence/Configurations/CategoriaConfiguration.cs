using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class CategoriaConfiguration : IEntityTypeConfiguration<Categoria>
{
    public void Configure(EntityTypeBuilder<Categoria> builder)
    {
        builder.ToTable("categorias");

        builder.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(80).IsRequired();
        builder.Property(x => x.Icono).HasMaxLength(100);
        builder.Property(x => x.Tono).HasMaxLength(30);

        builder.HasIndex(x => x.Nombre).IsUnique();
        builder.HasIndex(x => x.Slug).IsUnique();
    }
}
