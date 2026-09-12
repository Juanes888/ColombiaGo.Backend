using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

public class FavoritoLugar : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public DateTimeOffset FechaAgregado { get; set; }
}
