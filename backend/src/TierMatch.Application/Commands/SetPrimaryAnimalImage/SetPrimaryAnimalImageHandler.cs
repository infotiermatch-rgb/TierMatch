using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;

public class SetPrimaryAnimalImageHandler
    : IRequestHandler<SetPrimaryAnimalImageCommand, bool>
{
    private readonly IAnimalImageRepository _repository;

    public SetPrimaryAnimalImageHandler(
        IAnimalImageRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        SetPrimaryAnimalImageCommand request,
        CancellationToken cancellationToken)
    {
        var images = await _repository.GetAllByAnimalIdAsync(
            request.AnimalId,
            cancellationToken);

        if (!images.Any())
            return false;

        var selectedImage = images.FirstOrDefault(
            i => i.Id == request.ImageId);

        if (selectedImage is null)
            return false;

        foreach (var image in images)
        {
            image.IsPrimary = image.Id == request.ImageId;

            _repository.Update(image);
        }

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}