using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimalImages;

public class GetAnimalImagesHandler
    : IRequestHandler<GetAnimalImagesQuery, Result<List<AnimalImageDto>>>
{
    private readonly IAnimalImageRepository _repository;

    public GetAnimalImagesHandler(
        IAnimalImageRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AnimalImageDto>>> Handle(
        GetAnimalImagesQuery request,
        CancellationToken cancellationToken)
    {
        var images = await _repository.GetByAnimalIdAsync(
            request.AnimalId,
            cancellationToken);

        return Result<List<AnimalImageDto>>.Success(
            images.ToDto());
    }
}