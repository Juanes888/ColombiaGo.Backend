namespace ColombiaGo.Domain.Common;

/// <summary>
/// Thrown when a domain invariant is violated (e.g. constructing an entity with impossible
/// state). Reserved for programmer errors / broken invariants — expected, recoverable failures
/// (not found, validation, conflict) should use <see cref="Result"/> instead.
/// </summary>
public class DomainException : Exception
{
    public DomainException(string message) : base(message)
    {
    }

    public DomainException(string message, Exception innerException) : base(message, innerException)
    {
    }
}
