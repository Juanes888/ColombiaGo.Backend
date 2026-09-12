using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Feeds <see cref="Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerOptions.ConfigurationManager"/>
/// from a bare JWKS endpoint (<c>{"keys": [...]}</c>) rather than a full OIDC discovery document —
/// which is what Supabase's <c>/.well-known/jwks.json</c> actually serves. The built-in
/// <c>ConfigurationManager&lt;OpenIdConnectConfiguration&gt;</c> that ships with ASP.NET Core
/// expects the latter, so this adapter wraps the JWKS fetch and exposes it through the same
/// interface, with an in-memory cache to avoid hitting the endpoint on every token validation.
/// </summary>
public sealed class JwksConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>, IDisposable
{
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(12);

    private readonly string _jwksUri;
    private readonly HttpClient _httpClient;
    private readonly SemaphoreSlim _refreshLock = new(1, 1);

    private OpenIdConnectConfiguration? _cached;
    private DateTimeOffset _cachedAt;

    public JwksConfigurationManager(string jwksUri, HttpClient httpClient)
    {
        _jwksUri = jwksUri;
        _httpClient = httpClient;
    }

    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
    {
        if (_cached is not null && DateTimeOffset.UtcNow - _cachedAt < CacheDuration)
            return _cached;

        await _refreshLock.WaitAsync(cancel);
        try
        {
            if (_cached is not null && DateTimeOffset.UtcNow - _cachedAt < CacheDuration)
                return _cached;

            var json = await _httpClient.GetStringAsync(_jwksUri, cancel);
            var jwks = new JsonWebKeySet(json);

            var configuration = new OpenIdConnectConfiguration();
            foreach (var key in jwks.GetSigningKeys())
                configuration.SigningKeys.Add(key);

            _cached = configuration;
            _cachedAt = DateTimeOffset.UtcNow;
            return configuration;
        }
        finally
        {
            _refreshLock.Release();
        }
    }

    public void RequestRefresh() => _cached = null;

    public void Dispose() => _refreshLock.Dispose();
}
