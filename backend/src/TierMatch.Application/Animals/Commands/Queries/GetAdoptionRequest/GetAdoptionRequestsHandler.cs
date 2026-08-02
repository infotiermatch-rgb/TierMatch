using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;

public class GetAdoptionRequestsHandler
    : IRequestHandler<GetAdoptionRequestsQuery, Result<List<AdoptionRequestDto>>>
{
    private readonly IAdoptionRequestRepository _repository;

    public GetAdoptionRequestsHandler(
        IAdoptionRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<AdoptionRequestDto>>> Handle(
        GetAdoptionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await _repository.GetAllAsync(
            cancellationToken);

        return Result<List<AdoptionRequestDto>>.Success(
            requests.ToDto());
    }
}