namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Moderation policy toggles, sourced from configuration in Infrastructure. Kept as an
/// abstraction (rather than injecting <c>IConfiguration</c> directly into Application) so
/// Application has zero dependency on any configuration library.
/// </summary>
public interface IModeracionSettings
{
    /// <summary>When true, new reviews are auto-approved instead of entering the moderation
    /// queue — useful for local/dev so ratings show up immediately without an admin step.</summary>
    bool AutoAprobarCalificaciones { get; }
}
