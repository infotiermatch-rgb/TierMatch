using MediatR;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Models;

namespace TierMatch.Application.Shelters.Queries.GetShelters;

public class GetSheltersHandler
    : IRequestHandler<GetSheltersQuery, List<ShelterDto>>
{
    private readonly IShelterRepository _repository;

    public GetSheltersHandler(IShelterRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ShelterDto>> Handle(
        GetSheltersQuery request,
        CancellationToken cancellationToken)
    {
        var shelters = await _repository.GetAllAsync(cancellationToken);

        return shelters
            .Select(s => new ShelterDto
            {
                Id = s.Id,
                Name = s.Name,
                Street = s.Street,
                HouseNumber = s.HouseNumber,
                PostalCode = s.PostalCode,
                City = s.City,
                Country = s.Country,
                PhoneNumber = s.PhoneNumber,
                Email = s.Email,
                Website = s.Website,
                Description = s.Description
            })
            .ToList();
    }
}