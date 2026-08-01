namespace TierMatch.Application.Common.Results;

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(
        T? value,
        bool success,
        string? error,
        ResultStatus status)
        : base(success, error, status)
    {
        Value = value;
    }

    public static Result<T> Success(T value)
        => new(value, true, null, ResultStatus.Success);

    public static new Result<T> NotFound(string message)
        => new(default, false, message, ResultStatus.NotFound);

    public static new Result<T> Validation(string message)
        => new(default, false, message, ResultStatus.Validation);

    public static new Result<T> Conflict(string message)
        => new(default, false, message, ResultStatus.Conflict);

    public static new Result<T> Unauthorized(string message = "Unauthorized.")
        => new(default, false, message, ResultStatus.Unauthorized);

    public static new Result<T> Forbidden(string message = "Forbidden.")
        => new(default, false, message, ResultStatus.Forbidden);
}