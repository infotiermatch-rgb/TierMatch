using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelterById;

public class GetShelterByIdHandler
    : IRequestHandler<GetShelterByIdQuery, ShelterDto?>
{
    private readonly IShelterRepository _repository;

    public GetShelterByIdHandler(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<ShelterDto?> Handle(
        GetShelterByIdQuery request,
        CancellationToken cancellationToken)
    {
        var shelter = await _repository.GetByIdAsync(
            request.Id,
            cancellationToken);

        if (shelter is null)
        {
            return null;
        }

        return new ShelterDto
        {
            Id = shelter.Id,
            Name = shelter.Name,
            Street = shelter.Street,
            HouseNumber = shelter.HouseNumber,
            PostalCode = shelter.PostalCode,
            City = shelter.City,
            Country = shelter.Country,
            PhoneNumber = shelter.PhoneNumber,
            Email = shelter.Email,
            Website = shelter.Website,
            Description = shelter.Description
        };
    }
}