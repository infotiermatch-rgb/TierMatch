using MediatR;

using TierMatch.Application.Common.Results;
using TierMatch.Application.ShelterRegistrations.DTOs;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.ShelterRegistrations.Queries.GetShelterRegistrations;

public sealed record GetShelterRegistrationsQuery(
    ShelterRegistrationStatus? Status)
    : IRequest<
        Result<List<ShelterRegistrationListItemDto>>>;