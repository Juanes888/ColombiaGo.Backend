using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Domain.Common;

namespace ColombiaGo.Application.Services;

public interface IEventoService
{
    Task<PagedResult<EventoDto>> BuscarAsync(string? ciudad, int page, int pageSize, CancellationToken ct = default);

    Task<Result<EventoDto>> GetPorIdAsync(int id, CancellationToken ct = default);

    /// <summary>Creates an event administered by the current user. Authorization (who may call
    /// this) is enforced at the controller via policy, not here.</summary>
    Task<Result<EventoDto>> CrearAsync(CrearEventoRequest request, CancellationToken ct = default);
}
