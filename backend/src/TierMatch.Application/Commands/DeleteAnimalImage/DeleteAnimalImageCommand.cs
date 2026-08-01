using MediatR;

namespace TierMatch.Application.Animals.Commands.DeleteAnimalImage;

public sealed record DeleteAnimalImageCommand(
    Guid AnimalId,
    Guid ImageId
) : IRequest<bool>;