using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Commands.SetPrimaryAnimalImage;

public sealed record SetPrimaryAnimalImageCommand(
    Guid AnimalId,
    Guid ImageId
) : IRequest<Result>;