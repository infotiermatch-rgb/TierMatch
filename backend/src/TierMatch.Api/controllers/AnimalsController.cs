using MediatR;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.DeleteAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Animals.Queries.GetAnimalById;
using TierMatch.Application.Animals.Queries.GetAnimals;
using TierMatch.Api.Contracts.Animals;
using TierMatch.Application.Animals.Commands.UpdateAnimalStatus;
using Microsoft.AspNetCore.Http;
using TierMatch.Application.Animals.Commands.UploadAnimalImage;
using TierMatch.Application.Animals.Queries.GetAnimalImages;
using TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;
using TierMatch.Application.Animals.Commands.DeleteAnimalImage;

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

    [HttpPatch("{id:guid}/status")]
public async Task<IActionResult> UpdateStatus(
    Guid id,
    [FromBody] UpdateAnimalStatusRequest request,
    CancellationToken cancellationToken)
{
    var updated = await _mediator.Send(
        new UpdateAnimalStatusCommand(
            id,
            request.Status),
        cancellationToken);

    if (!updated)
        return NotFound();

    return NoContent();
}
/// <summary>
/// Lädt ein Bild für ein Tier hoch.
/// </summary>
[HttpPost("{id:guid}/images")]
[Consumes("multipart/form-data")]
public async Task<ActionResult<Guid>> UploadImage(
    Guid id,
    IFormFile file,
    CancellationToken cancellationToken)
{
    if (file is null || file.Length == 0)
    {
        return BadRequest("Es wurde keine Datei hochgeladen.");
    }

    await using var stream = file.OpenReadStream();

    var imageId = await _mediator.Send(
        new UploadAnimalImageCommand(
            id,
            stream,
            file.FileName,
            file.ContentType,
            file.Length),
        cancellationToken);

    return Ok(imageId);
}

/// <summary>
/// Gibt alle Bilder eines Tieres zurück.
/// </summary>
[HttpGet("{id:guid}/images")]
public async Task<ActionResult<List<AnimalImageDto>>> GetImages(
    Guid id,
    CancellationToken cancellationToken)
{
    var images = await _mediator.Send(
        new GetAnimalImagesQuery(id),
        cancellationToken);

    return Ok(images);
}
/// <summary>
/// Legt das Hauptbild eines Tieres fest.
/// </summary>
[HttpPatch("{animalId:guid}/images/{imageId:guid}/primary")]
public async Task<IActionResult> SetPrimaryImage(
    Guid animalId,
    Guid imageId,
    CancellationToken cancellationToken)
{
    var success = await _mediator.Send(
        new SetPrimaryAnimalImageCommand(
            animalId,
            imageId),
        cancellationToken);

    if (!success)
        return NotFound();

    return NoContent();
}
/// <summary>
/// Löscht ein Bild eines Tieres.
/// </summary>
[HttpDelete("{animalId:guid}/images/{imageId:guid}")]
public async Task<IActionResult> DeleteImage(
    Guid animalId,
    Guid imageId,
    CancellationToken cancellationToken)
{
    var deleted = await _mediator.Send(
        new DeleteAnimalImageCommand(
            animalId,
            imageId),
        cancellationToken);

    if (!deleted)
        return NotFound();

    return NoContent();
}
}