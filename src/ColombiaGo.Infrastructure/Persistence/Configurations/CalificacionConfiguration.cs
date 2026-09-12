using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class CalificacionConfiguration : IEntityTypeConfiguration<Calificacion>
{
    public void Configure(EntityTypeBuilder<Calificacion> builder)
    {
        builder.ToTable("calificaciones");

        builder.Property(x => x.Comentario).HasMaxLength(2000);
        builder.ToTable(t => t.HasCheckConstraint("ck_calificaciones_puntuacion", "puntuacion BETWEEN 1 AND 5"));

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Calificaciones)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Calificaciones)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.LugarId }).IsUnique();
        builder.HasIndex(x => new { x.LugarId, x.EstadoModeracion });
    }
}
