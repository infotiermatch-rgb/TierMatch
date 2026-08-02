using MediatR;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;
using TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;
using TierMatch.Application.AdoptionRequests.Commands.ApproveAdoptionRequest;


namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AdoptionRequestsController : BaseApiController
{

/// <summary>
/// Erstellt eine neue Adoptionsanfrage.
/// </summary>
[HttpPost]
public async Task<IActionResult> Create(
    [FromBody] CreateAdoptionRequestDto dto,
    CancellationToken cancellationToken)
{
    return HandleResult(
        await Mediator.Send(
            new CreateAdoptionRequestCommand(
                dto.AnimalId,
                dto.FirstName,
                dto.LastName,
                dto.Email,
                dto.PhoneNumber,
                dto.Message),
            cancellationToken));
}

    /// <summary>
/// Gibt alle Adoptionsanfragen zurück.
/// </summary>
[HttpGet]
public async Task<ActionResult<List<AdoptionRequestDto>>> GetAll(
    CancellationToken cancellationToken)
{
    var requests = await Mediator.Send(
        new GetAdoptionRequestsQuery(),
        cancellationToken);

    return Ok(requests);
}

/// <summary>
/// Gibt eine Adoptionsanfrage anhand ihrer ID zurück.
/// </summary>
[HttpGet("{id:guid}")]
public async Task<ActionResult<AdoptionRequestDto>> GetById(
    Guid id,
    CancellationToken cancellationToken)
{
    var request = await Mediator.Send(
        new GetAdoptionRequestByIdQuery(id),
        cancellationToken);

    if (request is null)
        return NotFound();

    return Ok(request);
}
/// <summary>
/// Genehmigt eine Adoptionsanfrage.
/// </summary>
[HttpPatch("{id:guid}/approve")]
public async Task<IActionResult> Approve(
    Guid id,
    CancellationToken cancellationToken)
{
 return HandleResult(
    await Mediator.Send(
        new ApproveAdoptionRequestCommand(id),
        cancellationToken));
}
}