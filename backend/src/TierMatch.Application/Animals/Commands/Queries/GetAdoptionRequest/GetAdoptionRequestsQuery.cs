using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;

public sealed record GetAdoptionRequestsQuery
    : IRequest<List<AdoptionRequestDto>>;