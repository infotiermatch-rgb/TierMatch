using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;

public sealed record GetAdoptionRequestByIdQuery(
    Guid Id
) : IRequest<Result<AdoptionRequestDto>>;