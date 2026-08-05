using MediatR;

using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Application.AdoptionRequests.Commands.CreateAdoptionRequest;

public class CreateAdoptionRequestHandler
    : IRequestHandler<
        CreateAdoptionRequestCommand,
        Result<Guid>>
{
    private readonly IAdoptionRequestRepository _repository;
    private readonly IAnimalRepository _animalRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateAdoptionRequestHandler(
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

    public async Task<Result<Guid>> Handle(
        CreateAdoptionRequestCommand request,
        CancellationToken cancellationToken)
    {
        var userId = _currentUserService.UserId;

        if (!_currentUserService.IsAuthenticated ||
            userId is null)
        {
            return Result<Guid>.Unauthorized();
        }

        var animal =
            await _animalRepository.GetByIdAsync(
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

        var pendingRequestExists =
            await _repository.HasPendingRequestAsync(
                userId.Value,
                request.AnimalId,
                cancellationToken);

        if (pendingRequestExists)
        {
            return Result<Guid>.Conflict(
                "Du hast für dieses Tier bereits eine offene Adoptionsanfrage gestellt.");
        }

        var adoptionRequest = new AdoptionRequest
        {
            AnimalId = request.AnimalId,
            UserId = userId.Value,
            FirstName = request.FirstName.Trim(),
            LastName = request.LastName.Trim(),
            Email = request.Email.Trim(),
            PhoneNumber = request.PhoneNumber.Trim(),
            Message = request.Message.Trim(),
            Status = AdoptionRequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(
            adoptionRequest,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(
            adoptionRequest.Id);
    }
}