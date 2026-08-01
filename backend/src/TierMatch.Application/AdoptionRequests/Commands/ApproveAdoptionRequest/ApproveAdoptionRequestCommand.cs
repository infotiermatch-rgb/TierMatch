using MediatR;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Commands.ApproveAdoptionRequest;

public sealed record ApproveAdoptionRequestCommand(
    Guid Id
) : IRequest<Result>;