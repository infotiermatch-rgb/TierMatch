using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;

public class SetPrimaryAnimalImageHandler
    : IRequestHandler<SetPrimaryAnimalImageCommand, Result>
{
    private readonly IAnimalImageRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public SetPrimaryAnimalImageHandler(
        IAnimalImageRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        SetPrimaryAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        var images = await _repository.GetByAnimalIdAsync(
            request.AnimalId,
            cancellationToken);

        if (!images.Any())
        {
            return Result.NotFound(
                "Für dieses Tier wurden keine Bilder gefunden.");
        }

        var image = images.FirstOrDefault(x => x.Id == request.ImageId);

        if (image is null)
        {
            return Result.NotFound(
                "Bild wurde nicht gefunden.");
        }

        foreach (var item in images)
        {
            item.IsPrimary = false;
            _repository.Update(item);
        }

        image.IsPrimary = true;
        _repository.Update(image);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}