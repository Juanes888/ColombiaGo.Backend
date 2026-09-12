using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class TipoAtributoConfiguration : IEntityTypeConfiguration<TipoAtributo>
{
    public void Configure(EntityTypeBuilder<TipoAtributo> builder)
    {
        builder.ToTable("tipos_atributo");

        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        builder.Property(x => x.Etiqueta).HasMaxLength(150).IsRequired();

        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.TiposAtributo)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CategoriaId);
    }
}
