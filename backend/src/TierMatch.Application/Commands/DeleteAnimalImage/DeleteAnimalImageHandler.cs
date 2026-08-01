using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.DeleteAnimalImage;

public class DeleteAnimalImageHandler
    : IRequestHandler<DeleteAnimalImageCommand, bool>
{
    private readonly IAnimalImageRepository _repository;
    private readonly IFileStorage _fileStorage;

    public DeleteAnimalImageHandler(
        IAnimalImageRepository repository,
        IFileStorage fileStorage)
    {
        _repository = repository;
        _fileStorage = fileStorage;
    }

    public async Task<bool> Handle(
        DeleteAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        var image = await _repository.GetByIdAsync(
            request.ImageId,
            cancellationToken);

        if (image is null)
            return false;

        var wasPrimary = image.IsPrimary;

        _repository.Delete(image);

        await _repository.SaveChangesAsync(cancellationToken);

        await _fileStorage.DeleteAsync(
            image.FilePath,
            cancellationToken);

        if (wasPrimary)
        {
            var remainingImages =
                await _repository.GetAllByAnimalIdAsync(
                    request.AnimalId,
                    cancellationToken);

            var nextPrimary =
                remainingImages
                    .OrderBy(i => i.SortOrder)
                    .FirstOrDefault();

            if (nextPrimary is not null)
            {
                nextPrimary.IsPrimary = true;

                _repository.Update(nextPrimary);

                await _repository.SaveChangesAsync(
                    cancellationToken);
            }
        }

        return true;
    }
}