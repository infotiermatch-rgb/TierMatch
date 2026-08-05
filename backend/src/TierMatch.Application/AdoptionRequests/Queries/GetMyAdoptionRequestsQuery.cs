using MediatR;

using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Queries.GetMyAdoptionRequests;

public sealed record GetMyAdoptionRequestsQuery
    : IRequest<Result<List<AdoptionRequestDto>>>;