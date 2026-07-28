using MediatR;
using TierMatch.Application.Animals.DTOs;

namespace TierMatch.Application.Animals.Queries.GetAnimalsByShelter;

public sealed record GetAnimalsByShelterQuery(Guid ShelterId)
    : IRequest<List<AnimalDto>>;