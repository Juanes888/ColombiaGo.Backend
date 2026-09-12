using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Runs once per authenticated request after JWT validation succeeds: resolves (provisioning if
/// needed) the caller's <see cref="Domain.Entities.Usuario"/> and stamps their role onto the
/// principal as <see cref="CurrentUser.RolClaimType"/>, which <see cref="CurrentUser"/> and the
/// <c>RequireAdmin</c>/<c>RequireGuia</c> authorization policies read from.
/// </summary>
public sealed class RolClaimsTransformation : IClaimsTransformation
{
    private readonly UsuarioProvisioner _provisioner;

    public RolClaimsTransformation(UsuarioProvisioner provisioner) => _provisioner = provisioner;

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity is not ClaimsIdentity identity || !identity.IsAuthenticated)
            return principal;

        if (identity.HasClaim(c => c.Type == CurrentUser.RolClaimType))
            return principal;

        var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? principal.FindFirstValue("sub");
        if (!Guid.TryParse(sub, out var userId))
            return principal;

        var email = principal.FindFirstValue(ClaimTypes.Email) ?? principal.FindFirstValue("email");
        var nombreCompleto = principal.FindFirstValue("full_name") ?? principal.FindFirstValue("name");

        var usuario = await _provisioner.GetOrCreateAsync(userId, email, nombreCompleto);
        identity.AddClaim(new Claim(CurrentUser.RolClaimType, usuario.Rol.Nombre));

        return principal;
    }
}
