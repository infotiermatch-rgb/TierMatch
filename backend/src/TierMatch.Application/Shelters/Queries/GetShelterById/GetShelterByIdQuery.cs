using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelterById;

public sealed record GetShelterByIdQuery(
    Guid Id
) : IRequest<Result<ShelterDto>>;