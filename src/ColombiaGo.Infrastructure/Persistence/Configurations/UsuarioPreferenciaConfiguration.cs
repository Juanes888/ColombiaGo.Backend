using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class UsuarioPreferenciaConfiguration : IEntityTypeConfiguration<UsuarioPreferencia>
{
    public void Configure(EntityTypeBuilder<UsuarioPreferencia> builder)
    {
        builder.ToTable("usuario_preferencias");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Preferencias)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.Preferencias)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.CategoriaId }).IsUnique();
    }
}
