using MediatR;
using TierMatch.Application.Common.Results;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.UpdateShelter;

public class UpdateShelterHandler
    : IRequestHandler<UpdateShelterCommand, Result>
{
    private readonly IShelterRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateShelterHandler(
        IShelterRepository repository,
        IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        UpdateShelterCommand request,
        CancellationToken cancellationToken)
    {
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