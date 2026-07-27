using FluentValidation.TestHelper;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.UpdateAnimal;

public class UpdateAnimalValidatorTests
{
    private readonly UpdateAnimalValidator _validator;

    public UpdateAnimalValidatorTests()
    {
        _validator = new UpdateAnimalValidator();
    }

    [Fact]
    public void Should_Not_Have_Validation_Error_When_Command_Is_Valid()
    {
        // Arrange
        var command = CreateValidCommand();

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Id_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Id = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Id);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = string.Empty
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Name_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Name = new string('A', 101)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Breed_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Breed = new string('B', 101)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Description_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Description = new string('D', 2001)
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Species_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Species = (AnimalSpecies)999
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Species);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Gender_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Gender = (AnimalGender)999
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Size_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand() with
        {
            Size = (AnimalSize)999
        };

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    private static UpdateAnimalCommand CreateValidCommand()
    {
        return new UpdateAnimalCommand(
            Guid.NewGuid(),
            "Bello",
            AnimalSpecies.Dog,
            "Labrador",
            AnimalGender.Male,
            AnimalSize.Medium,
            new DateOnly(2022, 5, 10),
            "Friendly family dog",
            true,
            false);
    }
}