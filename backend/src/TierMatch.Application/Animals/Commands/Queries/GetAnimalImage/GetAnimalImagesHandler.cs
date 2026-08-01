using MediatR;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Queries.GetAnimalImages;

public class GetAnimalImagesHandler
    : IRequestHandler<GetAnimalImagesQuery, List<AnimalImageDto>>
{
    private readonly IAnimalImageRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAnimalImagesHandler(
        IAnimalImageRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AnimalImageDto>> Handle(
        GetAnimalImagesQuery request,
        CancellationToken cancellationToken)
    {
        var images = await _repository.GetByAnimalIdAsync(
            request.AnimalId,
            cancellationToken);

        return images
            .Select(i => i.ToDto())
            .ToList();
    }
}