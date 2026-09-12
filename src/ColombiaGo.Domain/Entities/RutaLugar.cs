using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>One stop within a <see cref="Ruta"/>, in optimized visiting order.</summary>
public class RutaLugar : Entity<int>
{
    public int RutaId { get; set; }
    public Ruta Ruta { get; set; } = null!;

    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public int OrdenOptimo { get; set; }
}
