using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Mappings;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;

public class GetAdoptionRequestByIdHandler
    : IRequestHandler<GetAdoptionRequestByIdQuery, AdoptionRequestDto?>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public GetAdoptionRequestByIdHandler(
        IAdoptionRequestRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<AdoptionRequestDto?> Handle(
        GetAdoptionRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var adoptionRequest = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (adoptionRequest is null)
            return null;

        return adoptionRequest.ToDto();
    }
}