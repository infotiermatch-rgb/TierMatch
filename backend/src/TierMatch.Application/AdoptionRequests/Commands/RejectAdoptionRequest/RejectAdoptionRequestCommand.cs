using MediatR;

using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Commands.RejectAdoptionRequest;

public sealed record RejectAdoptionRequestCommand(
    Guid Id)
    : IRequest<Result>;