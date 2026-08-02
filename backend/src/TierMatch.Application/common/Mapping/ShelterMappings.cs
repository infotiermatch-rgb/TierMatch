using TierMatch.Application.Shelters.Models;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mapping;

public static class ShelterMappings
{
    public static ShelterDto ToDto(this Shelter shelter)
    {
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

    public static List<ShelterDto> ToDto(
        this IEnumerable<Shelter> shelters)
    {
        return shelters
            .Select(s => s.ToDto())
            .ToList();
    }
}