using System.Diagnostics.CodeAnalysis;

namespace ColombiaGo.Domain.Common;

/// <summary>
/// Outcome of an operation that can fail for an expected, handleable reason. Application
/// services return <see cref="Result"/>/<see cref="Result{T}"/> instead of throwing for
/// expected failures (not found, validation, conflict) — the API layer maps <see cref="Error.Type"/>
/// to the matching HTTP status. Exceptions remain reserved for genuinely unexpected failures.
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public Error Error { get; }

    protected Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None)
            throw new InvalidOperationException("A successful result cannot carry an error.");
        if (!isSuccess && error == Error.None)
            throw new InvalidOperationException("A failed result must carry an error.");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    [SuppressMessage("Design", "CA1000", Justification =
        "These generic factory helpers exist purely so callers can write `Result.Success(value)` " +
        "without naming the generic Result<T> type explicitly, mirroring the non-generic factories " +
        "right above them; that convenience is the point.")]
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);

    [SuppressMessage("Design", "CA1000", Justification =
        "See Success<T> above — same rationale.")]
    public static Result<T> Failure<T>(Error error) => Result<T>.Failure(error);
}

/// <summary>
/// A <see cref="Result"/> that carries a value on success.
/// </summary>
public class Result<T> : Result
{
    private readonly T? _value;

    public T Value => IsSuccess
        ? _value!
        : throw new InvalidOperationException("Cannot access the value of a failed result.");

    private Result(bool isSuccess, T? value, Error error) : base(isSuccess, error)
    {
        _value = value;
    }

    [SuppressMessage("Design", "CA1000", Justification =
        "Static factory methods are the intended construction path for this type (constructors " +
        "are private) — this is the standard Result<T> factory pattern, not an oversight.")]
    public static Result<T> Success(T value) => new(true, value, Error.None);

    [SuppressMessage("Design", "CA1000", Justification = "See Success above — same rationale.")]
    public static new Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);
}
