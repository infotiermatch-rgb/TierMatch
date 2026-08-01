using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Animals.Commands.UpdateAnimalStatus;

public sealed record UpdateAnimalStatusCommand(
    Guid Id,
    AnimalStatus Status
) : IRequest<Result>;