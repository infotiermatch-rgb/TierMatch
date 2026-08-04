using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimal;

public sealed class UpdateAnimalHandler
    : IRequestHandler<UpdateAnimalCommand, Result>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAnimalHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        UpdateAnimalCommand request,
        CancellationToken cancellationToken)
    {
        if (!_currentUserService.IsAuthenticated)
        {
            return Result.Unauthorized();
        }

        var isAdmin =
            _currentUserService.IsInRole(Roles.Admin);

        var isShelterAdmin =
            _currentUserService.IsInRole(Roles.ShelterAdmin);

        if (!isAdmin && !isShelterAdmin)
        {
            return Result.Forbidden();
        }

        var animal = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (animal is null)
        {
            return Result.NotFound(
                "Tier wurde nicht gefunden.");
        }

        /*
         * Ein Administrator darf jedes Tier bearbeiten und
         * bei Bedarf einem anderen Tierheim zuordnen.
         *
         * Ein ShelterAdmin darf ausschließlich Tiere seines
         * eigenen Tierheims bearbeiten und die Tierheim-
         * Zuordnung nicht verändern.
         */
        if (isShelterAdmin && !isAdmin)
        {
            var currentShelterId =
                _currentUserService.ShelterId;

            if (!currentShelterId.HasValue)
            {
                return Result.Forbidden();
            }

            if (animal.ShelterId != currentShelterId.Value)
            {
                return Result.Forbidden();
            }

            if (request.ShelterId != currentShelterId.Value)
            {
                return Result.Forbidden();
            }
        }

        animal.Name = request.Name;
        animal.Species = request.Species;
        animal.Breed = request.Breed;
        animal.Gender = request.Gender;
        animal.Size = request.Size;
        animal.BirthDate = request.BirthDate;
        animal.Description = request.Description;
        animal.IsVaccinated = request.IsVaccinated;
        animal.IsCastrated = request.IsCastrated;
        animal.Status = request.Status;
        animal.ShelterId = request.ShelterId;

        _repository.Update(animal);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}