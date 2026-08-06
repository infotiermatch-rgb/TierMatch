using MediatR;

using TierMatch.Application.Common.Results;
using TierMatch.Application.ShelterRegistrations.DTOs;

namespace TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrationById;

public sealed record GetShelterRegistrationByIdQuery(
    Guid Id)
    : IRequest<Result<ShelterRegistrationDetailsDto>>;