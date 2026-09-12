namespace ColombiaGo.Api.Extensions;

public static class HealthCheckExtensions
{
    public static IServiceCollection AddApiHealthChecks(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("ColombiaGoDb");

        var builder = services.AddHealthChecks();
        if (!string.IsNullOrWhiteSpace(connectionString))
            builder.AddNpgSql(connectionString, name: "postgres");

        return services;
    }
}
