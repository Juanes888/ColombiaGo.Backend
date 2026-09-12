using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class LugarAtributoConfiguration : IEntityTypeConfiguration<LugarAtributo>
{
    public void Configure(EntityTypeBuilder<LugarAtributo> builder)
    {
        builder.ToTable("lugar_atributos");

        builder.Property(x => x.Valor).HasMaxLength(500).IsRequired();

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Atributos)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.TipoAtributo)
            .WithMany(t => t.LugarAtributos)
            .HasForeignKey(x => x.TipoAtributoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => new { x.LugarId, x.TipoAtributoId }).IsUnique();
    }
}
