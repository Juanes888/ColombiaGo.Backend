using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

/// <summary>Moderation queue operations. Who may call these is enforced by the controller's
/// authorization policy, not by this service.</summary>
public interface IAdminService
{
    Task<PagedResult<LugarPendienteDto>> GetLugaresPendientesAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result> ModerarLugarAsync(int lugarId, ModerarLugarRequest request, CancellationToken ct = default);

    Task<PagedResult<CalificacionPendienteDto>> GetCalificacionesPendientesAsync(int page, int pageSize, CancellationToken ct = default);

    Task<Result> ModerarCalificacionAsync(int calificacionId, ModerarCalificacionRequest request, CancellationToken ct = default);
}
