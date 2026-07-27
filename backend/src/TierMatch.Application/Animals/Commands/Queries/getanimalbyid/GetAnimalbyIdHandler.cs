using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimalById;

public class GetAnimalByIdHandler
    : IRequestHandler<GetAnimalByIdQuery, AnimalDto?>
{
    private readonly IAnimalRepository _repository;

    public GetAnimalByIdHandler(IAnimalRepository repository)
    {
        _repository = repository;
    }

    public async Task<AnimalDto?> Handle(
        GetAnimalByIdQuery request,
        CancellationToken cancellationToken)
    {
        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
            return null;

        return animal.ToDto();
    }
}