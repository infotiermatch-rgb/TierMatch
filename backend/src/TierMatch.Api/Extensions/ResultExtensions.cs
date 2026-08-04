using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Common.Results;

namespace TierMatch.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(
        this ControllerBase controller,
        Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Success =>
                controller.Ok(result.Value),

            ResultStatus.Created =>
                controller.CreatedAtAction(
                    result.ActionName!,
                    result.RouteValues,
                    result.Value),

            ResultStatus.NoContent =>
                controller.NoContent(),

            ResultStatus.Validation =>
                controller.BadRequest(result.Error),

            ResultStatus.NotFound =>
                controller.NotFound(result.Error),

            ResultStatus.Conflict =>
                controller.Conflict(result.Error),

            ResultStatus.Unauthorized =>
                controller.Unauthorized(result.Error),

            ResultStatus.Forbidden =>
                controller.Forbid(),

            _ =>
                controller.StatusCode(
                    500,
                    result.Error)
        };
    }

    public static IActionResult ToActionResult(
        this ControllerBase controller,
        Result result)
    {
        return result.Status switch
        {
            ResultStatus.Success =>
                controller.Ok(),

            ResultStatus.Created =>
                controller.StatusCode(201),

            ResultStatus.NoContent =>
                controller.NoContent(),

            ResultStatus.Validation =>
                controller.BadRequest(result.Error),

            ResultStatus.NotFound =>
                controller.NotFound(result.Error),

            ResultStatus.Conflict =>
                controller.Conflict(result.Error),

            ResultStatus.Unauthorized =>
                controller.Unauthorized(result.Error),

            ResultStatus.Forbidden =>
                controller.Forbid(),

            _ =>
                controller.StatusCode(
                    500,
                    result.Error)
        };
    }
}