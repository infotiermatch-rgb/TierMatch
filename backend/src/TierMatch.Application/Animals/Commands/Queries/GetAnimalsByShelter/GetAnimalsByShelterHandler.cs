using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimalsByShelter;

public class GetAnimalsByShelterHandler
    : IRequestHandler<GetAnimalsByShelterQuery, List<AnimalDto>>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAnimalsByShelterHandler(IAnimalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AnimalDto>> Handle(
        GetAnimalsByShelterQuery request,
        CancellationToken cancellationToken)
    {
        var animals = await _repository.GetByShelterIdAsync(
            request.ShelterId,
            cancellationToken);

        return animals
            .Select(a => a.ToDto())
            .ToList();
    }
}