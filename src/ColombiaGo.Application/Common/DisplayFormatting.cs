using System.Globalization;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Application.Common;

/// <summary>
/// Formats domain values into the short display strings the frontend already renders (e.g.
/// "2k reseñas", "Desde $15.000"). Centralized here so every mapper produces the same labels
/// instead of each DTO re-implementing its own formatting.
/// </summary>
public static class DisplayFormatting
{
    private static readonly CultureInfo CopCulture = CultureInfo.GetCultureInfo("es-CO");

    /// <summary>"2k reseñas", "980 reseñas" — matches <c>Destination.reviews</c> in the frontend mock.</summary>
    public static string ToReviewsLabel(int count)
    {
        if (count < 1000)
            return $"{count} reseña{(count == 1 ? string.Empty : "s")}";

        var thousands = count / 1000m;
        var rounded = Math.Round(thousands, 1);
        var text = rounded == Math.Floor(rounded)
            ? rounded.ToString("0", CultureInfo.InvariantCulture)
            : rounded.ToString("0.#", CultureInfo.InvariantCulture);
        return $"{text}k reseñas";
    }

    /// <summary>"Gratis", "Desde $15.000", "Económico" — matches <c>Destination.price</c>.</summary>
    public static string ToPriceLabel(RangoPrecio rango, decimal? costoEntrada)
    {
        if (rango == RangoPrecio.Gratis)
            return "Gratis";

        if (costoEntrada is > 0)
            return $"Desde {FormatCop(costoEntrada.Value)}";

        return rango switch
        {
            RangoPrecio.Economico => "Económico",
            RangoPrecio.Moderado => "Moderado",
            RangoPrecio.Costoso => "Costoso",
            RangoPrecio.Lujo => "Lujo",
            _ => "Según plan",
        };
    }

    /// <summary>"$70.000 por hora" — matches the guide card's <c>price</c> field.</summary>
    public static string ToHourlyPriceLabel(decimal tarifaHora) => $"{FormatCop(tarifaHora)} por hora";

    private static string FormatCop(decimal value) => "$" + value.ToString("N0", CopCulture);
}
