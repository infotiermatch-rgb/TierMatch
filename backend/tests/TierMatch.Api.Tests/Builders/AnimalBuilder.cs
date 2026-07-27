using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Tests.Builders;

public sealed class AnimalBuilder
{
    private readonly CreateAnimalCommand _command = new()
    {
        Name = "Bello",
        Species = AnimalSpecies.Dog,
        Breed = "Labrador",
        Gender = AnimalGender.Male,
        Size = AnimalSize.Medium,
        BirthDate = new DateOnly(2022, 5, 10),
        Description = "Friendly dog",
        IsVaccinated = true,
        IsCastrated = false
    };

    public static AnimalBuilder Create()
    {
        return new AnimalBuilder();
    }

    public AnimalBuilder WithName(string name)
    {
        _command.Name = name;
        return this;
    }

    public AnimalBuilder WithSpecies(AnimalSpecies species)
    {
        _command.Species = species;
        return this;
    }

    public AnimalBuilder WithBreed(string breed)
    {
        _command.Breed = breed;
        return this;
    }

    public AnimalBuilder WithGender(AnimalGender gender)
    {
        _command.Gender = gender;
        return this;
    }

    public AnimalBuilder WithSize(AnimalSize size)
    {
        _command.Size = size;
        return this;
    }

    public AnimalBuilder WithBirthDate(DateOnly? birthDate)
    {
        _command.BirthDate = birthDate;
        return this;
    }

    public AnimalBuilder WithDescription(string description)
    {
        _command.Description = description;
        return this;
    }

    public AnimalBuilder Vaccinated(bool vaccinated = true)
    {
        _command.IsVaccinated = vaccinated;
        return this;
    }

    public AnimalBuilder Castrated(bool castrated = true)
    {
        _command.IsCastrated = castrated;
        return this;
    }

    public CreateAnimalCommand Build()
    {
        return _command;
    }
}