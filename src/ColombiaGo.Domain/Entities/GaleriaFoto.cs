using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>One photo in a place's gallery, in display order.</summary>
public class GaleriaFoto : Entity<int>
{
    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public string UrlImagen { get; set; } = string.Empty;
    public int Orden { get; set; }
}
