using ColombiaGo.Application.DTOs.Eventos;
using ColombiaGo.Domain.Entities;

namespace ColombiaGo.Application.Mapping;

public static class EventoMappings
{
    public static EventoDto ToDto(this Evento evento, bool isFavorite) => new(
        Id: evento.Id,
        Name: evento.NombreEvento,
        Date: evento.FechaEvento,
        Time: evento.HoraEvento,
        Address: evento.DireccionEvento,
        Cost: evento.Costo,
        Description: evento.Descripcion,
        Status: evento.Estado.ToString(),
        LugarId: evento.LugarId,
        LugarNombre: evento.Lugar?.Nombre,
        IsFavorite: isFavorite);
}
