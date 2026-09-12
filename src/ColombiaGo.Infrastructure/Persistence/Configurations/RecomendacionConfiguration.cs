using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class RecomendacionConfiguration : IEntityTypeConfiguration<Recomendacion>
{
    public void Configure(EntityTypeBuilder<Recomendacion> builder)
    {
        builder.ToTable("recomendaciones");

        builder.Property(x => x.Motivo).HasMaxLength(300);
        builder.Property(x => x.Score).HasPrecision(5, 2);

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Recomendaciones)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Recomendaciones)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.FechaGenerada });
    }
}
