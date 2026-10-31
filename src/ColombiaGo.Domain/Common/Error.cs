using System.Diagnostics.CodeAnalysis;

namespace ColombiaGo.Domain.Common;

/// <summary>
/// A structured, serializable failure reason used by <see cref="Result{T}"/>. Distinct from
/// exceptions: an <see cref="Error"/> represents an expected, handleable outcome (not found,
/// validation failed, conflict) that the API layer translates into the right HTTP status code —
/// exceptions stay reserved for truly unexpected failures.
/// </summary>
[SuppressMessage("Naming", "CA1716", Justification =
    "Error is the conventional name for this pattern (Result/Error), well established in .NET " +
    "functional-result libraries; renaming it would only hurt readability.")]
public sealed class Error : IEquatable<Error>
{
    public static readonly Error None = new(string.Empty, string.Empty, ErrorType.None);

    public string Code { get; }
    public string Message { get; }
    public ErrorType Type { get; }

    private Error(string code, string message, ErrorType type)
    {
        Code = code;
        Message = message;
        Type = type;
    }

    public static Error NotFound(string code, string message) => new(code, message, ErrorType.NotFound);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);

    public static Error Unauthorized(string code, string message) => new(code, message, ErrorType.Unauthorized);

    public static Error TooManyRequests(string code, string message) => new(code, message, ErrorType.TooManyRequests);

    public static Error Unexpected(string code, string message) => new(code, message, ErrorType.Unexpected);

    public bool Equals(Error? other) =>
        other is not null && Code == other.Code && Type == other.Type;

    public override bool Equals(object? obj) => Equals(obj as Error);

    public override int GetHashCode() => HashCode.Combine(Code, Type);

    public override string ToString() => string.IsNullOrEmpty(Code) ? "None" : $"{Code}: {Message}";
}

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Conflict,
    Forbidden,
    Unauthorized,
    TooManyRequests,
    Unexpected,
}
