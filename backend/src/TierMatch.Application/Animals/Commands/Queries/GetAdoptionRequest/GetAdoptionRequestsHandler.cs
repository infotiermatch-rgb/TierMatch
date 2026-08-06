using MediatR;

using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Mapping;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetAdoptionRequests;

public class GetAdoptionRequestsHandler
    : IRequestHandler<
        GetAdoptionRequestsQuery,
        Result<List<AdoptionRequestDto>>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetAdoptionRequestsHandler(
        IAdoptionRequestRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AdoptionRequestDto>>> Handle(
        GetAdoptionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<List<AdoptionRequestDto>>
                .Unauthorized();
        }

        List<Domain.Entities.AdoptionRequest> requests;

        if (_currentUserService.IsInRole(Roles.Admin))
        {
            requests = await _repository.GetAllAsync(
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
                return Result<List<AdoptionRequestDto>>
                    .Forbidden();
            }

            requests =
                await _repository.GetByShelterIdAsync(
                    shelterId.Value,
                    cancellationToken);
        }
        else
        {
            return Result<List<AdoptionRequestDto>>
                .Forbidden();
        }

        return Result<List<AdoptionRequestDto>>
            .Success(requests.ToDto());
    }
}