using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Tests.Builders;

public sealed class UpdateAnimalBuilder
{
    private Guid _id;
    private string _name = "Rocky";
    private AnimalSpecies _species = AnimalSpecies.Dog;
    private string _breed = "Golden Retriever";
    private AnimalGender _gender = AnimalGender.Male;
    private AnimalSize _size = AnimalSize.Large;
    private DateOnly? _birthDate = new DateOnly(2021, 3, 20);
    private string _description = "Updated description";
    private bool _isVaccinated;
    private bool _isCastrated = true;

    private UpdateAnimalBuilder(Guid id)
    {
        _id = id;
    }

    public static UpdateAnimalBuilder Create(Guid id)
    {
        return new UpdateAnimalBuilder(id);
    }

    public UpdateAnimalBuilder WithId(Guid id)
    {
        _id = id;
        return this;
    }

    public UpdateAnimalBuilder WithName(string name)
    {
        _name = name;
        return this;
    }

    public UpdateAnimalBuilder WithSpecies(AnimalSpecies species)
    {
        _species = species;
        return this;
    }

    public UpdateAnimalBuilder WithBreed(string breed)
    {
        _breed = breed;
        return this;
    }

    public UpdateAnimalBuilder WithGender(AnimalGender gender)
    {
        _gender = gender;
        return this;
    }

    public UpdateAnimalBuilder WithSize(AnimalSize size)
    {
        _size = size;
        return this;
    }

    public UpdateAnimalBuilder WithBirthDate(DateOnly? birthDate)
    {
        _birthDate = birthDate;
        return this;
    }

    public UpdateAnimalBuilder WithDescription(string description)
    {
        _description = description;
        return this;
    }

    public UpdateAnimalBuilder Vaccinated(bool vaccinated = true)
    {
        _isVaccinated = vaccinated;
        return this;
    }

    public UpdateAnimalBuilder Castrated(bool castrated = true)
    {
        _isCastrated = castrated;
        return this;
    }

    public UpdateAnimalCommand Build()
    {
        return new UpdateAnimalCommand
        {
            Id = _id,
            Name = _name,
            Species = _species,
            Breed = _breed,
            Gender = _gender,
            Size = _size,
            BirthDate = _birthDate,
            Description = _description,
            IsVaccinated = _isVaccinated,
            IsCastrated = _isCastrated
        };
    }
}