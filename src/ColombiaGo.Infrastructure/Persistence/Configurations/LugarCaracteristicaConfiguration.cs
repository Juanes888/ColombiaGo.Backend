using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class LugarCaracteristicaConfiguration : IEntityTypeConfiguration<LugarCaracteristica>
{
    public void Configure(EntityTypeBuilder<LugarCaracteristica> builder)
    {
        builder.ToTable("lugar_caracteristicas");

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Caracteristicas)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Caracteristica)
            .WithMany(c => c.LugarCaracteristicas)
            .HasForeignKey(x => x.CaracteristicaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LugarId, x.CaracteristicaId }).IsUnique();
    }
}
