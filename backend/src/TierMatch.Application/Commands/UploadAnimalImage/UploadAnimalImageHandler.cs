using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Animals.Commands.UploadAnimalImage;

public class UploadAnimalImageHandler
    : IRequestHandler<UploadAnimalImageCommand, Guid>
{
    private readonly IAnimalRepository _animalRepository;
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;

    public UploadAnimalImageHandler(
        IAnimalRepository animalRepository,
        IAnimalImageRepository imageRepository,
        IFileStorage fileStorage)
    {
        _animalRepository = animalRepository;
        _imageRepository = imageRepository;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(
        UploadAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
            throw new KeyNotFoundException("Animal not found.");

        var (fileName, filePath) =
            await _fileStorage.SaveAnimalImageAsync(
                request.AnimalId,
                request.Stream,
                request.FileName,
                cancellationToken);

        var sortOrder =
            await _imageRepository.GetNextSortOrderAsync(
                request.AnimalId,
                cancellationToken);

        var isPrimary =
            await _imageRepository.GetPrimaryAsync(
                request.AnimalId,
                cancellationToken) is null;

        var image = new AnimalImage
        {
            AnimalId = request.AnimalId,
            FileName = fileName,
            FilePath = filePath,
            ContentType = request.ContentType,
            FileSize = request.FileSize,
            SortOrder = sortOrder,
            IsPrimary = isPrimary
        };

        await _imageRepository.AddAsync(
            image,
            cancellationToken);

        await _imageRepository.SaveChangesAsync(
            cancellationToken);

        return image.Id;
    }
}