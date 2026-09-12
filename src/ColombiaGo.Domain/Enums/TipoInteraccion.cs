namespace ColombiaGo.Domain.Enums;

/// <summary>Kind of interaction recorded in <c>historial_interacciones</c>, used to feed the
/// recommendation engine.</summary>
public enum TipoInteraccion
{
    Vista = 0,
    Busqueda = 1,
    Click = 2,
    Compartido = 3,
    Favorito = 4,
}
