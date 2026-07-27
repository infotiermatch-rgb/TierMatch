using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals;

public class UpdateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly UpdateAnimalHandler _handler;

    public UpdateAnimalHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new UpdateAnimalHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Animal_And_Return_True()
    {
        // Arrange
        var id = Guid.NewGuid();

        var animal = new Animal
        {
            Id = id,
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

        var command = new UpdateAnimalCommand(
            id,
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            false,
            true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        animal.Name.Should().Be(command.Name);
        animal.Species.Should().Be(command.Species);
        animal.Breed.Should().Be(command.Breed);
        animal.Gender.Should().Be(command.Gender);
        animal.Size.Should().Be(command.Size);
        animal.BirthDate.Should().Be(command.BirthDate);
        animal.Description.Should().Be(command.Description);
        animal.IsVaccinated.Should().Be(command.IsVaccinated);
        animal.IsCastrated.Should().Be(command.IsCastrated);

        _repositoryMock.Verify(
            r => r.Update(animal),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        var command = new UpdateAnimalCommand(
            id,
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            false,
            true);

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(
            r => r.Update(It.IsAny<Animal>()),
            Times.Never);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Call_GetById_Exactly_Once()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Animal());

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new UpdateAnimalCommand(
            id,
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            false,
            true);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()),
            Times.Once);
    }
}