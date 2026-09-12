using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>Join entity: a feature/amenity a place offers.</summary>
public class LugarCaracteristica : Entity<int>
{
    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public int CaracteristicaId { get; set; }
    public Caracteristica Caracteristica { get; set; } = null!;
}
