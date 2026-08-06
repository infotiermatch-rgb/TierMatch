using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Authorization;
using TierMatch.Application.ShelterRegistrations.Commands.ApproveShelterRegistration;
using TierMatch.Application.ShelterRegistrations.Commands.CreateShelterRegistration;
using TierMatch.Application.ShelterRegistrations.Commands.RejectShelterRegistration;
using TierMatch.Application.ShelterRegistrations.DTOs;
using TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrationById;
using TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrations;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Controllers;

public sealed class ShelterRegistrationsController
    : BaseApiController
{
    /// <summary>
    /// Reicht eine neue Tierheimregistrierung ein.
    /// </summary>
    [AllowAnonymous]
    [HttpPost("~/api/v1/shelter-registrations")]
    [ProducesResponseType(
        typeof(Guid),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create(
        [FromBody]
        CreateShelterRegistrationCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            command,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt Tierheimregistrierungen für Administratoren zurück.
    /// Optional kann nach dem Status gefiltert werden.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpGet("~/api/v1/shelter-registrations")]
    [ProducesResponseType(
        typeof(List<ShelterRegistrationListItemDto>),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> GetAll(
        [FromQuery]
        ShelterRegistrationStatus? status,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetShelterRegistrationsQuery(
                status),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt die vollständigen Details einer
    /// Tierheimregistrierung zurück.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpGet(
        "~/api/v1/shelter-registrations/{id:guid}")]
    [ProducesResponseType(
        typeof(ShelterRegistrationDetailsDto),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetShelterRegistrationByIdQuery(
                id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Genehmigt eine offene Tierheimregistrierung.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpPatch(
        "~/api/v1/shelter-registrations/{id:guid}/approve")]
    [ProducesResponseType(
        typeof(ApproveShelterRegistrationResponse),
        StatusCodes.Status200OK)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    [ProducesResponseType(
        StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Approve(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new ApproveShelterRegistrationCommand(
                id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Lehnt eine offene Tierheimregistrierung
    /// mit einer Begründung ab.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpPatch(
        "~/api/v1/shelter-registrations/{id:guid}/reject")]
    [ProducesResponseType(
        StatusCodes.Status204NoContent)]
    [ProducesResponseType(
        StatusCodes.Status400BadRequest)]
    [ProducesResponseType(
        StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(
        StatusCodes.Status403Forbidden)]
    [ProducesResponseType(
        StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Reject(
        Guid id,
        [FromBody]
        RejectShelterRegistrationRequest request,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new RejectShelterRegistrationCommand(
                id,
                request.Reason),
            cancellationToken);

        return HandleResult(result);
    }
}