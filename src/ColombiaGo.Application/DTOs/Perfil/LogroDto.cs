namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Mirrors the frontend's hardcoded <c>ACHIEVEMENTS</c> list in <c>ProfilePage</c>, now
/// backed by real per-user unlock state.</summary>
public sealed record LogroDto(
    int Id,
    string Title,
    string Text,
    string Tone,
    string Icon,
    bool Unlocked,
    DateTimeOffset? UnlockedAt);
