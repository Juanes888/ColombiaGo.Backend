using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class GaleriaFotoConfiguration : IEntityTypeConfiguration<GaleriaFoto>
{
    public void Configure(EntityTypeBuilder<GaleriaFoto> builder)
    {
        builder.ToTable("galeria_fotos");

        builder.Property(x => x.UrlImagen).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Galeria)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.LugarId);
    }
}
