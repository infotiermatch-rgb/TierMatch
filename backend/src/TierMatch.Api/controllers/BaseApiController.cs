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

    protected ActionResult HandleResult(Result result)
    {
        if (result.IsSuccess)
            return NoContent();

        return result.Status switch
        {
            ResultStatus.NotFound =>
                NotFound(result.Error),

            ResultStatus.Validation =>
                BadRequest(result.Error),

            ResultStatus.Conflict =>
                Conflict(result.Error),

            ResultStatus.Unauthorized =>
                Unauthorized(),

            ResultStatus.Forbidden =>
                Forbid(),

            _ => Problem(result.Error)
        };
    }

    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.IsSuccess)
            return Ok(result.Value);

        return result.Status switch
        {
            ResultStatus.NotFound =>
                NotFound(result.Error),

            ResultStatus.Validation =>
                BadRequest(result.Error),

            ResultStatus.Conflict =>
                Conflict(result.Error),

            ResultStatus.Unauthorized =>
                Unauthorized(),

            ResultStatus.Forbidden =>
                Forbid(),

            _ => Problem(result.Error)
        };
    }
}