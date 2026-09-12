namespace ColombiaGo.Application.Abstractions;

/// <summary>
/// Identity of the caller, resolved once per request from the validated Supabase JWT.
/// Implemented in Infrastructure so Application never touches JWT/claims parsing directly.
/// </summary>
public interface ICurrentUser
{
    bool IsAuthenticated { get; }

    /// <summary>The Supabase <c>auth.users</c> id (JWT <c>sub</c> claim). Null when anonymous.</summary>
    Guid? UserId { get; }

    string? Email { get; }

    /// <summary>Role slug (e.g. "turista", "guia", "admin") as recorded in <c>usuarios.rol_id</c>.</summary>
    string? Role { get; }

    bool IsInRole(string role);

    /// <summary>Throws <see cref="UnauthorizedAccessException"/> if anonymous; otherwise returns
    /// <see cref="UserId"/>. Convenience for endpoints that require auth (enforced again by
    /// <c>[Authorize]</c> at the controller level — this is a defensive second layer).</summary>
    Guid RequireUserId();
}
