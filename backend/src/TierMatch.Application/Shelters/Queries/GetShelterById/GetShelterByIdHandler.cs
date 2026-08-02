using MediatR;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelterById;

public class GetShelterByIdHandler
    : IRequestHandler<GetShelterByIdQuery, Result<ShelterDto>>
{
    private readonly IShelterRepository _repository;

    public GetShelterByIdHandler(
        IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ShelterDto>> Handle(
        GetShelterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return Result<ShelterDto>.NotFound(
                "Tierheim wurde nicht gefunden.");
        }

        return Result<ShelterDto>.Success(
            shelter.ToDto());
    }
}