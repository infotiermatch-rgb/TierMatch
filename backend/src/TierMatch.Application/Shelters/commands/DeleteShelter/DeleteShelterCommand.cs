using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Shelters.Commands.DeleteShelter;

public sealed record DeleteShelterCommand(
    Guid Id
) : IRequest<Result>;