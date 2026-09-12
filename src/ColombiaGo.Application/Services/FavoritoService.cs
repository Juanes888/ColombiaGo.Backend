using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Application.DTOs.Favoritos;
using ColombiaGo.Application.DTOs.Lugares;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

public sealed class FavoritoService : IFavoritoService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public FavoritoService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<LugarResumenDto>> GetLugaresFavoritosAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var favoritos = await _uow.FavoritosLugares.GetByUsuarioAsync(userId, ct);
        return favoritos
            .OrderByDescending(f => f.FechaAgregado)
            .Select(f => f.Lugar.ToResumenDto())
            .ToList();
    }

    public async Task<Result<FavoritoEstadoDto>> AgregarLugarAsync(string lugarSlug, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<FavoritoEstadoDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        if (await _uow.FavoritosLugares.FindAsync(userId, lugar.Id, ct) is null)
        {
            await _uow.FavoritosLugares.AddAsync(new FavoritoLugar
            {
                UsuarioId = userId,
                LugarId = lugar.Id,
                FechaAgregado = DateTimeOffset.UtcNow,
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<FavoritoEstadoDto>.Success(new FavoritoEstadoDto(true));
    }

    public async Task<Result<FavoritoEstadoDto>> QuitarLugarAsync(string lugarSlug, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var lugar = await _uow.Lugares.GetBySlugAsync(lugarSlug, ct);
        if (lugar is null)
            return Result<FavoritoEstadoDto>.Failure(Error.NotFound("Lugar.NotFound", $"No existe un lugar con id '{lugarSlug}'."));

        var existente = await _uow.FavoritosLugares.FindAsync(userId, lugar.Id, ct);
        if (existente is not null)
        {
            _uow.FavoritosLugares.Remove(existente);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<FavoritoEstadoDto>.Success(new FavoritoEstadoDto(false));
    }

    public async Task<IReadOnlyList<EventoDto>> GetEventosFavoritosAsync(CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();
        var favoritos = await _uow.FavoritosEventos.GetByUsuarioAsync(userId, ct);
        return favoritos
            .OrderByDescending(f => f.FechaAgregado)
            .Select(f => f.Evento.ToDto(isFavorite: true))
            .ToList();
    }

    public async Task<Result<FavoritoEstadoDto>> AgregarEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var evento = await _uow.Eventos.GetByIdAsync(eventoId, ct);
        if (evento is null)
            return Result<FavoritoEstadoDto>.Failure(Error.NotFound("Evento.NotFound", $"No existe un evento con id '{eventoId}'."));

        if (await _uow.FavoritosEventos.FindAsync(userId, eventoId, ct) is null)
        {
            await _uow.FavoritosEventos.AddAsync(new FavoritoEvento
            {
                UsuarioId = userId,
                EventoId = eventoId,
                FechaAgregado = DateTimeOffset.UtcNow,
            }, ct);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<FavoritoEstadoDto>.Success(new FavoritoEstadoDto(true));
    }

    public async Task<Result<FavoritoEstadoDto>> QuitarEventoAsync(int eventoId, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var existente = await _uow.FavoritosEventos.FindAsync(userId, eventoId, ct);
        if (existente is not null)
        {
            _uow.FavoritosEventos.Remove(existente);
            await _uow.SaveChangesAsync(ct);
        }

        return Result<FavoritoEstadoDto>.Success(new FavoritoEstadoDto(false));
    }
}
