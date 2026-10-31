using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class GuiaViajeConfiguration : IEntityTypeConfiguration<GuiaViaje>
{
    public void Configure(EntityTypeBuilder<GuiaViaje> builder)
    {
        builder.ToTable("guias_viaje");

        builder.Property(x => x.Ciudad).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Presupuesto).HasPrecision(14, 2);
        builder.Property(x => x.Idioma).HasMaxLength(5).IsRequired();
        builder.Property(x => x.Intereses).HasColumnType("text[]");
        builder.Property(x => x.Contenido).HasColumnType("jsonb").IsRequired();

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.FechaCreacion });
    }
}
