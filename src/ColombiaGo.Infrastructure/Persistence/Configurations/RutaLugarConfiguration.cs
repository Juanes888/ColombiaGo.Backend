using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class RutaLugarConfiguration : IEntityTypeConfiguration<RutaLugar>
{
    public void Configure(EntityTypeBuilder<RutaLugar> builder)
    {
        builder.ToTable("ruta_lugares");

        builder.HasOne(x => x.Ruta)
            .WithMany(r => r.Paradas)
            .HasForeignKey(x => x.RutaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.RutaLugares)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.RutaId, x.LugarId }).IsUnique();
    }
}
