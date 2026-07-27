using MediatR;
using TierMatch.Application.Animals.DTOs;

namespace TierMatch.Application.Animals.Queries.GetAnimals;

public record GetAnimalsQuery : IRequest<List<AnimalDto>>;