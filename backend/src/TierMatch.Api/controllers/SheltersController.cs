using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Animals.Queries.GetAnimalsByShelter;
using TierMatch.Application.Authorization;
using TierMatch.Application.Shelters.Commands.CreateShelter;
using TierMatch.Application.Shelters.Commands.DeleteShelter;
using TierMatch.Application.Shelters.Commands.UpdateShelter;
using TierMatch.Application.Shelters.Queries.GetShelterById;
using TierMatch.Application.Shelters.Queries.GetShelters;

namespace TierMatch.Api.Controllers;

public sealed class SheltersController : BaseApiController
{
    /// <summary>
    /// Erstellt ein neues Tierheim.
    /// Nur Administratoren dürfen neue Tierheime anlegen.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateShelterCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            command,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt alle Tierheime zurück.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetSheltersQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt ein Tierheim anhand seiner ID zurück.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetShelterByIdQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Aktualisiert ein Tierheim.
    /// Administratoren dürfen alle Tierheime bearbeiten.
    /// ShelterAdmins dürfen nur ihr eigenes Tierheim bearbeiten.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateShelterCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new
            {
                error =
                    "Die ID in der URL stimmt nicht mit der ID im Body überein."
            });
        }

        var result = await Mediator.Send(
            command,
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Löscht ein Tierheim.
    /// Das Löschen ist ausschließlich Administratoren erlaubt.
    /// </summary>
    [Authorize(Policy = Policies.Admin)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeleteShelterCommand(id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt alle Tiere eines Tierheims zurück.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}/animals")]
    [ProducesResponseType(
        typeof(List<AnimalDto>),
        StatusCodes.Status200OK)]
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