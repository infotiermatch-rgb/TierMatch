using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Animals.Commands.UploadAnimalImage;

public class UploadAnimalImageHandler
    : IRequestHandler<UploadAnimalImageCommand, Result<Guid>>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public UploadAnimalImageHandler(
        IAnimalRepository animalRepository,
        IAnimalImageRepository imageRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _animalRepository = animalRepository;
        _imageRepository = imageRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
        UploadAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
        {
            return Result<Guid>.NotFound(
                "Tier wurde nicht gefunden.");
        }

        var storedFile = await _fileStorage.SaveAnimalImageAsync(
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

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<Guid>.Success(image.Id);
    }
}