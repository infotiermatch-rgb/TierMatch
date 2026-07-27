using MediatR;
using TierMatch.Application.Animals.DTOs;

namespace TierMatch.Application.Animals.Queries.GetAnimalById;

public record GetAnimalByIdQuery(Guid Id) : IRequest<AnimalDto?>;