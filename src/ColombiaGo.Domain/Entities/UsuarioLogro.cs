using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>Join entity recording that a user unlocked a given <see cref="Logro"/> and when.</summary>
public class UsuarioLogro : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int LogroId { get; set; }
    public Logro Logro { get; set; } = null!;

    public DateTimeOffset FechaObtenido { get; set; }
}
