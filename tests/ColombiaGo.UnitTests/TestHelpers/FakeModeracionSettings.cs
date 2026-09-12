using ColombiaGo.Application.Abstractions;

namespace ColombiaGo.UnitTests.TestHelpers;

public sealed class FakeModeracionSettings : IModeracionSettings
{
    public FakeModeracionSettings(bool autoAprobar = false) => AutoAprobarCalificaciones = autoAprobar;

    public bool AutoAprobarCalificaciones { get; }
}
