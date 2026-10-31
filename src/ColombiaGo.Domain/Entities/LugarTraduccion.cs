using ColombiaGo.Domain.Common;

namespace ColombiaGo.Domain.Entities;

/// <summary>
/// Traducción en caché del contenido textual de un <see cref="Lugar"/> a un idioma (en, pt, zh, fr;
/// el español es el idioma base y vive en la propia fila del lugar). Se genera una sola vez con IA la
/// primera vez que alguien pide ese lugar en ese idioma y se reutiliza después.
/// </summary>
public class LugarTraduccion : Entity<int>
{
    public int LugarId { get; set; }
    public Lugar Lugar { get; set; } = null!;

    public string Idioma { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? Eslogan { get; set; }
    public string? ComoLlegar { get; set; }
    public string? HorarioAtencion { get; set; }

    public DateTimeOffset FechaCreacion { get; set; }
}
