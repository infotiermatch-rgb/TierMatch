using MediatR;
using TierMatch.Application.Interfaces;

namespace TierMatch.Application.Shelters.Commands.UpdateShelter;

public class UpdateShelterHandler
    : IRequestHandler<UpdateShelterCommand, bool>
{
    private readonly IShelterRepository _repository;

    public UpdateShelterHandler(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(
        UpdateShelterCommand request,
        CancellationToken cancellationToken)
    {
        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return false;
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

        await _repository.SaveChangesAsync(cancellationToken);

        return true;
    }
}