using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public class CreateAdoptionRequestHandler
    : IRequestHandler<CreateAdoptionRequestCommand, Guid>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IAnimalRepository _animalRepository;

    public CreateAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        IAnimalRepository animalRepository)
    {
        _repository = repository;
        _animalRepository = animalRepository;
    }

    public async Task<Guid> Handle(
        CreateAdoptionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
            throw new KeyNotFoundException("Animal not found.");

        if (animal.Status != AnimalStatus.Available)
            throw new InvalidOperationException(
                "Dieses Tier steht nicht mehr zur Adoption.");

        var adoptionRequest = new AdoptionRequest
        {
            AnimalId = request.AnimalId,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            Message = request.Message
        };

        await _repository.AddAsync(
            adoptionRequest,
            cancellationToken);

        await _repository.SaveChangesAsync(
            cancellationToken);

        return adoptionRequest.Id;
    }
}