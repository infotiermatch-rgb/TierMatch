using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Commands.DeleteAnimal;

public sealed record DeleteAnimalCommand(Guid Id)
    : IRequest<Result>;