using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TierMatch.Api.Contracts.Animals;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.DeleteAnimal;
using TierMatch.Application.Animals.Commands.DeleteAnimalImage;
using TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimalStatus;
using TierMatch.Application.Animals.Commands.UploadAnimalImage;
using TierMatch.Application.Animals.Queries.GetAnimalById;
using TierMatch.Application.Animals.Queries.GetAnimalImages;
using TierMatch.Application.Animals.Queries.GetAnimals;

namespace TierMatch.Api.Controllers;

public class AnimalsController : BaseApiController
{
    /// <summary>
    /// Erstellt ein neues Tier.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnimalCommand command,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Gibt alle Tiere zurück.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new GetAnimalsQuery(),
                cancellationToken));
    }

    /// <summary>
    /// Gibt ein Tier anhand seiner ID zurück.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new GetAnimalByIdQuery(id),
                cancellationToken));
    }

    /// <summary>
    /// Aktualisiert ein Tier.
    /// </summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAnimalCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();

        return HandleResult(
            await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Löscht ein Tier.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new DeleteAnimalCommand(id),
                cancellationToken));
    }

    /// <summary>
    /// Aktualisiert den Status eines Tieres.
    /// </summary>
    [HttpPatch("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAnimalStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
            return BadRequest();

        return HandleResult(
            await Mediator.Send(command, cancellationToken));
    }

    /// <summary>
    /// Lädt ein Bild für ein Tier hoch.
    /// </summary>
    [HttpPost("{animalId:guid}/images")]
    public async Task<IActionResult> UploadImage(
        Guid animalId,
        IFormFile file,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new UploadAnimalImageCommand(
                    animalId,
                    file.OpenReadStream(),
                    file.FileName,
                    file.ContentType,
                    file.Length),
                cancellationToken));
    }

    /// <summary>
    /// Gibt alle Bilder eines Tieres zurück.
    /// </summary>
    [HttpGet("{id:guid}/images")]
    public async Task<IActionResult> GetImages(
        Guid id,
        CancellationToken cancellationToken)
    {
        return HandleResult(
            await Mediator.Send(
                new GetAnimalImagesQuery(id),
                cancellationToken));
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
        return HandleResult(
            await Mediator.Send(
                new SetPrimaryAnimalImageCommand(
                    animalId,
                    imageId),
                cancellationToken));
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
        return HandleResult(
            await Mediator.Send(
                new DeleteAnimalImageCommand(
                    animalId,
                    imageId),
                cancellationToken));
    }
}