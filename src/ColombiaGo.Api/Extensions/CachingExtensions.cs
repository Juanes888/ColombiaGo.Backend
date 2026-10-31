namespace ColombiaGo.Api.Extensions;

public static class CachingExtensions
{
    /// <summary>Política para el catálogo público (lugares, guías): misma respuesta para todas las personas del mismo idioma.</summary>
    public const string CatalogPolicy = "Catalogo";

    public static IServiceCollection AddCatalogCaching(this IServiceCollection services)
    {
        services.AddOutputCache(options =>
        {
            // La política por defecto no guarda nada cuando la petición trae Authorization, y la app siempre manda el token.
            // Estas respuestas no dependen de la persona (el idioma sí: va en la clave), así que es seguro guardarlas.
            options.AddPolicy(CatalogPolicy, builder => builder
                    .Expire(TimeSpan.FromMinutes(5))
                    .SetVaryByHeader("Accept-Language")
                    .SetVaryByQuery("*"),
                excludeDefaultPolicy: true);
        });

        return services;
    }
}
