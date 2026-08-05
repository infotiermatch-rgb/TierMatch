using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Common.Results;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/auth")]
public sealed class AuthenticationController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(
        IIdentityService identityService,
        ILogger<AuthenticationController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<AuthenticationResponse>> RegisterAsync(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.RegisterAsync(
            request,
            cancellationToken);

        return ToAuthenticationActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthenticationResponse>> LoginAsync(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.LoginAsync(
            request,
            cancellationToken);

        return ToAuthenticationActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("forgot-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ForgotPasswordAsync(
        [FromBody] ForgotPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.ForgotPasswordAsync(
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("reset-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> ResetPasswordAsync(
        [FromBody] ResetPasswordRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.ResetPasswordAsync(
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<CurrentUserResponse>> GetCurrentUserAsync(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                "GET /api/v1/auth/me",
                out var userId))
        {
            return Unauthorized();
        }

        var result = await _identityService.GetCurrentUserAsync(
            userId,
            cancellationToken);

        return ToCurrentUserActionResult(result);
    }

    [Authorize]
    [HttpPatch("me")]
    [ProducesResponseType(typeof(CurrentUserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<CurrentUserResponse>> UpdateCurrentUserProfileAsync(
        [FromBody] UpdateCurrentUserProfileRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                "PATCH /api/v1/auth/me",
                out var userId))
        {
            return Unauthorized();
        }

        var result = await _identityService.UpdateCurrentUserProfileAsync(
            userId,
            request,
            cancellationToken);

        return ToCurrentUserActionResult(result);
    }

    [Authorize]
    [HttpPost("change-password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> ChangePasswordAsync(
        [FromBody] ChangePasswordRequest request,
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                "POST /api/v1/auth/change-password",
                out var userId))
        {
            return Unauthorized();
        }

        var result = await _identityService.ChangePasswordAsync(
            userId,
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthenticationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AuthenticationResponse>> RefreshAsync(
        [FromBody] RefreshRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.RefreshAsync(
            request,
            cancellationToken);

        return ToAuthenticationActionResult(result);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> LogoutAsync(
        [FromBody] LogoutRequest request,
        CancellationToken cancellationToken)
    {
        var result = await _identityService.LogoutAsync(
            request,
            cancellationToken);

        return ToActionResult(result);
    }

    [Authorize]
    [HttpPost("logout-all")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> LogoutAllAsync(
        CancellationToken cancellationToken)
    {
        if (!TryGetCurrentUserId(
                "POST /api/v1/auth/logout-all",
                out var userId))
        {
            return Unauthorized();
        }

        var result = await _identityService.LogoutAllAsync(
            userId,
            cancellationToken);

        return ToActionResult(result);
    }

    private bool TryGetCurrentUserId(
        string endpoint,
        out Guid userId)
    {
        var userIdClaim = User.FindFirstValue(
            ClaimTypes.NameIdentifier);

        if (Guid.TryParse(userIdClaim, out userId))
        {
            return true;
        }

        _logger.LogWarning(
            "{Endpoint} wurde ohne gültige Benutzer-ID aufgerufen.",
            endpoint);

        return false;
    }

    private ActionResult<AuthenticationResponse> ToAuthenticationActionResult(
        Result<AuthenticationResponse> result)
    {
        return result.Status switch
        {
            ResultStatus.Success => Ok(result.Value),
            ResultStatus.Created => StatusCode(StatusCodes.Status201Created, result.Value),
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Validation => BadRequest(result.Error),
            ResultStatus.Conflict => Conflict(result.Error),
            ResultStatus.Unauthorized => Unauthorized(result.Error),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error),
            ResultStatus.NotFound => NotFound(result.Error),
            _ => HandleUnexpectedAuthenticationResult(result)
        };
    }

    private ActionResult<CurrentUserResponse> ToCurrentUserActionResult(
        Result<CurrentUserResponse> result)
    {
        return result.Status switch
        {
            ResultStatus.Success => Ok(result.Value),
            ResultStatus.Created => StatusCode(StatusCodes.Status201Created, result.Value),
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Validation => BadRequest(result.Error),
            ResultStatus.Conflict => Conflict(result.Error),
            ResultStatus.Unauthorized => Unauthorized(result.Error),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error),
            ResultStatus.NotFound => NotFound(result.Error),
            _ => HandleUnexpectedCurrentUserResult(result)
        };
    }

    private IActionResult ToActionResult(Result result)
    {
        return result.Status switch
        {
            ResultStatus.Success => Ok(),
            ResultStatus.Created => StatusCode(StatusCodes.Status201Created),
            ResultStatus.NoContent => NoContent(),
            ResultStatus.Validation => BadRequest(result.Error),
            ResultStatus.Conflict => Conflict(result.Error),
            ResultStatus.Unauthorized => Unauthorized(result.Error),
            ResultStatus.Forbidden => StatusCode(StatusCodes.Status403Forbidden, result.Error),
            ResultStatus.NotFound => NotFound(result.Error),
            _ => HandleUnexpectedResult(result)
        };
    }

    private ActionResult<AuthenticationResponse> HandleUnexpectedAuthenticationResult(
        Result<AuthenticationResponse> result)
    {
        _logger.LogError(
            "Der AuthenticationController erhielt bei einer Authentifizierungsanfrage " +
            "den unbekannten Result-Status {ResultStatus}.",
            result.Status);

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new { message = "Bei der Verarbeitung der Anfrage ist ein Fehler aufgetreten." });
    }

    private ActionResult<CurrentUserResponse> HandleUnexpectedCurrentUserResult(
        Result<CurrentUserResponse> result)
    {
        _logger.LogError(
            "Der AuthenticationController erhielt bei der Verarbeitung des " +
            "Benutzerprofils den unbekannten Result-Status {ResultStatus}.",
            result.Status);

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new { message = "Bei der Verarbeitung der Anfrage ist ein Fehler aufgetreten." });
    }

    private IActionResult HandleUnexpectedResult(Result result)
    {
        _logger.LogError(
            "Der AuthenticationController erhielt den unbekannten " +
            "Result-Status {ResultStatus}.",
            result.Status);

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new { message = "Bei der Verarbeitung der Anfrage ist ein Fehler aufgetreten." });
    }
}
