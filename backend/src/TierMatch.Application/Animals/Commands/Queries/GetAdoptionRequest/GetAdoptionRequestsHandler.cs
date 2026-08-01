using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;

public class GetAdoptionRequestsHandler
    : IRequestHandler<GetAdoptionRequestsQuery, List<AdoptionRequestDto>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAdoptionRequestsHandler(
        IAdoptionRequestRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<List<AdoptionRequestDto>> Handle(
        GetAdoptionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var requests = await _repository.GetAllAsync(
            cancellationToken);

        return requests
            .Select(r => r.ToDto())
            .ToList();
    }
}