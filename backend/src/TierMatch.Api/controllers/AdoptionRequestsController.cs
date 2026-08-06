using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.AdoptionRequests.Commands.ApproveAdoptionRequest;
using TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;
using TierMatch.Application.AdoptionRequests.Commands.RejectAdoptionRequest;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;
using TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;
using TierMatch.Application.AdoptionRequests.Queries.GetMyAdoptionRequests;
using TierMatch.Application.Authorization;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/adoption-requests")]
public sealed class AdoptionRequestsController
    : BaseApiController
{
    /// <summary>
    /// Erstellt eine neue Adoptionsanfrage.
    /// </summary>
    [Authorize]
    [HttpPost]
    [ProducesResponseType(
        typeof(Guid),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAdoptionRequestDto dto,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new CreateAdoptionRequestCommand(
                dto.AnimalId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.PhoneNumber,
                dto.Message),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt die Adoptionsanfragen des angemeldeten
    /// Benutzers zurück.
    /// </summary>
    [Authorize]
    [HttpGet("me")]
    [ProducesResponseType(
        typeof(List<AdoptionRequestDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetMine(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetMyAdoptionRequestsQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt die verwaltbaren Adoptionsanfragen zurück.
    ///
    /// Globale Administratoren sehen alle Anfragen.
    /// Tierheimadministratoren sehen nur Anfragen
    /// zu Tieren ihres eigenen Tierheims.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpGet]
    [ProducesResponseType(
        typeof(List<AdoptionRequestDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAdoptionRequestsQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt eine verwaltbare Adoptionsanfrage
    /// anhand ihrer ID zurück.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(
        typeof(AdoptionRequestDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAdoptionRequestByIdQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Genehmigt eine offene Adoptionsanfrage.
    ///
    /// Das Tier wird reserviert und alle anderen
    /// offenen Anfragen für dieses Tier werden abgelehnt.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPatch("{id:guid}/approve")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ApproveAdoptionRequestCommand(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Lehnt eine offene Adoptionsanfrage ab.
    /// Der Status des zugehörigen Tieres bleibt unverändert.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPatch("{id:guid}/reject")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RejectAdoptionRequestCommand(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }
}