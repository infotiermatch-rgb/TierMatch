using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mappings;

public static class AnimalMappings
{
    public static AnimalDto ToDto(this Animal animal)
    {
        return new AnimalDto
        {
            Id = animal.Id,
            Name = animal.Name,
            Breed = animal.Breed,
            Species = animal.Species.ToString(),
            Gender = animal.Gender.ToString(),
            Size = animal.Size.ToString(),
            BirthDate = animal.BirthDate,
            Description = animal.Description,
            IsVaccinated = animal.IsVaccinated,
            IsCastrated = animal.IsCastrated,

            ShelterId = animal.ShelterId,
            ShelterName = animal.Shelter?.Name,

            Status = animal.Status.ToString(),
        };
    }
}