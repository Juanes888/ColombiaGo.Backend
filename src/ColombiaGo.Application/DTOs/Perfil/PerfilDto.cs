namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Mirrors the frontend's <c>USER</c> object in <c>src/data/destinations.ts</c> plus its
/// stat-card counters. <see cref="Guides"/> currently always reports 0 — there is no
/// guide-booking/usage table yet to count against; see docs/api-contract.md.</summary>
public sealed record PerfilDto(
    Guid Id,
    string Name,
    string FirstName,
    string Level,
    string? Photo,
    int Places,
    int Guides,
    int Reviews);
