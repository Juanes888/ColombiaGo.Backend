using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class GuiaConfiguration : IEntityTypeConfiguration<Guia>
{
    public void Configure(EntityTypeBuilder<Guia> builder)
    {
        builder.ToTable("guias");

        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.FotoUrl).HasMaxLength(500);
        builder.Property(x => x.Especialidad).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Etiqueta).HasMaxLength(60).IsRequired();
        builder.Property(x => x.TonoEtiqueta).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Insignia).HasMaxLength(150);
        builder.Property(x => x.Lugar).HasMaxLength(150).IsRequired();
        builder.Property(x => x.TarifaHora).HasPrecision(12, 2);
        builder.Property(x => x.CalificacionPromedio).HasPrecision(3, 2);
        builder.Property(x => x.Idiomas).HasColumnType("text[]");
        builder.Property(x => x.DescripcionI18n).HasColumnType("jsonb").HasDefaultValueSql("'{}'::jsonb");

        // Filtered unique index: many guides can have UsuarioId = null (unlinked directory
        // entries), but a real user account can only back one guide profile.
        builder.HasIndex(x => x.UsuarioId).IsUnique().HasFilter("usuario_id IS NOT NULL");
    }
}
