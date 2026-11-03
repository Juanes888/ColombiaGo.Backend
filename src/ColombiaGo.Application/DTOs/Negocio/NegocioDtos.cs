using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.DTOs.Negocio;

/// <summary>
/// Datos que llena un negocio para publicar (o editar) un lugar. Latitud/longitud son opcionales: si no vienen, se
/// buscan a partir de la dirección.
/// </summary>
public sealed record GuardarLugarRequest(
    string Name,
    string Category,          // food | culture | nature | nightlife
    string City,
    string Region,
    string Address,
    string Description,
    string? Tagline,
    string? Hours,
    string? HowToGet,
    RangoPrecio PriceRange,
    decimal? EntryCost,
    string? ImageUrl,
    decimal? Latitude,
    decimal? Longitude);

/// <summary>Un lugar publicado por el negocio, con su estado de moderación.</summary>
public sealed record MiLugarDto(
    int Id,
    string Slug,
    string Name,
    string Category,
    string City,
    string Region,
    string Address,
    string Description,
    string? Tagline,
    string? Hours,
    string? HowToGet,
    string PriceRange,
    decimal? EntryCost,
    string? Image,
    decimal Latitude,
    decimal Longitude,
    string Status,            // pendiente | aprobado | rechazado | archivado
    string? RejectReason,
    decimal Rating,
    int ReviewsCount,
    DateTimeOffset UpdatedAt);
