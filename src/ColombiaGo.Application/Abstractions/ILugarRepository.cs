using ColombiaGo.Application.Common;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Abstractions;

public interface ILugarRepository : IRepository<Lugar, int>
{
    /// <summary>Loads a place by its public slug with everything a detail page needs
    /// (category, gallery, features, dynamic attributes) eagerly loaded.</summary>
    Task<Lugar?> GetBySlugWithDetailsAsync(string slug, CancellationToken ct = default);

    /// <summary>Lean slug lookup (no related-data includes) — for callers that only need the
    /// numeric id, e.g. resolving a favorite/route toggle by the slug the frontend routes with.</summary>
    Task<Lugar?> GetBySlugAsync(string slug, CancellationToken ct = default);

    Task<PagedResult<Lugar>> SearchAsync(LugarQueryParams query, CancellationToken ct = default);

    /// <summary>Lugares aprobados de una ciudad (coincidencia parcial: incluye "A 2h de Medellín"), opcionalmente
    /// limitados a categorías, mejor calificados primero. Candidatos reales para la guía de viaje con IA.</summary>
    Task<IReadOnlyList<Lugar>> GetPorCiudadAsync(
        string ciudad, IReadOnlyCollection<string>? categoriaSlugs, int take, CancellationToken ct = default);

    Task<IReadOnlyList<Lugar>> GetBySlugsAsync(IReadOnlyCollection<string> slugs, CancellationToken ct = default);

    /// <summary>Ciudades que tienen al menos un lugar aprobado, con su cantidad de lugares.</summary>
    Task<IReadOnlyList<(string Ciudad, int Total)>> GetCiudadesAsync(CancellationToken ct = default);

    Task<IReadOnlyList<Lugar>> GetDestacadosAsync(int take, CancellationToken ct = default);

    /// <summary>Highest-rated approved places, optionally restricted to a set of categories —
    /// the candidate pool the recommendation heuristic ranks and trims down from.</summary>
    Task<IReadOnlyList<Lugar>> GetTopRatedAsync(
        int take, IReadOnlyCollection<int>? categoriaIds, CancellationToken ct = default);

    Task<PagedResult<Lugar>> GetPendientesAsync(int page, int pageSize, CancellationToken ct = default);
}
