using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;

public sealed class SetPrimaryAnimalImageHandler
    : IRequestHandler<SetPrimaryAnimalImageCommand, Result>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public SetPrimaryAnimalImageHandler(
        IAnimalRepository animalRepository,
        IAnimalImageRepository imageRepository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _animalRepository = animalRepository;
        _imageRepository = imageRepository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        SetPrimaryAnimalImageCommand request,
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
         * Ein Administrator darf Bilder aller Tiere verwalten.
         *
         * Ein ShelterAdmin darf nur Bilder von Tieren seines
         * eigenen Tierheims verwalten.
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

        var images =
            await _imageRepository.GetByAnimalIdAsync(
                request.AnimalId,
                cancellationToken);

        if (images.Count == 0)
        {
            return Result.NotFound(
                "Für dieses Tier wurden keine Bilder gefunden.");
        }

        var selectedImage = images.FirstOrDefault(
            image => image.Id == request.ImageId);

        if (selectedImage is null)
        {
            return Result.NotFound(
                "Bild wurde nicht gefunden.");
        }

        foreach (var image in images)
        {
            var shouldBePrimary =
                image.Id == selectedImage.Id;

            if (image.IsPrimary == shouldBePrimary)
            {
                continue;
            }

            image.IsPrimary = shouldBePrimary;

            _imageRepository.Update(image);
        }

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}