using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

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
using TierMatch.Application.Authorization;

namespace TierMatch.Api.Controllers;

public sealed class AnimalsController : BaseApiController
{
    /// <summary>
    /// Erstellt ein neues Tier.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAnimalCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            command,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Value },
            result.Value);
    }

    /// <summary>
    /// Gibt alle Tiere zurück.
    /// </summary>
    [AllowAnonymous]
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAnimalsQuery(),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt ein Tier anhand seiner ID zurück.
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
            new GetAnimalByIdQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Aktualisiert ein Tier.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateAnimalCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new
            {
                error = "Die ID in der URL stimmt nicht mit der ID im Request überein."
            });
        }

        var result = await Mediator.Send(
            command,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Löscht ein Tier.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeleteAnimalCommand(id),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Aktualisiert den Status eines Tieres.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPatch("{id:guid}/status")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateAnimalStatusCommand command,
        CancellationToken cancellationToken)
    {
        if (id != command.Id)
        {
            return BadRequest(new
            {
                error = "Die ID in der URL stimmt nicht mit der ID im Request überein."
            });
        }

        var result = await Mediator.Send(
            command,
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Lädt ein Bild für ein Tier hoch.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPost("{animalId:guid}/images")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadImage(
        Guid animalId,
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest(new
            {
                error = "Es wurde keine gültige Datei hochgeladen."
            });
        }

        await using var stream = file.OpenReadStream();

        var result = await Mediator.Send(
            new UploadAnimalImageCommand(
                animalId,
                stream,
                file.FileName,
                file.ContentType,
                file.Length),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Gibt alle Bilder eines Tieres zurück.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("{id:guid}/images")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetImages(
        Guid id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetAnimalImagesQuery(id),
            cancellationToken);

        return HandleResult(result);
    }

    /// <summary>
    /// Legt das Hauptbild eines Tieres fest.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpPatch("{animalId:guid}/images/{imageId:guid}/primary")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetPrimaryImage(
        Guid animalId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new SetPrimaryAnimalImageCommand(
                animalId,
                imageId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }

    /// <summary>
    /// Löscht ein Bild eines Tieres.
    /// </summary>
    [Authorize(Policy = Policies.CanManageShelter)]
    [HttpDelete("{animalId:guid}/images/{imageId:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteImage(
        Guid animalId,
        Guid imageId,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new DeleteAnimalImageCommand(
                animalId,
                imageId),
            cancellationToken);

        if (!result.IsSuccess)
        {
            return HandleResult(result);
        }

        return NoContent();
    }
}