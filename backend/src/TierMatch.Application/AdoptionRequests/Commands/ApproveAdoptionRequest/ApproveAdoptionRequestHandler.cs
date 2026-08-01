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

    public ApproveAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        IAnimalRepository animalRepository)
    {
        _repository = repository;
        _animalRepository = animalRepository;
    }

    public async Task<Result> Handle(
        ApproveAdoptionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var adoptionRequest = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (adoptionRequest is null)
            return Result<bool>.NotFound("Adoption request not found");

        if (adoptionRequest.Status != AdoptionRequestStatus.Pending)
            return Result<bool>.Validation("Adoption request is not pending");

        var animal = await _animalRepository.GetByIdAsync(
            adoptionRequest.AnimalId,
            cancellationToken);

        if (animal is null)
            return Result<bool>.NotFound("Animal not found");

        if (animal.Status != AnimalStatus.Available)
            return Result<bool>.Validation("Animal is not available for adoption");

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

await _repository.SaveChangesAsync(cancellationToken);
await _animalRepository.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }
}
