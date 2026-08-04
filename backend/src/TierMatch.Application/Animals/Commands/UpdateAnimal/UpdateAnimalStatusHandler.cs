using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Animals.Commands.UpdateAnimalStatus;

public sealed class UpdateAnimalStatusHandler
    : IRequestHandler<UpdateAnimalStatusCommand, Result>
{
    private readonly IAnimalRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateAnimalStatusHandler(
        IAnimalRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        UpdateAnimalStatusCommand request,
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
         * Ein Administrator darf den Status jedes Tieres ändern.
         *
         * Ein ShelterAdmin darf nur den Status von Tieren
         * seines eigenen Tierheims ändern.
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
        }

        animal.Status = request.Status;

        _repository.Update(animal);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}