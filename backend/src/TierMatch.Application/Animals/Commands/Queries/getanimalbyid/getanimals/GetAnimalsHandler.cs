using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimals;

public class GetAnimalsHandler
    : IRequestHandler<GetAnimalsQuery, List<AnimalDto>>
{
    private readonly IAnimalRepository _repository;

    public GetAnimalsHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<AnimalDto>> Handle(
        GetAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var animals = await _repository.GetAllAsync(cancellationToken);

        return animals
            .Select(a => a.ToDto())
            .ToList();
    }
}