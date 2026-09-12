using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Common;
using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Services;

public sealed class EventoService : IEventoService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public EventoService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<EventoDto>> BuscarAsync(
        string? ciudad, int page, int pageSize, CancellationToken ct = default)
    {
        var pageResult = await _uow.Eventos.SearchAsync(ciudad, page, pageSize, ct);
        var favoritos = await GetFavoritoIdsAsync(ct);

        var items = pageResult.Items.Select(e => e.ToDto(favoritos.Contains(e.Id))).ToList();
        return new PagedResult<EventoDto>(items, pageResult.Page, pageResult.PageSize, pageResult.TotalCount);
    }

    public async Task<Result<EventoDto>> GetPorIdAsync(int id, CancellationToken ct = default)
    {
        var evento = await _uow.Eventos.GetByIdAsync(id, ct);
        if (evento is null)
            return Result<EventoDto>.Failure(Error.NotFound("Evento.NotFound", $"No existe un evento con id '{id}'."));

        var isFavorite = false;
        if (_currentUser.IsAuthenticated)
        {
            var userId = _currentUser.RequireUserId();
            isFavorite = await _uow.FavoritosEventos.FindAsync(userId, id, ct) is not null;
        }

        return Result<EventoDto>.Success(evento.ToDto(isFavorite));
    }

    public async Task<Result<EventoDto>> CrearAsync(CrearEventoRequest request, CancellationToken ct = default)
    {
        var adminId = _currentUser.RequireUserId();

        if (request.LugarId is { } lugarId && await _uow.Lugares.GetByIdAsync(lugarId, ct) is null)
            return Result<EventoDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarId}'."));

        var evento = new Evento
        {
            AdminId = adminId,
            LugarId = request.LugarId,
            NombreEvento = request.NombreEvento,
            FechaEvento = request.FechaEvento,
            HoraEvento = request.HoraEvento,
            DireccionEvento = request.DireccionEvento,
            Costo = request.Costo,
            Descripcion = request.Descripcion,
            Estado = EstadoEvento.Programado,
            FechaCreacion = DateTimeOffset.UtcNow,
        };

        await _uow.Eventos.AddAsync(evento, ct);
        await _uow.SaveChangesAsync(ct);

        return Result<EventoDto>.Success(evento.ToDto(isFavorite: false));
    }

    private async Task<HashSet<int>> GetFavoritoIdsAsync(CancellationToken ct)
    {
        if (!_currentUser.IsAuthenticated)
            return new HashSet<int>();

        var favoritos = await _uow.FavoritosEventos.GetByUsuarioAsync(_currentUser.RequireUserId(), ct);
        return favoritos.Select(f => f.EventoId).ToHashSet();
    }
}
