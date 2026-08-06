using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.AdoptionRequests.Commands.RejectAdoptionRequest;

public sealed class RejectAdoptionRequestHandler
    : IRequestHandler<
        RejectAdoptionRequestCommand,
        Result>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public RejectAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RejectAdoptionRequestCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Unauthorized();
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
                return Result.Forbidden();
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
            return Result.Forbidden();
        }

        if (adoptionRequest is null)
        {
            return Result.NotFound(
                "Adoptionsanfrage wurde nicht gefunden.");
        }

        if (
            adoptionRequest.Status !=
            AdoptionRequestStatus.Pending)
        {
            return Result.Validation(
                "Die Adoptionsanfrage wurde bereits bearbeitet.");
        }

        adoptionRequest.Status =
            AdoptionRequestStatus.Rejected;

        _repository.Update(adoptionRequest);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}