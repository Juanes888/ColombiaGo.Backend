using ColombiaGo.Application.DTOs.Lugares;

namespace ColombiaGo.Application.DTOs.Rutas;

/// <summary>Suggested route from the AI planner. Unlike <see cref="RutaDto"/> this is never
/// persisted — it's a suggestion the user reviews before adding stops to their real route via
/// the existing <c>mis-paradas</c> endpoints.</summary>
public sealed record RutaIaDto(
    IReadOnlyList<LugarResumenDto> Paradas,
    decimal CostoEstimado,
    string Explicacion);
