using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class UsuarioLogroConfiguration : IEntityTypeConfiguration<UsuarioLogro>
{
    public void Configure(EntityTypeBuilder<UsuarioLogro> builder)
    {
        builder.ToTable("usuario_logros");

        builder.HasOne(x => x.Usuario)
            .WithMany(u => u.Logros)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.Logro)
            .WithMany(l => l.UsuarioLogros)
            .HasForeignKey(x => x.LogroId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.UsuarioId, x.LogroId }).IsUnique();
    }
}
