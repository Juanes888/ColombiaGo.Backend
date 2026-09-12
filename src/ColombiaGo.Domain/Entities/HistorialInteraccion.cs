using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>Append-only log of user-place interactions, the raw signal the recommendation
/// engine (<see cref="Recomendacion"/>) is computed from.</summary>
public class HistorialInteraccion : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public TipoInteraccion TipoInteraccion { get; set; }
    public DateTimeOffset Fecha { get; set; }
}
