using MediatR;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.DeleteAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Animals.Queries.GetAnimalById;
using TierMatch.Application.Animals.Queries.GetAnimals;

namespace TierMatch.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class AnimalsController : ControllerBase
{
    private readonly IMediator _mediator;

    public AnimalsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Erstellt ein neues Tier.
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<Guid>> Create(
        [FromBody] CreateAnimalCommand command,
        CancellationToken cancellationToken)
    {
        var id = await _mediator.Send(command, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, id);
    }

    /// <summary>
    /// Gibt alle Tiere zurück.
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<List<AnimalDto>>> GetAll(
        CancellationToken cancellationToken)
    {
        var animals = await _mediator.Send(
            new GetAnimalsQuery(),
            cancellationToken);

        return Ok(animals);
    }

    /// <summary>
    /// Gibt ein Tier anhand seiner ID zurück.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AnimalDto>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var animal = await _mediator.Send(
            new GetAnimalByIdQuery(id),
            cancellationToken);

        if (animal is null)
            return NotFound();

        return Ok(animal);
    }

    /// <summary>
    /// Aktualisiert ein vorhandenes Tier.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAnimalCommand command,
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
    /// Löscht ein Tier anhand seiner ID.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var deleted = await _mediator.Send(
            new DeleteAnimalCommand(id),
            cancellationToken);

        if (!deleted)
            return NotFound();

        return NoContent();
    }
}