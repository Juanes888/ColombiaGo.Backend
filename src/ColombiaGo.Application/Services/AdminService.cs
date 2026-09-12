using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class AdminService : IAdminService
{
    private readonly IUnitOfWork _uow;

    public AdminService(IUnitOfWork uow) => _uow = uow;

    public async Task<PagedResult<LugarPendienteDto>> GetLugaresPendientesAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var pageResult = await _uow.Lugares.GetPendientesAsync(page, pageSize, ct);
        var items = pageResult.Items.Select(l => l.ToPendienteDto()).ToList();
        return new PagedResult<LugarPendienteDto>(items, pageResult.Page, pageResult.PageSize, pageResult.TotalCount);
    }

    public async Task<Result> ModerarLugarAsync(int lugarId, ModerarLugarRequest request, CancellationToken ct = default)
    {
        var lugar = await _uow.Lugares.GetByIdAsync(lugarId, ct);
        if (lugar is null)
            return Result.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarId}'."));

        if (request.Aprobar)
        {
            lugar.EstadoPublicacion = EstadoPublicacion.Aprobado;
            lugar.MotivoRechazo = null;
        }
        else
        {
            lugar.EstadoPublicacion = EstadoPublicacion.Rechazado;
            lugar.MotivoRechazo = request.MotivoRechazo;
        }

        lugar.FechaActualizacion = DateTimeOffset.UtcNow;
        _uow.Lugares.Update(lugar);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }

    public async Task<PagedResult<CalificacionPendienteDto>> GetCalificacionesPendientesAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var pageResult = await _uow.Calificaciones.GetPendientesAsync(page, pageSize, ct);
        var items = pageResult.Items.Select(c => c.ToPendienteDto()).ToList();
        return new PagedResult<CalificacionPendienteDto>(items, pageResult.Page, pageResult.PageSize, pageResult.TotalCount);
    }

    public async Task<Result> ModerarCalificacionAsync(
        int calificacionId, ModerarCalificacionRequest request, CancellationToken ct = default)
    {
        var calificacion = await _uow.Calificaciones.GetByIdAsync(calificacionId, ct);
        if (calificacion is null)
            return Result.Failure(Error.NotFound("Calificacion.NotFound", $"No existe una calificación con id '{calificacionId}'."));

        calificacion.EstadoModeracion = request.Aprobar ? EstadoModeracion.Aprobada : EstadoModeracion.Rechazada;
        _uow.Calificaciones.Update(calificacion);
        await _uow.SaveChangesAsync(ct);

        return Result.Success();
    }
}
