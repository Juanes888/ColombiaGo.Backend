using System.Net.Http.Json;
using System.Text.Json.Serialization;
using ColombiaGo.Application.Abstractions;
using Microsoft.Extensions.Options;

namespace ColombiaGo.Infrastructure.Ia;

public sealed class GeminiClient : IGeminiClient
{
    private readonly HttpClient _http;
    private readonly GeminiOptions _options;

    public GeminiClient(HttpClient http, IOptions<GeminiOptions> options)
    {
        _http = http;
        _options = options.Value;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_options.ApiKey);

    public async Task<string?> GenerarJsonAsync(string prompt, CancellationToken ct = default)
    {
        var url = $"https://generativelanguage.googleapis.com/v1beta/models/{_options.Model}:generateContent?key={_options.ApiKey}";

        var body = new GeminiRequest
        {
            Contents = [new GeminiContent { Parts = [new GeminiPart { Text = prompt }] }],
            GenerationConfig = new GeminiGenerationConfig { ResponseMimeType = "application/json" },
        };

        // Gemini devuelve 503 ocasionalmente cuando el modelo está saturado — es transitorio,
        // así que vale la pena un segundo intento antes de darle el error al usuario.
        const int maxIntentos = 2;
        HttpResponseMessage? response = null;
        for (var intento = 1; intento <= maxIntentos; intento++)
        {
            response = await _http.PostAsJsonAsync(url, body, ct);
            if (response.StatusCode != System.Net.HttpStatusCode.ServiceUnavailable || intento == maxIntentos)
                break;

            response.Dispose();
            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        using (response)
        {
            response!.EnsureSuccessStatusCode();
            var payload = await response.Content.ReadFromJsonAsync<GeminiResponse>(cancellationToken: ct);
            return payload?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text;
        }
    }

    private sealed class GeminiRequest
    {
        [JsonPropertyName("contents")] public List<GeminiContent> Contents { get; set; } = [];
        [JsonPropertyName("generationConfig")] public GeminiGenerationConfig? GenerationConfig { get; set; }
    }

    private sealed class GeminiGenerationConfig
    {
        [JsonPropertyName("responseMimeType")] public string? ResponseMimeType { get; set; }
    }

    private sealed class GeminiResponse
    {
        [JsonPropertyName("candidates")] public List<GeminiCandidate>? Candidates { get; set; }
    }

    private sealed class GeminiCandidate
    {
        [JsonPropertyName("content")] public GeminiContent? Content { get; set; }
    }

    private sealed class GeminiContent
    {
        [JsonPropertyName("parts")] public List<GeminiPart> Parts { get; set; } = [];
    }

    private sealed class GeminiPart
    {
        [JsonPropertyName("text")] public string? Text { get; set; }
    }
}
