using ColombiaGo.Domain.Common;
using ColombiaGo.Domain.Enums;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Defines a dynamic (EAV) attribute available to places of a given category — e.g. "tipo de
/// cocina" only makes sense for Gastronomía. Kept strictly for category-specific, variable
/// attributes; fixed fields every place has (city, region, price range, …) live as real columns
/// on <see cref="Lugar"/> instead, so common listing/filtering queries never need an EAV join.
/// </summary>
public class TipoAtributo : Entity<int>
{
    public int CategoriaId { get; set; }
    public Categoria Categoria { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public string Etiqueta { get; set; } = string.Empty;
    public TipoDato TipoDato { get; set; }
    public bool EsFiltrable { get; set; }
    public bool EsObligatorio { get; set; }

    public ICollection<LugarAtributo> LugarAtributos { get; set; } = new List<LugarAtributo>();
}
