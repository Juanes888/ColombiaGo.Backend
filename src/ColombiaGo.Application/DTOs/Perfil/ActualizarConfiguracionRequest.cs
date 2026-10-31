namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Language and OnboardingCompleted are optional so existing clients that only send the
/// three toggles keep working unchanged.</summary>
public sealed record ActualizarConfiguracionRequest(
    bool Notifications, bool Location, bool ReduceMotion, string? Language = null, bool? OnboardingCompleted = null);
