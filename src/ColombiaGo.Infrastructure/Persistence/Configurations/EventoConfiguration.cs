using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class EventoConfiguration : IEntityTypeConfiguration<Evento>
{
    public void Configure(EntityTypeBuilder<Evento> builder)
    {
        builder.ToTable("eventos");

        builder.Property(x => x.NombreEvento).HasMaxLength(200).IsRequired();
        builder.Property(x => x.DireccionEvento).HasMaxLength(300).IsRequired();
        builder.Property(x => x.Descripcion).HasMaxLength(2000);
        builder.Property(x => x.Costo).HasPrecision(12, 2);

        builder.HasOne(x => x.Admin)
            .WithMany(u => u.EventosAdministrados)
            .HasForeignKey(x => x.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Lugar)
            .WithMany(l => l.Eventos)
            .HasForeignKey(x => x.LugarId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(x => x.FechaEvento);
        builder.HasIndex(x => x.LugarId);
    }
}
