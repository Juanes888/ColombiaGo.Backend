using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class LogroConfiguration : IEntityTypeConfiguration<Logro>
{
    public void Configure(EntityTypeBuilder<Logro> builder)
    {
        builder.ToTable("logros");

        builder.Property(x => x.Titulo).HasMaxLength(150).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(500).IsRequired();
        builder.Property(x => x.Icono).HasMaxLength(50).IsRequired();
        builder.Property(x => x.Tono).HasMaxLength(30).IsRequired();
        builder.Property(x => x.Criterio).HasMaxLength(300);
    }
}
