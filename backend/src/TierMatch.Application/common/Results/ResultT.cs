using System.Diagnostics.CodeAnalysis;

namespace TierMatch.Application.Common.Results;

public class Result<T> : Result
{
    public T? Value { get; }

    /// <summary>
    /// Name der Controller-Aktion für CreatedAtAction().
    /// </summary>
    public string? ActionName { get; }

    /// <summary>
    /// Route-Werte für CreatedAtAction().
    /// </summary>
    public object? RouteValues { get; }

    private Result(
        T? value,
        ResultStatus status,
        Error error,
        string? actionName = null,
        object? routeValues = null)
        : base(status, error)
    {
        Value = value;
        ActionName = actionName;
        RouteValues = routeValues;
    }

    public static Result<T> Success(T value)
    {
        return new Result<T>(
            value,
            ResultStatus.Success,
            Error.None);
    }

    public static Result<T> Created(
        T value,
        string actionName,
        object routeValues)
    {
        return new Result<T>(
            value,
            ResultStatus.Created,
            Error.None,
            actionName,
            routeValues);
    }

    public static new Result<T> NotFound(string message)
    {
        return new Result<T>(
            default,
            ResultStatus.NotFound,
            new Error("NotFound", message));
    }

    public static new Result<T> Validation(string message)
    {
        return new Result<T>(
            default,
            ResultStatus.Validation,
            new Error("Validation", message));
    }

    public static new Result<T> Conflict(string message)
    {
        return new Result<T>(
            default,
            ResultStatus.Conflict,
            new Error("Conflict", message));
    }

    public static new Result<T> Unauthorized()
    {
        return new Result<T>(
            default,
            ResultStatus.Unauthorized,
            new Error("Unauthorized", "Unauthorized"));
    }

    public static new Result<T> Forbidden()
    {
        return new Result<T>(
            default,
            ResultStatus.Forbidden,
            new Error("Forbidden", "Forbidden"));
    }
}