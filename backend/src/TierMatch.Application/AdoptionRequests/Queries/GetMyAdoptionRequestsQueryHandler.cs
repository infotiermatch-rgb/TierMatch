using MediatR;

using TierMatch.Application.AdoptionRequests.DTOs;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.AdoptionRequests.Queries.GetMyAdoptionRequests;

public sealed class GetMyAdoptionRequestsQueryHandler
    : IRequestHandler<
        GetMyAdoptionRequestsQuery,
        Result<List<AdoptionRequestDto>>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;

    public GetMyAdoptionRequestsQueryHandler(
        IAdoptionRequestRepository repository,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _currentUserService = currentUserService;
    }

    public async Task<Result<List<AdoptionRequestDto>>> Handle(
        GetMyAdoptionRequestsQuery request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!_currentUserService.IsAuthenticated ||
            userId is null)
        {
            return Result<List<AdoptionRequestDto>>
                .Unauthorized();
        }

        var adoptionRequests =
            await _repository.GetByUserIdAsync(
                userId.Value,
                cancellationToken);

        var response = adoptionRequests
            .Select(adoptionRequest =>
                new AdoptionRequestDto
                {
                    Id = adoptionRequest.Id,
                    AnimalId = adoptionRequest.AnimalId,
                    AnimalName =
                        adoptionRequest.Animal?.Name ??
                        string.Empty,
                    FirstName =
                        adoptionRequest.FirstName,
                    LastName =
                        adoptionRequest.LastName,
                    Email =
                        adoptionRequest.Email,
                    PhoneNumber =
                        adoptionRequest.PhoneNumber,
                    Message =
                        adoptionRequest.Message,
                    Status =
                        adoptionRequest.Status,
                    RequestedAt =
                        adoptionRequest.RequestedAt
                })
            .ToList();

        return Result<List<AdoptionRequestDto>>
            .Success(response);
    }
}