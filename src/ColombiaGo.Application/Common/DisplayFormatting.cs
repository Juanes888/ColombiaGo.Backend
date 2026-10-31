using System.Globalization;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Common;

/// <summary>
/// Formats domain values into the short display strings the frontend renders (e.g. "2k reseñas",
/// "Desde $15.000"), in the caller's language. Centralized so every mapper produces the same labels.
/// Money is always shown in Colombian pesos with Colombian separators, whatever the language.
/// </summary>
public static class DisplayFormatting
{
    private static readonly CultureInfo CopCulture = CultureInfo.GetCultureInfo("es-CO");

    private sealed record Textos(
        string ResenaSingular, string ResenaPlural, string ResenaSufijoCorto,
        string Gratis, string Desde, bool DesdeEsSufijo,
        string Economico, string Moderado, string Costoso, string Lujo, string SegunPlan,
        string PorHora);

    private static readonly Dictionary<string, Textos> PorIdioma = new()
    {
        ["es"] = new("reseña", "reseñas", "reseñas", "Gratis", "Desde", false,
            "Económico", "Moderado", "Costoso", "Lujo", "Según plan", "{0} por hora"),
        ["en"] = new("review", "reviews", "reviews", "Free", "From", false,
            "Budget", "Moderate", "Expensive", "Luxury", "Depends on plan", "{0} per hour"),
        ["pt"] = new("avaliação", "avaliações", "avaliações", "Grátis", "A partir de", false,
            "Econômico", "Moderado", "Caro", "Luxo", "Conforme o plano", "{0} por hora"),
        ["zh"] = new("条评价", "条评价", "条评价", "免费", "起", true,
            "经济实惠", "适中", "较贵", "奢华", "视安排而定", "{0} / 小时"),
        ["fr"] = new("avis", "avis", "avis", "Gratuit", "À partir de", false,
            "Économique", "Modéré", "Cher", "Luxe", "Selon le plan", "{0} par heure"),
    };

    private static Textos T(string idioma) => PorIdioma[Idiomas.Normalizar(idioma)];

    /// <summary>"2k reseñas", "980 reviews", "1.4k 条评价" — equivalent of <c>Destination.reviews</c> in the front.</summary>
    public static string ToReviewsLabel(int count, string idioma = Idiomas.Espanol)
    {
        var t = T(idioma);
        if (count < 1000)
            return $"{count} {(count == 1 ? t.ResenaSingular : t.ResenaPlural)}";

        var thousands = count / 1000m;
        var rounded = Math.Round(thousands, 1);
        var text = rounded == Math.Floor(rounded)
            ? rounded.ToString("0", CultureInfo.InvariantCulture)
            : rounded.ToString("0.#", CultureInfo.InvariantCulture);
        return $"{text}k {t.ResenaSufijoCorto}";
    }

    /// <summary>"Gratis", "Desde $15.000", "Económico" (or its translation).</summary>
    public static string ToPriceLabel(RangoPrecio rango, decimal? costoEntrada, string idioma = Idiomas.Espanol)
    {
        var t = T(idioma);

        if (rango == RangoPrecio.Gratis)
            return t.Gratis;

        if (costoEntrada is > 0)
            return t.DesdeEsSufijo
                ? $"{FormatCop(costoEntrada.Value)} {t.Desde}"
                : $"{t.Desde} {FormatCop(costoEntrada.Value)}";

        return rango switch
        {
            RangoPrecio.Economico => t.Economico,
            RangoPrecio.Moderado => t.Moderado,
            RangoPrecio.Costoso => t.Costoso,
            RangoPrecio.Lujo => t.Lujo,
            _ => t.SegunPlan,
        };
    }

    /// <summary>"$70.000 por hora" (or its translation) — the guide card's price.</summary>
    public static string ToHourlyPriceLabel(decimal tarifaHora, string idioma = Idiomas.Espanol) =>
        string.Format(CultureInfo.InvariantCulture, T(idioma).PorHora, FormatCop(tarifaHora));

    private static string FormatCop(decimal value) => "$" + value.ToString("N0", CopCulture);
}
