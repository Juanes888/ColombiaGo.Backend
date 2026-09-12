namespace ColombiaGo.Infrastructure.Auth;

/// <summary>Binds the "Moderacion" configuration section.</summary>
public sealed class ModeracionOptions
{
    public const string SectionName = "Moderacion";

    public bool AutoAprobarCalificaciones { get; set; }
}
