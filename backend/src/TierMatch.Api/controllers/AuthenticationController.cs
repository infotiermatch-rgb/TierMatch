using MediatR;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Authentication.Commands.Login;
using TierMatch.Application.Authentication.Commands.Register;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthenticationController(
        IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.Status switch
        {
            TierMatch.Application.Common.Results.ResultStatus.Success
                => Ok(result.Value),

            TierMatch.Application.Common.Results.ResultStatus.Validation
                => BadRequest(result.Error),

            TierMatch.Application.Common.Results.ResultStatus.Conflict
                => Conflict(result.Error),

            _ => StatusCode(500, result.Error)
        };
    }

    [HttpPost("login")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            command,
            cancellationToken);

        return result.Status switch
        {
            TierMatch.Application.Common.Results.ResultStatus.Success
                => Ok(result.Value),

            TierMatch.Application.Common.Results.ResultStatus.Unauthorized
                => Unauthorized(result.Error),

            TierMatch.Application.Common.Results.ResultStatus.Forbidden
                => StatusCode(StatusCodes.Status403Forbidden, result.Error),

            _ => StatusCode(500, result.Error)
        };
    }
}