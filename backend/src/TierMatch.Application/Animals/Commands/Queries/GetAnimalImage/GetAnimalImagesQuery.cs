using MediatR;
using TierMatch.Application.Animals.DTOs;

namespace TierMatch.Application.Animals.Queries.GetAnimalImages;

public sealed record GetAnimalImagesQuery(
    Guid AnimalId
) : IRequest<List<AnimalImageDto>>;