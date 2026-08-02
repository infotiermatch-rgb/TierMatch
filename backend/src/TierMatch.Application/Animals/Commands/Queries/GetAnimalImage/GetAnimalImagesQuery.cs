using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.Animals.Queries.GetAnimalImages;

public sealed record GetAnimalImagesQuery(
    Guid AnimalId
) : IRequest<Result<List<AnimalImageDto>>>;