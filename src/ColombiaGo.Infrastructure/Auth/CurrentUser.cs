using System.Security.Claims;
using ColombiaGo.Application.Abstractions;
using Microsoft.AspNetCore.Http;

namespace ColombiaGo.Infrastructure.Auth;

/// <summary>
/// Reads identity off the current request's validated <see cref="ClaimsPrincipal"/> — the
/// Supabase JWT's <c>sub</c> claim for the user id, and the app-specific role claim added by
/// <see cref="RolClaimsTransformation"/> (Supabase's own "role" claim carries the Postgres role
/// "authenticated"/"anon", not our app's rol_id, so a separate claim type is used to avoid
/// colliding with it).
/// </summary>
public sealed class CurrentUser : ICurrentUser
{
    public const string RolClaimType = "colombiago:rol";

    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUser(IHttpContextAccessor httpContextAccessor) => _httpContextAccessor = httpContextAccessor;

    private ClaimsPrincipal? Principal => _httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated => Principal?.Identity?.IsAuthenticated ?? false;

    public Guid? UserId
    {
        get
        {
            var sub = Principal?.FindFirstValue(ClaimTypes.NameIdentifier) ?? Principal?.FindFirstValue("sub");
            return Guid.TryParse(sub, out var id) ? id : null;
        }
    }

    public string? Email => Principal?.FindFirstValue(ClaimTypes.Email) ?? Principal?.FindFirstValue("email");

    public string? Role => Principal?.FindFirstValue(RolClaimType);

    public bool IsInRole(string role) =>
        string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

    public Guid RequireUserId() =>
        UserId ?? throw new UnauthorizedAccessException("This operation requires an authenticated user.");
}
