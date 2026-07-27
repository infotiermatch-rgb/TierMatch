using FluentValidation.TestHelper;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.CreateAnimal;

public class CreateAnimalValidatorTests
{
    private readonly CreateAnimalValidator _validator = new();

    [Fact]
    public void Should_Not_Have_Validation_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = new CreateAnimalCommand
        {
            Name = "Bello",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description = "Friendly family dog",
            IsVaccinated = true,
            IsCastrated = false
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Name_Is_Empty()
    {
        var command = new CreateAnimalCommand
        {
            Name = string.Empty,
            Species = AnimalSpecies.Dog,
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Name_Is_Too_Long()
    {
        var command = new CreateAnimalCommand
        {
            Name = new string('A', 101),
            Species = AnimalSpecies.Dog,
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Breed_Is_Too_Long()
    {
        var command = new CreateAnimalCommand
        {
            Name = "Bello",
            Breed = new string('B', 101),
            Species = AnimalSpecies.Dog,
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Description_Is_Too_Long()
    {
        var command = new CreateAnimalCommand
        {
            Name = "Bello",
            Breed = "Labrador",
            Species = AnimalSpecies.Dog,
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            Description = new string('D', 2001)
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }
}