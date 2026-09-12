using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class LugarConfiguration : IEntityTypeConfiguration<Lugar>
{
    public void Configure(EntityTypeBuilder<Lugar> builder)
    {
        builder.ToTable("lugares");

        builder.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
        builder.Property(x => x.Slug).HasMaxLength(220).IsRequired();
        builder.Property(x => x.Ciudad).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Region).HasMaxLength(120).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(4000).IsRequired();
        builder.Property(x => x.Eslogan).HasMaxLength(200);
        builder.Property(x => x.Direccion).HasMaxLength(300).IsRequired();
        builder.Property(x => x.HorarioAtencion).HasMaxLength(150);
        builder.Property(x => x.ComoLlegar).HasMaxLength(150);
        builder.Property(x => x.ImagenPortadaUrl).HasMaxLength(500);
        builder.Property(x => x.MotivoRechazo).HasMaxLength(500);

        builder.Property(x => x.Latitud).HasPrecision(9, 6);
        builder.Property(x => x.Longitud).HasPrecision(9, 6);
        builder.Property(x => x.CalificacionPromedio).HasPrecision(3, 2);
        builder.Property(x => x.CostoEntrada).HasPrecision(12, 2);

        builder.HasOne(x => x.Categoria)
            .WithMany(c => c.Lugares)
            .HasForeignKey(x => x.CategoriaId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.UsuarioRegistrador)
            .WithMany(u => u.LugaresRegistrados)
            .HasForeignKey(x => x.RegistradoPor)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => new { x.CategoriaId, x.EstadoPublicacion });
        builder.HasIndex(x => x.Ciudad);
        builder.HasIndex(x => x.Destacado);
    }
}
