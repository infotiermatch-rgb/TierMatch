using System.Security.Claims;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Authentication.DTOs;
using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users")]
[Authorize(Policy = Policies.Admin)]
public sealed class AdminUsersController : ControllerBase
{
    private readonly IIdentityService _identityService;
    private readonly ILogger<AdminUsersController> _logger;

    public AdminUsersController(
        IIdentityService identityService,
        ILogger<AdminUsersController> logger)
    {
        _identityService = identityService;
        _logger = logger;
    }

    /// <summary>
    /// Gibt eine durchsuchbare und paginierte Benutzerliste zurück.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(
        typeof(AdminUserListResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<AdminUserListResponse>>
        GetUsersAsync(
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
    {
        var result =
            await _identityService.GetAdminUsersAsync(
                search,
                page,
                pageSize,
                cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Gibt einen Benutzer anhand seiner ID zurück.
    /// </summary>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(
        typeof(AdminUserDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AdminUserDto>>
        GetUserByIdAsync(
            Guid userId,
            CancellationToken cancellationToken)
    {
        var result =
            await _identityService.GetAdminUserByIdAsync(
                userId,
                cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Weist einem Benutzer die Rolle ShelterAdmin
    /// und ein Tierheim zu.
    /// </summary>
    [HttpPut("{userId:guid}/shelter-access")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignShelterAccessAsync(
        Guid userId,
        [FromBody] AssignShelterAdminRequest request,
        CancellationToken cancellationToken)
    {
        var result =
            await _identityService.AssignShelterAdminAsync(
                userId,
                request.ShelterId,
                cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Entzieht einem Benutzer den Tierheimzugriff.
    /// </summary>
    [HttpDelete("{userId:guid}/shelter-access")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> RemoveShelterAccessAsync(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var result =
            await _identityService.RemoveShelterAccessAsync(
                userId,
                cancellationToken);

        return ToActionResult(result);
    }

    /// <summary>
    /// Aktiviert oder deaktiviert einen Benutzer.
    /// </summary>
    [HttpPatch("{userId:guid}/active-status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> SetActiveStatusAsync(
        Guid userId,
        [FromBody] SetUserActiveStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (!request.IsActive)
        {
            var currentUserIdClaim =
                User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

            if (Guid.TryParse(
                    currentUserIdClaim,
                    out var currentUserId) &&
                currentUserId == userId)
            {
                return Conflict(new
                {
                    message =
                        "Ein Administrator kann sein eigenes Benutzerkonto nicht deaktivieren."
                });
            }
        }

        var result =
            await _identityService.SetUserActiveStatusAsync(
                userId,
                request.IsActive,
                cancellationToken);

        return ToActionResult(result);
    }

    private ActionResult<T> ToActionResult<T>(
        Result<T> result)
    {
        return result.Status switch
        {
            ResultStatus.Success =>
                Ok(result.Value),

            ResultStatus.Created =>
                StatusCode(
                    StatusCodes.Status201Created,
                    result.Value),

            ResultStatus.NoContent =>
                NoContent(),

            ResultStatus.Validation =>
                BadRequest(result.Error),

            ResultStatus.Conflict =>
                Conflict(result.Error),

            ResultStatus.Unauthorized =>
                Unauthorized(result.Error),

            ResultStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            ResultStatus.NotFound =>
                NotFound(result.Error),

            _ => HandleUnexpectedResult(result)
        };
    }

    private IActionResult ToActionResult(
        Result result)
    {
        return result.Status switch
        {
            ResultStatus.Success =>
                NoContent(),

            ResultStatus.NoContent =>
                NoContent(),

            ResultStatus.Validation =>
                BadRequest(result.Error),

            ResultStatus.Conflict =>
                Conflict(result.Error),

            ResultStatus.Unauthorized =>
                Unauthorized(result.Error),

            ResultStatus.Forbidden =>
                StatusCode(
                    StatusCodes.Status403Forbidden,
                    result.Error),

            ResultStatus.NotFound =>
                NotFound(result.Error),

            _ => HandleUnexpectedResult(result)
        };
    }

    private ActionResult<T> HandleUnexpectedResult<T>(
        Result<T> result)
    {
        _logger.LogError(
            "Der AdminUsersController erhielt den unbekannten " +
            "Result-Status {ResultStatus}.",
            result.Status);

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new
            {
                message =
                    "Bei der Verarbeitung der Anfrage ist ein Fehler aufgetreten."
            });
    }

    private IActionResult HandleUnexpectedResult(
        Result result)
    {
        _logger.LogError(
            "Der AdminUsersController erhielt den unbekannten " +
            "Result-Status {ResultStatus}.",
            result.Status);

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            new
            {
                message =
                    "Bei der Verarbeitung der Anfrage ist ein Fehler aufgetreten."
            });
    }
}