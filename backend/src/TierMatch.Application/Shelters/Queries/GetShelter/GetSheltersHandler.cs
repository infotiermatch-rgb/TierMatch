using MediatR;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelters;

public class GetSheltersHandler
    : IRequestHandler<GetSheltersQuery, Result<List<ShelterDto>>>
{
    private readonly IShelterRepository _repository;

    public GetSheltersHandler(
        IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ShelterDto>>> Handle(
        GetSheltersQuery request,
        CancellationToken cancellationToken)
    {
        var shelters = await _repository.GetAllAsync(
            cancellationToken);

        return Result<List<ShelterDto>>.Success(
            shelters.ToDto());
    }
}