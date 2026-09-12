using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class CaracteristicaConfiguration : IEntityTypeConfiguration<Caracteristica>
{
    public void Configure(EntityTypeBuilder<Caracteristica> builder)
    {
        builder.ToTable("caracteristicas");

        builder.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.Caracteristicas)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.CategoriaId);
    }
}
