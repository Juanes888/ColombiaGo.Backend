namespace ColombiaGo.Application.DTOs.Lugares;

/// <summary>Everything <c>DestinationPage</c> renders: the resumen fields plus description,
/// practical info, gallery, features and the caller's favorite/route state.</summary>
public sealed record LugarDetalleDto(
    string Id,
    string Name,
    string Region,
    string City,
    decimal Rating,
    int ReviewsCount,
    string ReviewsLabel,
    string Category,
    string CategoryLabel,
    string? Image,
    string? Tagline,
    string Description,
    string? Hours,
    string Price,
    string? Transport,
    string Address,
    decimal Latitude,
    decimal Longitude,
    IReadOnlyList<string> Gallery,
    IReadOnlyList<string> Features,
    IReadOnlyList<LugarAtributoDto> Attributes,
    bool IsFavorite,
    bool IsInRoute);
