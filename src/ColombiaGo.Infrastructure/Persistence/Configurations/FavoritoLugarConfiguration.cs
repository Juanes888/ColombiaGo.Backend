using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class FavoritoLugarConfiguration : IEntityTypeConfiguration<FavoritoLugar>
{
    public void Configure(EntityTypeBuilder<FavoritoLugar> builder)
    {
        builder.ToTable("favoritos_lugares");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.FavoritosLugares)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.FavoritosLugares)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.LugarId }).IsUnique();
    }
}
