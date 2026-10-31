namespace ColombiaGo.Application.Common;

/// <summary>Idiomas soportados por la app. El español es el idioma base del contenido.</summary>
public static class Idiomas
{
    public const string Espanol = "es";

    public static readonly IReadOnlyList<string> Soportados = ["es", "en", "pt", "zh", "fr"];

    public static bool EsSoportado(string? codigo) =>
        codigo is not null && Soportados.Contains(codigo, StringComparer.OrdinalIgnoreCase);

    /// <summary>Normaliza "zh-CN", "PT-br", "en_US"… al código de dos letras soportado; si no existe, español.</summary>
    public static string Normalizar(string? codigo)
    {
        if (string.IsNullOrWhiteSpace(codigo)) return Espanol;
        var corto = codigo.Trim().ToLowerInvariant().Split('-', '_', ';')[0];
        return EsSoportado(corto) ? corto : Espanol;
    }

    /// <summary>Nombre del idioma en inglés, para incluirlo en los prompts de la IA.</summary>
    public static string NombreParaPrompt(string codigo) => Normalizar(codigo) switch
    {
        "en" => "English",
        "pt" => "Brazilian Portuguese",
        "zh" => "Simplified Chinese (Mandarin)",
        "fr" => "French",
        _ => "Spanish",
    };
}
