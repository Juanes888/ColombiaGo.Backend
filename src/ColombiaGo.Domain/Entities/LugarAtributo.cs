using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>Value of a category-specific dynamic attribute (see <see cref="TipoAtributo"/>) for
/// one place. Value is stored as text and parsed per <c>TipoAtributo.TipoDato</c>.</summary>
public class LugarAtributo : Entity<int>
{
    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public int TipoAtributoId { get; set; }
    public TipoAtributo TipoAtributo { get; set; } = null!;

    public string Valor { get; set; } = string.Empty;
}
