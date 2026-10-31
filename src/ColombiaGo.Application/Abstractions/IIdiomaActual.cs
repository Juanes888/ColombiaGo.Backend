namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Idioma con el que la persona está usando la app en esta petición (cabecera <c>Accept-Language</c>).
/// Siempre devuelve uno de los soportados (es, en, pt, zh, fr); si no viene o no se reconoce, "es".
/// </summary>
public interface IIdiomaActual
{
    string Codigo { get; }
}
