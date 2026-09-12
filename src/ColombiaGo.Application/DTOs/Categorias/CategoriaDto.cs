namespace ColombiaGo.Application.DTOs.Categorias;

/// <summary>Mirrors the frontend's <c>HOME_CATEGORIES</c> / <c>CATEGORIES</c> shape.</summary>
public sealed record CategoriaDto(string Id, string Label, string? Icon, string? Tone);
