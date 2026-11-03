using ColombiaGo.Application.DTOs.Admin;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class AdminMappings
{
    public static LugarPendienteDto ToPendienteDto(this Lugar lugar) => new(
        Id: lugar.Id,
        Nombre: lugar.Nombre,
        Slug: lugar.Slug,
        Categoria: lugar.Categoria.Nombre,
        Ciudad: lugar.Ciudad,
        RegistradoPorNombre: lugar.UsuarioRegistrador?.NombreCompleto,
        RegistradoPorEmail: lugar.UsuarioRegistrador?.CorreoElectronico,
        FechaRegistro: lugar.FechaRegistro,
        Descripcion: lugar.Descripcion,
        Direccion: lugar.Direccion,
        ImagenUrl: lugar.ImagenPortadaUrl,
        Latitud: lugar.Latitud,
        Longitud: lugar.Longitud);

    public static CalificacionPendienteDto ToPendienteDto(this Calificacion calificacion) => new(
        Id: calificacion.Id,
        LugarId: calificacion.LugarId,
        LugarNombre: calificacion.Lugar.Nombre,
        UsuarioNombre: calificacion.Usuario.NombreCompleto,
        UsuarioFoto: calificacion.Usuario.FotoPerfilUrl,
        Puntuacion: calificacion.Puntuacion,
        Comentario: calificacion.Comentario,
        Fecha: calificacion.Fecha);
}
