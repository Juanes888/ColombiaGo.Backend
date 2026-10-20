using ColombiaGo.Application.Abstractions;
using ColombiaGo.Domain.Enums;
using ColombiaGo.Infrastructure.Auth;
using ColombiaGo.Infrastructure.Ia;
using ColombiaGo.Infrastructure.Persistence;
using ColombiaGo.Infrastructure.Repositories;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ColombiaGo.Infrastructure;

/// <summary>Wires persistence, repositories, and Supabase authentication into the container.
/// Called once from the API's <c>Program.cs</c>.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // appsettings.json ships a blank "" placeholder for this key (never a real value —
        // never commit one), so an empty/whitespace string must fail exactly like a missing key.
        var connectionString = configuration.GetConnectionString("ColombiaGoDb");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Missing connection string 'ColombiaGoDb'. Set it via User Secrets or the " +
                "ConnectionStrings__ColombiaGoDb environment variable.");

        // MapEnum (not just HasPostgresEnum in the model) is what makes Npgsql send these as the
        // native Postgres enum types; without it EF falls back to int and every comparison like
        // `estado_publicacion = 1` fails with 42883. Names must match db/001_schema.sql.
        services.AddDbContext<ColombiaGoDbContext>(options => options
            .UseNpgsql(connectionString, npgsql => npgsql
                .MapEnum<RangoPrecio>("rango_precio")
                .MapEnum<EstadoPublicacion>("estado_publicacion")
                .MapEnum<EstadoModeracion>("estado_moderacion")
                .MapEnum<EstadoEvento>("estado_evento")
                .MapEnum<TipoInteraccion>("tipo_interaccion")
                .MapEnum<TipoDato>("tipo_dato")
                .MapEnum<TipoReporte>("tipo_reporte")
                .MapEnum<FormatoSalida>("formato_salida"))
            .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.Configure<ModeracionOptions>(configuration.GetSection(ModeracionOptions.SectionName));
        services.AddSingleton<IModeracionSettings, ModeracionSettings>();

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.AddHttpClient<IGeminiClient, GeminiClient>();

        services.AddScoped<UsuarioProvisioner>();
        services.AddScoped<IClaimsTransformation, RolClaimsTransformation>();

        services.AddSupabaseAuthentication(configuration);
        services.AddAuthorizationPolicies();

        return services;
    }

    private static IServiceCollection AddAuthorizationPolicies(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireAdmin", policy =>
                policy.RequireClaim(CurrentUser.RolClaimType, "admin"));

            options.AddPolicy("RequireGuia", policy =>
                policy.RequireClaim(CurrentUser.RolClaimType, "guia", "admin"));
        });

        return services;
    }
}
