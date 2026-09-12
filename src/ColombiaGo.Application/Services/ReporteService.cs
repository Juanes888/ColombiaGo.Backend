using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class ReporteService : IReporteService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public ReporteService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<ReporteDto> CrearAsync(CrearReporteRequest request, CancellationToken ct = default)
    {
        var adminId = _currentUser.RequireUserId();

        var reporte = new ReporteGenerado
        {
            AdminId = adminId,
            TipoReporte = request.Tipo,
            FechaInicio = request.FechaInicio,
            FechaFin = request.FechaFin,
            FormatoSalida = request.Formato,
            ArchivoUrl = null,
            FechaGeneracion = DateTimeOffset.UtcNow,
        };

        await _uow.Reportes.AddAsync(reporte, ct);
        await _uow.SaveChangesAsync(ct);

        return reporte.ToDto();
    }
}
