using ColombiaGo.Application.DTOs.Calificaciones;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class CalificacionMappings
{
    public static CalificacionDto ToDto(this Calificacion calificacion) => new(
        Id: calificacion.Id,
        UsuarioNombre: calificacion.Usuario.NombreCompleto,
        UsuarioFoto: calificacion.Usuario.FotoPerfilUrl,
        Puntuacion: calificacion.Puntuacion,
        Comentario: calificacion.Comentario,
        Fecha: calificacion.Fecha);
}
