using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimals;

public class GetAnimalsHandler
    : IRequestHandler<GetAnimalsQuery, List<AnimalDto>>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAnimalsHandler(IAnimalRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AnimalDto>> Handle(
        GetAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var animals = await _repository.GetAllAsync(
            request.Status,
            cancellationToken);

        return animals
            .Select(a => a.ToDto())
            .ToList();
    }
}