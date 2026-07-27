using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals;

public class CreateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly CreateAnimalHandler _handler;

    public CreateAnimalHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new CreateAnimalHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Animal_And_Return_Id()
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
            Description = "Friendly dog",
            IsVaccinated = true,
            IsCastrated = false
        };

        _repositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<Animal>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().NotBe(Guid.Empty);

        _repositoryMock.Verify(
            r => r.AddAsync(
                It.IsAny<Animal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Map_All_Properties_Correctly()
    {
        // Arrange
        var command = new CreateAnimalCommand
        {
            Name = "Rocky",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2020, 4, 15),
            Description = "Very friendly",
            IsVaccinated = false,
            IsCastrated = true
        };

        Animal? capturedAnimal = null;

        _repositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<Animal>(),
                It.IsAny<CancellationToken>()))
            .Callback<Animal, CancellationToken>((animal, _) =>
            {
                capturedAnimal = animal;
            })
            .Returns(Task.CompletedTask);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        capturedAnimal.Should().NotBeNull();

        capturedAnimal!.Name.Should().Be(command.Name);
        capturedAnimal.Species.Should().Be(command.Species);
        capturedAnimal.Breed.Should().Be(command.Breed);
        capturedAnimal.Gender.Should().Be(command.Gender);
        capturedAnimal.Size.Should().Be(command.Size);
        capturedAnimal.BirthDate.Should().Be(command.BirthDate);
        capturedAnimal.Description.Should().Be(command.Description);
        capturedAnimal.IsVaccinated.Should().Be(command.IsVaccinated);
        capturedAnimal.IsCastrated.Should().Be(command.IsCastrated);
    }
}