namespace ColombiaGo.Application.DTOs.Admin;

public sealed record CalificacionPendienteDto(
    int Id,
    int LugarId,
    string LugarNombre,
    string UsuarioNombre,
    string? UsuarioFoto,
    int Puntuacion,
    string? Comentario,
    DateTimeOffset Fecha);
