using ColombiaGo.Application.DTOs.Perfil;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class PerfilMappings
{
    public static PerfilDto ToPerfilDto(this Usuario usuario, int places, int guides, int reviews)
    {
        var firstName = usuario.NombreCompleto.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault() ?? usuario.NombreCompleto;

        return new PerfilDto(
            Id: usuario.Id,
            Name: usuario.NombreCompleto,
            FirstName: firstName,
            Level: usuario.Nivel,
            Photo: usuario.FotoPerfilUrl,
            Places: places,
            Guides: guides,
            Reviews: reviews);
    }

    public static ConfiguracionDto ToDto(this UsuarioConfiguracion config) =>
        new(config.Notificaciones, config.Ubicacion, config.ReduceMotion);

    public static LogroDto ToLogroDto(this Logro logro, bool unlocked, DateTimeOffset? unlockedAt) =>
        new(logro.Id, logro.Titulo, logro.Descripcion, logro.Tono, logro.Icono, unlocked, unlockedAt);
}
