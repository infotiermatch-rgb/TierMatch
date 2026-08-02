using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Queries.GetAnimalById;

public sealed record GetAnimalByIdQuery(
    Guid Id
) : IRequest<Result<AnimalDto>>;