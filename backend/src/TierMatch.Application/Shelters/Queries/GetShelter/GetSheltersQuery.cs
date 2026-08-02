using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelters;

public sealed record GetSheltersQuery
    : IRequest<Result<List<ShelterDto>>>;