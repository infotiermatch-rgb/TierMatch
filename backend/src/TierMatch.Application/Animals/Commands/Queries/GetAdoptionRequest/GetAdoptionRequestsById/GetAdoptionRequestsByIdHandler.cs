using MediatR;

using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequestById;

public class GetAdoptionRequestByIdHandler
    : IRequestHandler<
        GetAdoptionRequestByIdQuery,
        Result<AdoptionRequestDto>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetAdoptionRequestByIdHandler(
        IAdoptionRequestRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<AdoptionRequestDto>> Handle(
        GetAdoptionRequestByIdQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<AdoptionRequestDto>
                .Unauthorized();
        }

        AdoptionRequest? adoptionRequest;

        if (_currentUserService.IsInRole(Roles.Admin))
        {
            adoptionRequest =
                await _repository.GetByIdAsync(
                    request.Id,
                    cancellationToken);
        }
        else if (
            _currentUserService.IsInRole(
                Roles.ShelterAdmin))
        {
            var shelterId =
                _currentUserService.ShelterId;

            if (shelterId is null)
            {
                return Result<AdoptionRequestDto>
                    .Forbidden();
            }

            adoptionRequest =
                await _repository
                    .GetByIdAndShelterIdAsync(
                        request.Id,
                        shelterId.Value,
                        cancellationToken);
        }
        else
        {
            return Result<AdoptionRequestDto>
                .Forbidden();
        }

        if (adoptionRequest is null)
        {
            return Result<AdoptionRequestDto>
                .NotFound(
                    "Adoptionsanfrage wurde nicht gefunden.");
        }

        return Result<AdoptionRequestDto>
            .Success(adoptionRequest.ToDto());
    }
}