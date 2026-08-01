using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Application.Common.Results;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public class CreateAdoptionRequestHandler
    : IRequestHandler<CreateAdoptionRequestCommand, Result<Guid>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IAnimalRepository _animalRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdoptionRequestHandler(
        IAdoptionRequestRepository repository,
        IAnimalRepository animalRepository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _animalRepository = animalRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<Guid>> Handle(
    CreateAdoptionRequestCommand request,
    CancellationToken cancellationToken)
    {
        var animal = await _animalRepository.GetByIdAsync(
            request.AnimalId,
            cancellationToken);

        if (animal is null)
{
    return Result<Guid>.NotFound(
        "Tier wurde nicht gefunden.");
}

        if (animal.Status != AnimalStatus.Available)
{
    return Result<Guid>.Validation(
        "Dieses Tier steht nicht mehr zur Adoption.");
}

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

await _unitOfWork.SaveChangesAsync(cancellationToken);

return Result<Guid>.Success(adoptionRequest.Id);
    }
}