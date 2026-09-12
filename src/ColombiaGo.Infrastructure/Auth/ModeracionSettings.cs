using ColombiaGo.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ColombiaGo.Infrastructure.Auth;

public sealed class ModeracionSettings : IModeracionSettings
{
    private readonly ModeracionOptions _options;

    public ModeracionSettings(IOptions<ModeracionOptions> options) => _options = options.Value;

    public bool AutoAprobarCalificaciones => _options.AutoAprobarCalificaciones;
}
