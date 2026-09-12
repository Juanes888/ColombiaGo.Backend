namespace ColombiaGo.Application.DTOs.Favoritos;

/// <summary>Result of a toggle-favorite call, so the client can update its UI without a refetch.</summary>
public sealed record FavoritoEstadoDto(bool IsFavorite);
