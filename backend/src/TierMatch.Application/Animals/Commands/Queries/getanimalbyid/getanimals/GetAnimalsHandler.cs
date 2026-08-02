using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimals;

public class GetAnimalsHandler
    : IRequestHandler<GetAnimalsQuery, Result<List<AnimalDto>>>
{
    private readonly IAnimalRepository _repository;

    public GetAnimalsHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AnimalDto>>> Handle(
        GetAnimalsQuery request,
        CancellationToken cancellationToken)
    {
        var animals = await _repository.GetAllAsync(
            request.Status,
            cancellationToken);

        return Result<List<AnimalDto>>.Success(
            animals.ToDto());
    }
}