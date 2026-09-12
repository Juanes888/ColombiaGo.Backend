using ColombiaGo.Application.DTOs.Admin;

namespace ColombiaGo.Application.Services;

public interface IReporteService
{
    /// <summary>Records that a report was requested. Actual file generation (PDF/Excel/CSV
    /// export) is out of scope for this service — <c>ArchivoUrl</c> stays null until a background
    /// export job is wired in; this only creates the audit/tracking row.</summary>
    Task<ReporteDto> CrearAsync(CrearReporteRequest request, CancellationToken ct = default);
}
