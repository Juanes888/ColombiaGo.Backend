using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class HistorialInteraccionConfiguration : IEntityTypeConfiguration<HistorialInteraccion>
{
    public void Configure(EntityTypeBuilder<HistorialInteraccion> builder)
    {
        builder.ToTable("historial_interacciones");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.HistorialInteracciones)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.HistorialInteracciones)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.Fecha });
    }
}
