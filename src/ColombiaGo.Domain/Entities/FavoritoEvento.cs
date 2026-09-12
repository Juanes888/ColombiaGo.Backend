using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

public class FavoritoEvento : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int EventoId { get; set; }
    public Evento Evento { get; set; } = null!;

    public DateTimeOffset FechaAgregado { get; set; }
}
