using ColombiaGo.Application.Abstractions;

namespace ColombiaGo.UnitTests.TestHelpers;

/// <summary>Simple test double — real NSubstitute mocking would be overkill for an interface
/// this small and behavior this deterministic.</summary>
public sealed class FakeCurrentUser : ICurrentUser
{
    public FakeCurrentUser(Guid? userId = null, string? role = null)
    {
        UserId = userId;
        Role = role;
    }

    public bool IsAuthenticated => UserId is not null;

    public Guid? UserId { get; }

    public string? Email => IsAuthenticated ? "test@colombiago.dev" : null;

    public string? Role { get; }

    public bool IsInRole(string role) => string.Equals(Role, role, StringComparison.OrdinalIgnoreCase);

    public Guid RequireUserId() =>
        UserId ?? throw new UnauthorizedAccessException("This operation requires an authenticated user.");
}
