namespace ColombiaGo.Application.DTOs.Lugares;

/// <summary>
/// Shape mirrors the frontend's <c>Destination</c> type in <c>src/data/destinations.ts</c> so a
/// listing screen can swap its mock import for a fetch to this DTO with no field renames.
/// </summary>
public sealed record LugarResumenDto(
    string Id,               // slug — the frontend routes by it (/destino/:id)
    string Name,
    string Region,
    string City,
    decimal Rating,
    int ReviewsCount,
    string ReviewsLabel,     // "2k reseñas"
    string Category,         // category slug
    string CategoryLabel,
    string? Image,
    string? Tagline);
