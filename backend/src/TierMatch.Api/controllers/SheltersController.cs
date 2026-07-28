using MediatR;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.Shelters.Commands.CreateShelter;
using TierMatch.Application.Shelters.Commands.DeleteShelter;
using TierMatch.Application.Shelters.Commands.UpdateShelter;
using TierMatch.Application.Shelters.Models;
using TierMatch.Application.Shelters.Queries.GetShelterById;
using TierMatch.Application.Shelters.Queries.GetShelters;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Animals.Queries.GetAnimalsByShelter;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class SheltersController : ControllerBase
{
    private readonly IMediator _mediator;

    public SheltersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Erstellt ein neues Tierheim.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateShelterCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Gibt alle Tierheime zurück.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<ShelterDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var shelters = await _mediator.Send(
            new GetSheltersQuery(),
            cancellationToken);

        return Ok(shelters);
    }

    /// <summary>
    /// Gibt ein Tierheim anhand seiner ID zurück.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ShelterDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var shelter = await _mediator.Send(
            new GetShelterByIdQuery { Id = id },
            cancellationToken);

        if (shelter is null)
            return NotFound();

        return Ok(shelter);
    }

    /// <summary>
    /// Aktualisiert ein vorhandenes Tierheim.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateShelterCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(
                "Die ID in der URL stimmt nicht mit der ID im Body überein.");
        }

        var updated = await _mediator.Send(command, cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    /// <summary>
    /// Löscht ein Tierheim anhand seiner ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(
            new DeleteShelterCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }

    [HttpGet("{id:guid}/animals")]
public async Task<ActionResult<List<AnimalDto>>> GetAnimals(
    Guid id,
    CancellationToken cancellationToken)
{
    var animals = await _mediator.Send(
        new GetAnimalsByShelterQuery(id),
        cancellationToken);

    return Ok(animals);
}
}