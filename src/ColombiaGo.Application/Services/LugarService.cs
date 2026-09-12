using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class LugarService : ILugarService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public LugarService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<LugarResumenDto>> BuscarAsync(LugarQueryParams query, CancellationToken ct = default)
    {
        var page = await _uow.Lugares.SearchAsync(query, ct);
        var items = page.Items.Select(l => l.ToResumenDto()).ToList();
        return new PagedResult<LugarResumenDto>(items, page.Page, page.PageSize, page.TotalCount);
    }

    public async Task<IReadOnlyList<LugarResumenDto>> GetDestacadosAsync(int take, CancellationToken ct = default)
    {
        var lugares = await _uow.Lugares.GetDestacadosAsync(take, ct);
        return lugares.Select(l => l.ToResumenDto()).ToList();
    }

    public async Task<Result<LugarDetalleDto>> GetPorSlugAsync(string slug, CancellationToken ct = default)
    {
        var lugar = await _uow.Lugares.GetBySlugWithDetailsAsync(slug, ct);
        if (lugar is null || lugar.EstadoPublicacion != EstadoPublicacion.Aprobado)
            return Result<LugarDetalleDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{slug}'."));

        var isFavorite = false;
        var isInRoute = false;

        if (_currentUser.IsAuthenticated)
        {
            var userId = _currentUser.RequireUserId();

            isFavorite = await _uow.FavoritosLugares.FindAsync(userId, lugar.Id, ct) is not null;

            var draft = await _uow.Rutas.GetDraftWithParadasAsync(userId, ct);
            isInRoute = draft?.Paradas.Any(p => p.LugarId == lugar.Id) ?? false;

            await RegistrarVistaAsync(userId, lugar.Id, ct);
        }

        return Result<LugarDetalleDto>.Success(lugar.ToDetalleDto(isFavorite, isInRoute));
    }

    private async Task RegistrarVistaAsync(Guid usuarioId, int lugarId, CancellationToken ct)
    {
        await _uow.HistorialInteracciones.AddAsync(new HistorialInteraccion
        {
            UsuarioId = usuarioId,
            LugarId = lugarId,
            TipoInteraccion = TipoInteraccion.Vista,
            Fecha = DateTimeOffset.UtcNow,
        }, ct);

        await _uow.SaveChangesAsync(ct);
    }
}
