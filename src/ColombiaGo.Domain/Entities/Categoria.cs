using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Place category (Gastronomía, Cultura, Naturaleza, Vida nocturna, …). <see cref="Slug"/> and
/// <see cref="Tono"/> are additions beyond the original ER model — the frontend keys categories
/// by a stable slug (food/culture/nature/nightlife) and colors chips by a tone (yellow/blue/…),
/// neither of which the Spanish display label <see cref="Nombre"/> can serve on its own.
/// </summary>
public class Categoria : Entity<int>
{
    public string Nombre { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string? Icono { get; set; }
    public string? Tono { get; set; }
    public bool Activa { get; set; } = true;

    public ICollection<Lugar> Lugares { get; set; } = new List<Lugar>();
    public ICollection<TipoAtributo> TiposAtributo { get; set; } = new List<TipoAtributo>();
    public ICollection<Caracteristica> Caracteristicas { get; set; } = new List<Caracteristica>();
    public ICollection<UsuarioPreferencia> Preferencias { get; set; } = new List<UsuarioPreferencia>();
}
