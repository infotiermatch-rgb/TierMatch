using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Common.Mapping;

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
            Status = animal.Status.ToString(),

            BirthDate = animal.BirthDate,
            Description = animal.Description,

            IsVaccinated = animal.IsVaccinated,
            IsCastrated = animal.IsCastrated,

            ShelterId = animal.ShelterId,
            ShelterName = animal.Shelter?.Name
        };
    }

    public static List<AnimalDto> ToDto(
        this IEnumerable<Animal> animals)
    {
        return animals
            .Select(a => a.ToDto())
            .ToList();
    }
}