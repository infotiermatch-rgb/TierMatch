using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Commands.DeleteAnimalImage;

public sealed record DeleteAnimalImageCommand(
    Guid AnimalId,
    Guid ImageId
) : IRequest<Result>;