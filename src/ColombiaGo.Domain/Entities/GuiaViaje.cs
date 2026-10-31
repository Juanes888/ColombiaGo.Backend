using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Guía de viaje día por día generada con IA para una persona (ciudad, días, presupuesto e intereses).
/// <see cref="Contenido"/> guarda el plan como JSON referenciando lugares por slug; así la guía siempre
/// se muestra con los datos vigentes de cada lugar. No confundir con <see cref="Guia"/> (guía turístico).
/// </summary>
public class GuiaViaje : Entity<int>
{
    public Guid UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string Ciudad { get; set; } = string.Empty;
    public int Dias { get; set; }
    public decimal Presupuesto { get; set; }
    public List<string> Intereses { get; set; } = new();
    public string Idioma { get; set; } = "es";

    /// <summary>JSON (columna jsonb) con el plan generado.</summary>
    public string Contenido { get; set; } = "{}";

    public DateTimeOffset FechaCreacion { get; set; }
}
