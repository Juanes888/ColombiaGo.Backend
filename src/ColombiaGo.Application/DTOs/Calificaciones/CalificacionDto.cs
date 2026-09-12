namespace ColombiaGo.Application.DTOs.Calificaciones;

public sealed record CalificacionDto(
    int Id,
    string UsuarioNombre,
    string? UsuarioFoto,
    int Puntuacion,
    string? Comentario,
    DateTimeOffset Fecha);
