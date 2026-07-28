using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Shelters.Commands.CreateShelter;

public class CreateShelterHandler
    : IRequestHandler<CreateShelterCommand, Guid>
{
    private readonly IShelterRepository _repository;

    public CreateShelterHandler(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<Guid> Handle(
        CreateShelterCommand request,
        CancellationToken cancellationToken)
    {
        var shelter = new Shelter
        {
            Name = request.Name,
            Street = request.Street,
            HouseNumber = request.HouseNumber,
            PostalCode = request.PostalCode,
            City = request.City,
            Country = request.Country,
            PhoneNumber = request.PhoneNumber,
            Email = request.Email,
            Website = request.Website,
            Description = request.Description
        };

        await _repository.AddAsync(shelter, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        return shelter.Id;
    }
}