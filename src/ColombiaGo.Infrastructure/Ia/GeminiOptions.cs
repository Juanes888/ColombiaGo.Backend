namespace ColombiaGo.Infrastructure.Ia;

/// <summary>Binds the "Gemini" configuration section.</summary>
public sealed class GeminiOptions
{
    public const string SectionName = "Gemini";

    public string? ApiKey { get; set; }

    /// <summary>Model id as accepted by the Generative Language API's generateContent endpoint.</summary>
    public string Model { get; set; } = "gemini-3.1-flash-lite";
}
