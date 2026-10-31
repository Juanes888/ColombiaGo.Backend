using ColombiaGo.Application.Abstractions;
using ColombiaGo.Application.Services;
using ColombiaGo.Infrastructure.Persistence;
using ColombiaGo.Infrastructure.Repositories;

namespace ColombiaGo.UnitTests.TestHelpers;

public sealed class FakeIdiomaActual : IIdiomaActual
{
    public FakeIdiomaActual(string codigo = "es") => Codigo = codigo;

    public string Codigo { get; }
}

/// <summary>Gemini de mentira: devuelve las respuestas JSON que se le programan, en orden, y registra los prompts.</summary>
public sealed class FakeGeminiClient : IGeminiClient
{
    private readonly Queue<string?> _respuestas;

    public FakeGeminiClient(bool configurado = true, params string?[] respuestas)
    {
        IsConfigured = configurado;
        _respuestas = new Queue<string?>(respuestas);
    }

    public bool IsConfigured { get; }

    public List<string> Prompts { get; } = [];

    public Task<string?> GenerarJsonAsync(string prompt, CancellationToken ct = default)
    {
        Prompts.Add(prompt);
        return Task.FromResult(_respuestas.Count > 0 ? _respuestas.Dequeue() : null);
    }
}

public static class TestServices
{
    public static LugarLocalizador Localizador(
        ColombiaGoDbContext context, string idioma = "es", IGeminiClient? gemini = null) =>
        new(new UnitOfWork(context), new FakeIdiomaActual(idioma), gemini ?? new FakeGeminiClient(configurado: false));
}
