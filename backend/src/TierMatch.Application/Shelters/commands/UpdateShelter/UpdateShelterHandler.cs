using MediatR;

using TierMatch.Application.Authorization;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.UpdateShelter;

public sealed class UpdateShelterHandler
    : IRequestHandler<UpdateShelterCommand, Result>
{
    private readonly IShelterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICurrentUserService _currentUserService;

    public UpdateShelterHandler(
        IShelterRepository repository,
        IUnitOfWork unitOfWork,
        ICurrentUserService currentUserService)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
        _currentUserService = currentUserService;
    }

    public async Task<Result> Handle(
        UpdateShelterCommand request,
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

        /*
         * Administratoren dürfen jedes Tierheim bearbeiten.
         *
         * ShelterAdmins dürfen ausschließlich das Tierheim
         * bearbeiten, dessen ID im ShelterId-Claim enthalten ist.
         */
        if (isShelterAdmin && !isAdmin)
        {
            var currentShelterId =
                _currentUserService.ShelterId;

            if (!currentShelterId.HasValue)
            {
                return Result.Forbidden();
            }

            if (currentShelterId.Value != request.Id)
            {
                return Result.Forbidden();
            }
        }

        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return Result.NotFound(
                "Tierheim wurde nicht gefunden.");
        }

        shelter.Name = request.Name;
        shelter.Street = request.Street;
        shelter.HouseNumber = request.HouseNumber;
        shelter.PostalCode = request.PostalCode;
        shelter.City = request.City;
        shelter.Country = request.Country;
        shelter.PhoneNumber = request.PhoneNumber;
        shelter.Email = request.Email;
        shelter.Website = request.Website;
        shelter.Description = request.Description;

        _repository.Update(shelter);

        await _unitOfWork.SaveChangesAsync(
            cancellationToken);

        return Result.Success();
    }
}