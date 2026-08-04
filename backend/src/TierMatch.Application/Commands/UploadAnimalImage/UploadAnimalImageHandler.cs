using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Animals.Commands.UploadAnimalImage;

public sealed class UploadAnimalImageHandler
    : IRequestHandler<UploadAnimalImageCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UploadAnimalImageHandler(
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

    public async Task<Result<Guid>> Handle(
        UploadAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized();
        }

        var isAdmin =
            _currentUserService.IsInRole(Roles.Admin);

        var isShelterAdmin =
            _currentUserService.IsInRole(Roles.ShelterAdmin);

        if (!isAdmin && !isShelterAdmin)
        {
            return Result<Guid>.Forbidden();
        }

        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
        {
            return Result<Guid>.NotFound(
                "Tier wurde nicht gefunden.");
        }

        /*
         * Ein Administrator darf Bilder für jedes Tier hochladen.
         *
         * Ein ShelterAdmin darf nur Bilder für Tiere seines
         * eigenen Tierheims hochladen.
         */
        if (isShelterAdmin && !isAdmin)
        {
            var currentShelterId =
                _currentUserService.ShelterId;

            if (!currentShelterId.HasValue)
            {
                return Result<Guid>.Forbidden();
            }

            if (animal.ShelterId != currentShelterId.Value)
            {
                return Result<Guid>.Forbidden();
            }
        }

        var storedFile =
            await _fileStorage.SaveAnimalImageAsync(
                request.AnimalId,
                request.Stream,
                request.FileName,
                cancellationToken);

        var image = new AnimalImage
        {
            AnimalId = request.AnimalId,
            FileName = storedFile.FileName,
            FilePath = storedFile.FilePath,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            IsPrimary = !animal.Images.Any(),
            SortOrder = animal.Images.Count
        };

        await _imageRepository.AddAsync(
            image,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(image.Id);
    }
}