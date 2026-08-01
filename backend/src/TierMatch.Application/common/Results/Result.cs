namespace TierMatch.Application.Common.Results;

public class Result
{
    public bool IsSuccess { get; }

    public string? Error { get; }

    public ResultStatus Status { get; }

    protected Result(
        bool success,
        string? error,
        ResultStatus status)
    {
        IsSuccess = success;
        Error = error;
        Status = status;
    }

    public static Result Success()
        => new(true, null, ResultStatus.Success);

    public static Result NotFound(string message)
        => new(false, message, ResultStatus.NotFound);

    public static Result Validation(string message)
        => new(false, message, ResultStatus.Validation);

    public static Result Conflict(string message)
        => new(false, message, ResultStatus.Conflict);

    public static Result Unauthorized(string message = "Unauthorized.")
        => new(false, message, ResultStatus.Unauthorized);

    public static Result Forbidden(string message = "Forbidden.")
        => new(false, message, ResultStatus.Forbidden);
}