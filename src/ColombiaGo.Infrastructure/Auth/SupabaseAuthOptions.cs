namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Binds the "Supabase" configuration section. Populate via User Secrets or environment
/// variables in every environment — never commit real values to <c>appsettings.json</c>.
/// </summary>
public sealed class SupabaseAuthOptions
{
    public const string SectionName = "Supabase";
    public const string Audience = "authenticated";

    /// <summary>Project URL, e.g. <c>https://&lt;project-ref&gt;.supabase.co</c>.</summary>
    public string Url { get; set; } = string.Empty;

    /// <summary>Legacy HS256 shared JWT secret (Supabase project settings → API → JWT Secret).
    /// Set this only for older projects still on symmetric signing; leave null to use the
    /// current default (asymmetric signing, keys fetched from <see cref="JwksUri"/>).</summary>
    public string? JwtSecret { get; set; }

    public string Issuer => $"{Url.TrimEnd('/')}/auth/v1";

    public string JwksUri => $"{Url.TrimEnd('/')}/auth/v1/.well-known/jwks.json";
}
