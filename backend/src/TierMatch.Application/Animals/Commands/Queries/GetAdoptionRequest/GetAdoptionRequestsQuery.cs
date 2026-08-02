using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;

public sealed record GetAdoptionRequestsQuery
    : IRequest<Result<List<AdoptionRequestDto>>>;