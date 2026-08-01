using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;

public sealed record GetAdoptionRequestByIdQuery(
    Guid Id
) : IRequest<AdoptionRequestDto?>;