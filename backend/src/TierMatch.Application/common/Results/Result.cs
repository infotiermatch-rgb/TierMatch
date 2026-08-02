namespace TierMatch.Application.Common.Results;

public class Result
{
    public bool IsSuccess => Status is
        ResultStatus.Success or
        ResultStatus.Created or
        ResultStatus.NoContent;

    public ResultStatus Status { get; }

    public Error Error { get; }

    protected Result(
        ResultStatus status,
        Error error)
    {
        Status = status;
        Error = error;
    }

    public static Result Success()
        => new(ResultStatus.Success, Error.None);

    public static Result Created()
        => new(ResultStatus.Created, Error.None);

    public static Result NoContent()
        => new(ResultStatus.NoContent, Error.None);

    public static Result NotFound(
        string message)
        => new(
            ResultStatus.NotFound,
            new Error("NotFound", message));

    public static Result Validation(
        string message)
        => new(
            ResultStatus.Validation,
            new Error("Validation", message));

    public static Result Conflict(
        string message)
        => new(
            ResultStatus.Conflict,
            new Error("Conflict", message));

    public static Result Unauthorized()
        => new(
            ResultStatus.Unauthorized,
            new Error("Unauthorized",
                "Unauthorized"));

    public static Result Forbidden()
        => new(
            ResultStatus.Forbidden,
            new Error("Forbidden",
                "Forbidden"));
}