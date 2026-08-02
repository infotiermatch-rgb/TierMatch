using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimalById;

public class GetAnimalByIdHandler
    : IRequestHandler<GetAnimalByIdQuery, Result<AnimalDto>>
{
    private readonly IAnimalRepository _repository;

    public GetAnimalByIdHandler(
        IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AnimalDto>> Handle(
        GetAnimalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
        {
            return Result<AnimalDto>.NotFound(
                "Tier wurde nicht gefunden.");
        }

        return Result<AnimalDto>.Success(
            animal.ToDto());
    }
}