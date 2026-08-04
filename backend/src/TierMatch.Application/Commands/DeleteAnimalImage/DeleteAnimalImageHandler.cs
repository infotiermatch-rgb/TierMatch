using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Animals.Commands.DeleteAnimalImage;

public sealed class DeleteAnimalImageHandler
    : IRequestHandler<DeleteAnimalImageCommand, Result>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public DeleteAnimalImageHandler(
        IAnimalRepository animalRepository,
        IAnimalImageRepository imageRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _animalRepository = animalRepository;
        _imageRepository = imageRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        DeleteAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Unauthorized();
        }

        var isAdmin =
            _currentUserService.IsInRole(Roles.Admin);

        var isShelterAdmin =
            _currentUserService.IsInRole(Roles.ShelterAdmin);

        if (!isAdmin && !isShelterAdmin)
        {
            return Result.Forbidden();
        }

        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
        {
            return Result.NotFound(
                "Tier wurde nicht gefunden.");
        }

        /*
         * Ein Administrator darf Bilder aller Tiere löschen.
         *
         * Ein ShelterAdmin darf nur Bilder von Tieren seines
         * eigenen Tierheims löschen.
         */
        if (isShelterAdmin && !isAdmin)
        {
            var currentShelterId =
                _currentUserService.ShelterId;

            if (!currentShelterId.HasValue)
            {
                return Result.Forbidden();
            }

            if (animal.ShelterId != currentShelterId.Value)
            {
                return Result.Forbidden();
            }
        }

        var image = await _imageRepository.GetByIdAsync(
            request.ImageId,
            cancellationToken);

        if (image is null)
        {
            return Result.NotFound(
                "Bild wurde nicht gefunden.");
        }

        if (image.AnimalId != request.AnimalId)
        {
            return Result.Validation(
                "Das Bild gehört nicht zu diesem Tier.");
        }

        AnimalImage? newPrimaryImage = null;

        /*
         * Wird das aktuelle Hauptbild gelöscht, bestimmen wir
         * vor dem Löschen das nächste Bild anhand der Sortierung.
         */
        if (image.IsPrimary)
        {
            var animalImages =
                await _imageRepository.GetByAnimalIdAsync(
                    request.AnimalId,
                    cancellationToken);

            newPrimaryImage = animalImages
                .Where(existingImage =>
                    existingImage.Id != image.Id)
                .OrderBy(existingImage =>
                    existingImage.SortOrder)
                .FirstOrDefault();
        }

        _imageRepository.Delete(image);

        if (newPrimaryImage is not null)
        {
            newPrimaryImage.IsPrimary = true;

            _imageRepository.Update(
                newPrimaryImage);
        }

        /*
         * Zuerst wird die Datenbank aktualisiert.
         * Dadurch verweist die API bei einem Fehler beim Löschen
         * der Datei nicht mehr auf eine nicht vorhandene Datei.
         */
        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        await _fileStorage.DeleteAsync(
            image.FilePath,
            cancellationToken);

        return Result.Success();
    }
}