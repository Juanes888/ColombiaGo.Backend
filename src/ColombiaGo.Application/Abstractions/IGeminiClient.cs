namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Thin abstraction over the Gemini API call (kept here, rather than injecting <c>HttpClient</c>
/// directly into Application, so Application stays free of any HTTP/config dependency — the
/// same split as <see cref="IModeracionSettings"/>). Implemented in Infrastructure.
/// </summary>
public interface IGeminiClient
{
    /// <summary>False when no API key is configured — callers should fail fast with a clear
    /// error instead of attempting a request that will never succeed.</summary>
    bool IsConfigured { get; }

    /// <summary>Sends <paramref name="prompt"/> to Gemini asking for a JSON-only response and
    /// returns the raw JSON text, or null if the model returned nothing usable.</summary>
    Task<string?> GenerarJsonAsync(string prompt, CancellationToken ct = default);
}
