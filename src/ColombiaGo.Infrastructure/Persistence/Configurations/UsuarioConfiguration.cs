using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("usuarios");

        // Id mirrors auth.users.id (provisioned by UsuarioProvisioner, not database-generated).
        builder.Property(x => x.Id).ValueGeneratedNever();

        builder.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        builder.Property(x => x.CorreoElectronico).HasMaxLength(320).IsRequired();
        builder.Property(x => x.FotoPerfilUrl).HasMaxLength(500);
        builder.Property(x => x.Nivel).HasMaxLength(50).IsRequired();

        builder.HasOne(x => x.Rol)
            .WithMany(r => r.Usuarios)
            .HasForeignKey(x => x.RolId)
            .OnDelete(DeleteBehavior.Restrict);

        // SetNull, not Cascade: a guide's directory listing (rating, specialty, etc.) should
        // survive the linked user account being removed — it just becomes unlinked again.
        builder.HasOne(x => x.Guia)
            .WithOne(g => g.Usuario)
            .HasForeignKey<Guia>(g => g.UsuarioId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(x => x.Configuracion)
            .WithOne(c => c.Usuario)
            .HasForeignKey<UsuarioConfiguracion>(c => c.Id)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CorreoElectronico).IsUnique();
        builder.HasIndex(x => x.RolId);
    }
}
