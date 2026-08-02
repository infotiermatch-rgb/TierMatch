using FluentValidation;
using TierMatch.Api.Models;

namespace TierMatch.Api.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed.");

            await WriteValidationErrorAsync(
                context,
                ex.Errors
                    .Select(e => e.ErrorMessage)
                    .Distinct()
                    .ToList());
        }
        catch (KeyNotFoundException ex)
        {
            _logger.LogWarning(ex, "Resource not found.");

            await WriteErrorAsync(
                context,
                StatusCodes.Status404NotFound,
                "Not Found",
                ex.Message);
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogWarning(ex, "Unauthorized.");

            await WriteErrorAsync(
                context,
                StatusCodes.Status401Unauthorized,
                "Unauthorized",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception.");

            await WriteErrorAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal Server Error",
                "Ein unerwarteter Fehler ist aufgetreten.");
        }
    }

    private static async Task WriteValidationErrorAsync(
        HttpContext context,
        IReadOnlyList<string> errors)
    {
        context.Response.StatusCode =
            StatusCodes.Status400BadRequest;

        context.Response.ContentType =
            "application/json";

        await context.Response.WriteAsJsonAsync(
            new ApiError
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "Eine oder mehrere Validierungen sind fehlgeschlagen.",
                Errors = errors,
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier
            });
    }

    private static async Task WriteErrorAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsJsonAsync(
            new ApiError
            {
                Status = statusCode,
                Title = title,
                Detail = detail,
                Timestamp = DateTime.UtcNow,
                TraceId = context.TraceIdentifier
            });
    }
}