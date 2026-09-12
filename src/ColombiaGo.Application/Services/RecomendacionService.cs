using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.DTOs.Recomendaciones;
using ColombiaGo.Application.Mapping;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Services;

/// <summary>
/// Computes a simple, explainable recommendation set: highest-rated approved places in the
/// categories the user follows (falling back to overall top-rated when they have no
/// preferences), excluding places they already favorited. Not a learned/ML ranking — a
/// deliberately transparent starting heuristic the <c>recomendaciones</c> table is built to
/// support swapping out later without changing its shape.
/// </summary>
public sealed class RecomendacionService : IRecomendacionService
{
    private readonly IUnitOfWork _uow;
    private readonly ICurrentUser _currentUser;

    public RecomendacionService(IUnitOfWork uow, ICurrentUser currentUser)
    {
        _uow = uow;
        _currentUser = currentUser;
    }

    public async Task<IReadOnlyList<RecomendacionDto>> GetRecomendacionesAsync(int take, CancellationToken ct = default)
    {
        var userId = _currentUser.RequireUserId();

        var categoriaIds = await _uow.UsuarioPreferencias.GetCategoriaIdsByUsuarioAsync(userId, ct);
        var favoritos = await _uow.FavoritosLugares.GetByUsuarioAsync(userId, ct);
        var favoritoIds = favoritos.Select(f => f.LugarId).ToHashSet();

        var candidatos = await _uow.Lugares.GetTopRatedAsync(
            take: take + favoritoIds.Count,
            categoriaIds: categoriaIds.Count > 0 ? categoriaIds : null,
            ct: ct);

        var seleccionados = candidatos
            .Where(l => !favoritoIds.Contains(l.Id))
            .Take(take)
            .ToList();

        var tienePreferencias = categoriaIds.Count > 0;
        var recomendaciones = seleccionados.Select(lugar => new Recomendacion
        {
            UsuarioId = userId,
            LugarId = lugar.Id,
            Lugar = lugar,
            Score = lugar.CalificacionPromedio,
            Motivo = tienePreferencias
                ? $"Popular en {lugar.Categoria.Nombre}, una de tus categorías favoritas"
                : "Uno de los lugares mejor calificados de ColombiaGO",
            FechaGenerada = DateTimeOffset.UtcNow,
        }).ToList();

        foreach (var recomendacion in recomendaciones)
            await _uow.Recomendaciones.AddAsync(recomendacion, ct);

        if (recomendaciones.Count > 0)
            await _uow.SaveChangesAsync(ct);

        return recomendaciones.Select(r => r.ToDto()).ToList();
    }
}
