namespace ColombiaGo.Application.DTOs.Guias;

/// <summary>Perfil que el propio guía crea o edita desde su panel. Queda sin verificar hasta que un admin lo revisa.</summary>
public sealed record GuardarGuiaRequest(
    string Name,
    string Specialty,
    string Place,
    decimal HourlyRate,
    IReadOnlyList<string> Languages,   // códigos: ES, EN, PT, FR, ZH…
    string? Bio,
    int ExperienceYears,
    string? PhotoUrl);

/// <summary>Un admin marca (o desmarca) a un guía como verificado; solo los verificados aparecen en la app.</summary>
public sealed record VerificarGuiaRequest(bool Verificado);
