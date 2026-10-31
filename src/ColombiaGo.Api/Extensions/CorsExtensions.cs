namespace ColombiaGo.Api.Extensions;

public static class CorsExtensions
{
    public const string FrontendPolicy = "Frontend";

    public static IServiceCollection AddFrontendCors(
        this IServiceCollection services, IConfiguration configuration, IHostEnvironment environment)
    {
        var origins = configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? ["http://localhost:5173"];

        services.AddCors(options =>
        {
            options.AddPolicy(FrontendPolicy, policy =>
            {
                // En desarrollo el front web de Expo cambia de puerto/host (8081, 8099, 19006, la IP de la red...):
                // se acepta cualquier origen. El API usa Bearer token (sin cookies), así que no se expone nada extra.
                // En producción solo se aceptan los orígenes de Cors:AllowedOrigins.
                if (environment.IsDevelopment())
                    policy.AllowAnyOrigin();
                else
                    policy.WithOrigins(origins);

                policy.AllowAnyHeader().AllowAnyMethod();
            });
        });

        return services;
    }
}
