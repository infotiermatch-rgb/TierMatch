using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.AdoptionRequests.Commands.ApproveAdoptionRequest;

public class ApproveAdoptionRequestHandler
    : IRequestHandler<
        ApproveAdoptionRequestCommand,
        Result>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        IAnimalRepository animalRepository,
        ICurrentUserService currentUserService,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _animalRepository = animalRepository;
        _currentUserService = currentUserService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ApproveAdoptionRequestCommand request,
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

        var animal = adoptionRequest.Animal;

        if (animal is null)
        {
            return Result.NotFound(
                "Tier wurde nicht gefunden.");
        }

        if (animal.Status != AnimalStatus.Available)
        {
            return Result.Validation(
                "Dieses Tier steht nicht mehr zur Adoption.");
        }

        adoptionRequest.Status =
            AdoptionRequestStatus.Approved;

        animal.Status =
            AnimalStatus.Reserved;

        var pendingRequests =
            await _repository.GetPendingByAnimalIdAsync(
                animal.Id,
                cancellationToken);

        foreach (var pendingRequest in pendingRequests)
        {
            if (pendingRequest.Id == adoptionRequest.Id)
            {
                continue;
            }

            pendingRequest.Status =
                AdoptionRequestStatus.Rejected;

            _repository.Update(pendingRequest);
        }

        _repository.Update(adoptionRequest);
        _animalRepository.Update(animal);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}