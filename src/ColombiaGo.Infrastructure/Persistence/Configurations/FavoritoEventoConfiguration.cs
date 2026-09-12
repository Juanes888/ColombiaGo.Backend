using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class FavoritoEventoConfiguration : IEntityTypeConfiguration<FavoritoEvento>
{
    public void Configure(EntityTypeBuilder<FavoritoEvento> builder)
    {
        builder.ToTable("favoritos_eventos");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.FavoritosEventos)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Evento)
            .WithMany(e => e.FavoritosEventos)
            .HasForeignKey(x => x.EventoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.EventoId }).IsUnique();
    }
}
