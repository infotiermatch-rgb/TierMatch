using MediatR;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.Common.Results;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    private IMediator? _mediator;

    protected IMediator Mediator =>
        _mediator ??= HttpContext.RequestServices.GetRequiredService<IMediator>();

    protected IActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Success => Ok(),

                ResultStatus.Created => StatusCode(StatusCodes.Status201Created),

                ResultStatus.NoContent => NoContent(),

                _ => Ok()
            };
        }

        return HandleFailure(result);
    }

    protected IActionResult HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
        {
            return result.Status switch
            {
                ResultStatus.Success =>
                    Ok(result.Value),

                ResultStatus.Created =>
                    CreatedAtAction(
                        result.ActionName!,
                        result.RouteValues,
                        result.Value),

                ResultStatus.NoContent =>
                    NoContent(),

                _ =>
                    Ok(result.Value)
            };
        }

        return HandleFailure(result);
    }

    private IActionResult HandleFailure(Result result)
    {
        return result.Status switch
        {
            ResultStatus.Validation =>
                BadRequest(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                }),

            ResultStatus.NotFound =>
                NotFound(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                }),

            ResultStatus.Conflict =>
                Conflict(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                }),

            ResultStatus.Unauthorized =>
                Unauthorized(new
                {
                    code = result.Error.Code,
                    message = result.Error.Message
                }),

            ResultStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    new
                    {
                        code = result.Error.Code,
                        message = result.Error.Message
                    }),

            _ =>
                Problem(
                    title: result.Error.Message)
        };
    }
}