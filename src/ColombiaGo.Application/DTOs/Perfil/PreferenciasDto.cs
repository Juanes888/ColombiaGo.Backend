namespace ColombiaGo.Application.DTOs.Perfil;

/// <summary>Categorías de interés (slugs: food, culture, nature, nightlife) elegidas por la persona.</summary>
public sealed record PreferenciasDto(IReadOnlyList<string> Categorias);

public sealed record GuardarPreferenciasRequest(IReadOnlyList<string> Categorias);
