namespace ColombiaGo.Application.DTOs.Eventos;

public sealed record CrearEventoRequest(
    string NombreEvento,
    DateOnly FechaEvento,
    TimeOnly HoraEvento,
    string DireccionEvento,
    decimal? Costo,
    string? Descripcion,
    int? LugarId);
