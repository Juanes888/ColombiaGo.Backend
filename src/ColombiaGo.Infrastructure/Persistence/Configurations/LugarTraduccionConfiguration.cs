using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class LugarTraduccionConfiguration : IEntityTypeConfiguration<LugarTraduccion>
{
    public void Configure(EntityTypeBuilder<LugarTraduccion> builder)
    {
        builder.ToTable("lugar_traducciones");

        builder.Property(x => x.Idioma).HasMaxLength(5).IsRequired();
        builder.Property(x => x.Nombre).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Eslogan).HasMaxLength(300);
        builder.Property(x => x.ComoLlegar).HasMaxLength(300);
        builder.Property(x => x.HorarioAtencion).HasMaxLength(300);

        builder.HasOne(x => x.Lugar)
            .WithMany()
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LugarId, x.Idioma }).IsUnique();
    }
}
