using FluentValidation.TestHelper;

using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.CreateAnimal;

public sealed class CreateAnimalValidatorTests
{
    private readonly CreateAnimalValidator _validator = new();

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
    public void Should_Have_Validation_Error_When_Name_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Name = string.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Name_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Name = new string('A', 101);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Breed_Is_Empty()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Breed = string.Empty;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Breed_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Breed = new string('B', 101);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Breed);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Description_Is_Too_Long()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Description = new string('D', 2001);

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_BirthDate_Is_In_The_Future()
    {
        // Arrange
        var command = CreateValidCommand();
        command.BirthDate = DateOnly.FromDateTime(
            DateTime.Today.AddDays(1));

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.BirthDate);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_ShelterId_Is_Null()
    {
        // Arrange
        var command = CreateValidCommand();
        command.ShelterId = null;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ShelterId);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Species_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Species = (AnimalSpecies)999;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Species);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Gender_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Gender = (AnimalGender)999;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Gender);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Size_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Size = (AnimalSize)999;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Size);
    }

    [Fact]
    public void Should_Have_Validation_Error_When_Status_Is_Invalid()
    {
        // Arrange
        var command = CreateValidCommand();
        command.Status = (AnimalStatus)999;

        // Act
        var result = _validator.TestValidate(command);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    private static CreateAnimalCommand CreateValidCommand()
    {
        return new CreateAnimalCommand
        {
            Name = "Bello",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description = "Friendly family dog",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = Guid.NewGuid()
        };
    }
}