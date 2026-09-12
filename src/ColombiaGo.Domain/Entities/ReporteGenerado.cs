using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>Record of an admin-generated report (analytics export), with its date range and
/// output format/location.</summary>
public class ReporteGenerado : Entity<int>
{
    public Guid AdminId { get; set; }
    public Usuario Admin { get; set; } = null!;

    public TipoReporte TipoReporte { get; set; }
    public DateOnly FechaInicio { get; set; }
    public DateOnly FechaFin { get; set; }
    public FormatoSalida FormatoSalida { get; set; }
    public string? ArchivoUrl { get; set; }

    public DateTimeOffset FechaGeneracion { get; set; }
}
