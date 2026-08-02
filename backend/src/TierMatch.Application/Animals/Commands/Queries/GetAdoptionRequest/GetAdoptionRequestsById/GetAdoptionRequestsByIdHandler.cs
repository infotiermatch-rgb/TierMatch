using MediatR;
using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;

public class GetAdoptionRequestByIdHandler
    : IRequestHandler<GetAdoptionRequestByIdQuery, Result<AdoptionRequestDto>>
{
    private readonly IAdoptionRequestRepository _repository;

    public GetAdoptionRequestByIdHandler(
        IAdoptionRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<AdoptionRequestDto>> Handle(
        GetAdoptionRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        var adoptionRequest = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (adoptionRequest is null)
        {
            return Result<AdoptionRequestDto>.NotFound(
                "Adoptionsanfrage wurde nicht gefunden.");
        }

        return Result<AdoptionRequestDto>.Success(
            adoptionRequest.ToDto());
    }
}