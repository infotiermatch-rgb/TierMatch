using MediatR;

namespace TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;

public sealed record SetPrimaryAnimalImageCommand(
    Guid AnimalId,
    Guid ImageId
) : IRequest<bool>;