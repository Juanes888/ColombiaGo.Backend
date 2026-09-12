using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class RutaConfiguration : IEntityTypeConfiguration<Ruta>
{
    public void Configure(EntityTypeBuilder<Ruta> builder)
    {
        builder.ToTable("rutas");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Rutas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        // Supports the "does this user already have an open draft?" lookup
        // (fecha_generacion is null while the route is still being assembled).
        builder.HasIndex(x => new { x.UsuarioId, x.FechaGeneracion });
    }
}
