using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Animals.Queries.GetAnimalsByShelter;
using TierMatch.Application.Shelters.Commands.CreateShelter;
using TierMatch.Application.Shelters.Commands.DeleteShelter;
using TierMatch.Application.Shelters.Commands.UpdateShelter;
using TierMatch.Application.Shelters.Models;
using TierMatch.Application.Shelters.Queries.GetShelterById;
using TierMatch.Application.Shelters.Queries.GetShelters;

namespace TierMatch.Api.Controllers;

public class SheltersController : BaseApiController
{
    /// <summary>
    /// Erstellt ein neues Tierheim.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateShelterCommand command,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                command,
                cancellationToken));
    }

    /// <summary>
    /// Gibt alle Tierheime zurück.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new GetSheltersQuery(),
                cancellationToken));
    }

    /// <summary>
    /// Gibt ein Tierheim anhand seiner ID zurück.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new GetShelterByIdQuery(id),
                cancellationToken));
    }

    /// <summary>
    /// Aktualisiert ein Tierheim.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateShelterCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest(
                "Die ID in der URL stimmt nicht mit der ID im Body überein.");

        return HandleResult(
            await Mediator.Send(
                command,
                cancellationToken));
    }

    /// <summary>
    /// Löscht ein Tierheim.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new DeleteShelterCommand(id),
                cancellationToken));
    }

    /// <summary>
    /// Gibt alle Tiere eines Tierheims zurück.
    /// </summary>
    [HttpGet("{id:guid}/animals")]
    public async Task<ActionResult<List<AnimalDto>>> GetAnimals(
        Guid id,
        CancellationToken cancellationToken)
    {
        var animals = await Mediator.Send(
            new GetAnimalsByShelterQuery(id),
            cancellationToken);

        return Ok(animals);
    }
}