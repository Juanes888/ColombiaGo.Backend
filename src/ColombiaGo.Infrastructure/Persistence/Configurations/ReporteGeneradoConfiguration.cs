using ColombiaGo.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ColombiaGo.Infrastructure.Persistence.Configurations;

public class ReporteGeneradoConfiguration : IEntityTypeConfiguration<ReporteGenerado>
{
    public void Configure(EntityTypeBuilder<ReporteGenerado> builder)
    {
        builder.ToTable("reportes_generados");

        builder.Property(x => x.ArchivoUrl).HasMaxLength(500);

        builder.HasOne(x => x.Admin)
            .WithMany(u => u.ReportesGenerados)
            .HasForeignKey(x => x.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.AdminId);
    }
}
