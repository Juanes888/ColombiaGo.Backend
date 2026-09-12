namespace ColombiaGo.Application.DTOs.Eventos;

public sealed record EventoDto(
    int Id,
    string Name,
    DateOnly Date,
    TimeOnly Time,
    string Address,
    decimal? Cost,
    string? Description,
    string Status,
    int? LugarId,
    string? LugarNombre,
    bool IsFavorite);
