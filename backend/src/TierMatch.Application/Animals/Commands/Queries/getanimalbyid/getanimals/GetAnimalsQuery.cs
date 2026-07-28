using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.Animals.Queries.GetAnimals;

public sealed record GetAnimalsQuery(
    AnimalStatus? Status = null
) : IRequest<List<AnimalDto>>;