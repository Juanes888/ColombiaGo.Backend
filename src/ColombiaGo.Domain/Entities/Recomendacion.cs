using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>A personalized place suggestion computed for a user, with a score and the reason it
/// was surfaced.</summary>
public class Recomendacion : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public decimal Score { get; set; }
    public string? Motivo { get; set; }
    public DateTimeOffset FechaGenerada { get; set; }
}
