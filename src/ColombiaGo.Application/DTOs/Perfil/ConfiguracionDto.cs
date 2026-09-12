namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Mirrors the frontend's <c>Settings</c> type in <c>SettingsPage</c> (today persisted
/// only to localStorage).</summary>
public sealed record ConfiguracionDto(bool Notifications, bool Location, bool ReduceMotion);
