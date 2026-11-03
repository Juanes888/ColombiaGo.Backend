using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Application.Abstractions;

namespace ColombiaGo.Infrastructure.Geo;

/// <summary>
/// Geocodificador con Nominatim (OpenStreetMap): gratis y sin API key, con un límite de ~1 petición por segundo, de
/// sobra para negocios que publican lugares de vez en cuando. Busca solo en Colombia.
/// </summary>
public sealed class NominatimGeocoder : IGeocoder
{
    private readonly HttpClient _http;

    public NominatimGeocoder(HttpClient http) => _http = http;

    public async Task<(decimal Latitud, decimal Longitud)?> BuscarAsync(
        string direccion, string ciudad, string region, CancellationToken ct = default)
    {
        // Primero la dirección completa; si no aparece (direcciones colombianas tipo "Cra 70 #44-21" a veces no
        // están en OSM), al menos se ubica en la ciudad para que el negocio quede en el mapa y luego se ajuste.
        foreach (var consulta in new[] { $"{direccion}, {ciudad}, {region}", $"{ciudad}, {region}" })
        {
            var url = "https://nominatim.openstreetmap.org/search?format=jsonv2&limit=1&countrycodes=co&q="
                      + Uri.EscapeDataString(consulta);
            try
            {
                var resultados = await _http.GetFromJsonAsync<List<ResultadoNominatim>>(url, ct);
                var primero = resultados?.FirstOrDefault();
                if (primero is not null
                    && decimal.TryParse(primero.Lat, NumberStyles.Float, CultureInfo.InvariantCulture, out var lat)
                    && decimal.TryParse(primero.Lon, NumberStyles.Float, CultureInfo.InvariantCulture, out var lon))
                {
                    return (lat, lon);
                }
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            {
                return null;
            }
        }

        return null;
    }

    private sealed class ResultadoNominatim
    {
        [JsonPropertyName("lat")] public string Lat { get; set; } = "";
        [JsonPropertyName("lon")] public string Lon { get; set; } = "";
    }
}
