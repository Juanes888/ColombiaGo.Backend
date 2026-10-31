using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguracionConfiguration : IEntityTypeConfiguration<UsuarioConfiguracion>
{
    public void Configure(EntityTypeBuilder<UsuarioConfiguracion> builder)
    {
        builder.ToTable("usuario_configuracion");

        // Shares its PK with usuarios (one settings row per user) — never database-generated.
        builder.Property(x => x.Id).ValueGeneratedNever();
        builder.Property(x => x.Idioma).HasMaxLength(5).IsRequired().HasDefaultValue("es");
    }
}
