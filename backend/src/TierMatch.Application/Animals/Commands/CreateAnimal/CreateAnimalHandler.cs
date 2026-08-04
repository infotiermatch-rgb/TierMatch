using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;


namespace TierMatch.Application.Animals.Commands.CreateAnimal;

public sealed class CreateAnimalHandler
    : IRequestHandler<CreateAnimalCommand, Result<Guid>>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;
    

    public CreateAnimalHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result<Guid>> Handle(
        CreateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result<Guid>.Unauthorized();
        }

        var isAdmin =
            _currentUserService.IsInRole(Roles.Admin);

        var isShelterAdmin =
            _currentUserService.IsInRole(Roles.ShelterAdmin);

        if (!isAdmin && !isShelterAdmin)
        {
            return Result<Guid>.Forbidden();
        }

        /*
         * Ein ShelterAdmin darf nur Tiere für sein eigenes
         * Tierheim erstellen.
         *
         * Ein Admin darf Tiere für jedes Tierheim erstellen.
         */
        if (isShelterAdmin && !isAdmin)
        {
            if (!_currentUserService.ShelterId.HasValue)
            {
                return Result<Guid>.Forbidden();
            }

            if (request.ShelterId !=
                _currentUserService.ShelterId)
            {
                return Result<Guid>.Forbidden();
            }
        }

        var animal = new Animal
        {
            Name = request.Name,
            Species = request.Species,
            Breed = request.Breed,
            Gender = request.Gender,
            Size = request.Size,
            BirthDate = request.BirthDate,
            Description = request.Description,
            IsVaccinated = request.IsVaccinated,
            IsCastrated = request.IsCastrated,
            ShelterId = request.ShelterId,
            Status = request.Status
        };

        await _repository.AddAsync(
            animal,
            cancellationToken);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result<Guid>.Success(animal.Id);
    }
}