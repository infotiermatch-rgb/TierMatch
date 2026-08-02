using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.DeleteAnimalImage;

public class DeleteAnimalImageHandler
    : IRequestHandler<DeleteAnimalImageCommand, Result>
{
    private readonly IAnimalImageRepository _imageRepository;
    private readonly IFileStorage _fileStorage;
    private readonly IUnitOfWork _unitOfWork;

    public DeleteAnimalImageHandler(
        IAnimalImageRepository imageRepository,
        IFileStorage fileStorage,
        IUnitOfWork unitOfWork)
    {
        _imageRepository = imageRepository;
        _fileStorage = fileStorage;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeleteAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
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

        var wasPrimary = image.IsPrimary;

        _imageRepository.Delete(image);

        await _fileStorage.DeleteAsync(
            image.FilePath,
            cancellationToken);

        if (wasPrimary)
        {
            var remainingImages = await _imageRepository.GetByAnimalIdAsync(
                request.AnimalId,
                cancellationToken);

            var newPrimary = remainingImages
                .Where(i => i.Id != image.Id)
                .OrderBy(i => i.SortOrder)
                .FirstOrDefault();

            if (newPrimary is not null)
            {
                newPrimary.IsPrimary = true;
                _imageRepository.Update(newPrimary);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}