namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Mirrors the frontend Settings type, plus the app language (es, en, pt, zh, fr) and
/// whether the person already finished the welcome tour.</summary>
public sealed record ConfiguracionDto(
    bool Notifications, bool Location, bool ReduceMotion, string Language, bool OnboardingCompleted);
