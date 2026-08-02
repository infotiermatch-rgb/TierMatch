using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Enums;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Commands.ApproveAdoptionRequest;

public class ApproveAdoptionRequestHandler
    : IRequestHandler<ApproveAdoptionRequestCommand, Result>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApproveAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        IAnimalRepository animalRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _animalRepository = animalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        ApproveAdoptionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var adoptionRequest = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (adoptionRequest is null)
    return Result.NotFound("Adoptionsanfrage wurde nicht gefunden.");

        if (adoptionRequest.Status != AdoptionRequestStatus.Pending)
            return Result<bool>.Validation("Adoption request is not pending");

        var animal = await _animalRepository.GetByIdAsync(
            adoptionRequest.AnimalId,
            cancellationToken);

        if (animal is null)
    return Result.NotFound("Tier wurde nicht gefunden.");

        if (animal.Status != AnimalStatus.Available)
{
    return Result.Validation(
        "Dieses Tier steht nicht mehr zur Adoption.");
}

        adoptionRequest.Status = AdoptionRequestStatus.Approved;

animal.Status = AnimalStatus.Reserved;

// Alle anderen offenen Anfragen ablehnen
var pendingRequests =
    await _repository.GetPendingByAnimalIdAsync(
        animal.Id,
        cancellationToken);

foreach (var requestItem in pendingRequests)
{
    if (requestItem.Id != adoptionRequest.Id)
    {
        requestItem.Status = AdoptionRequestStatus.Rejected;
        _repository.Update(requestItem);
    }
}

_repository.Update(adoptionRequest);
_animalRepository.Update(animal);


await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
