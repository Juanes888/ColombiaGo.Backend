using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>Wires JWT Bearer authentication to accept and validate Supabase-issued access tokens.</summary>
public static class SupabaseAuthenticationExtensions
{
    public static IServiceCollection AddSupabaseAuthentication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SupabaseAuthOptions>(configuration.GetSection(SupabaseAuthOptions.SectionName));

        var supabase = configuration.GetSection(SupabaseAuthOptions.SectionName).Get<SupabaseAuthOptions>()
            ?? new SupabaseAuthOptions();

        // appsettings.json ships a blank "" placeholder for Supabase:Url — fail at startup with a
        // clear message rather than lazily, the first time a request tries to validate a token
        // against an empty JWKS URI.
        if (string.IsNullOrWhiteSpace(supabase.Url) && string.IsNullOrWhiteSpace(supabase.JwtSecret))
            throw new InvalidOperationException(
                "Missing 'Supabase:Url' (and no 'Supabase:JwtSecret' fallback either). Set it via " +
                "User Secrets or the Supabase__Url environment variable.");

        services
            .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(jwt =>
            {
                jwt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = supabase.Issuer,
                    ValidateAudience = true,
                    ValidAudience = SupabaseAuthOptions.Audience,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ClockSkew = TimeSpan.FromSeconds(30),
                };

                if (!string.IsNullOrWhiteSpace(supabase.JwtSecret))
                {
                    // Legacy HS256 shared-secret signing.
                    jwt.TokenValidationParameters.IssuerSigningKey =
                        new SymmetricSecurityKey(Encoding.UTF8.GetBytes(supabase.JwtSecret));
                }
                else
                {
                    // Current default: asymmetric signing, public keys fetched from the JWKS.
                    var jwks = new JwksConfigurationManager(supabase.JwksUri, new HttpClient());
                    jwks.PreloadInBackground();
                    jwt.ConfigurationManager = jwks;
                }
            });

        return services;
    }
}
