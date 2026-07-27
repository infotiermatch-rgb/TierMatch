using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.UpdateAnimal;

public class UpdateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly UpdateAnimalHandler _handler;

    public UpdateAnimalHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();
        _handler = new UpdateAnimalHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Existing_Animal()
    {
        // Arrange
        var animal = new Animal
        {
            Name = "Bello",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var command = new UpdateAnimalCommand(
            Guid.NewGuid(),
            "Max",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 1, 1),
            "Updated description",
            true,
            true);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        animal.Name.Should().Be("Max");
        animal.Breed.Should().Be("Golden Retriever");
        animal.Size.Should().Be(AnimalSize.Large);
        animal.Description.Should().Be("Updated description");
        animal.IsVaccinated.Should().BeTrue();
        animal.IsCastrated.Should().BeTrue();

        _repositoryMock.Verify(r => r.Update(animal), Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Animal_Does_Not_Exist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var command = new UpdateAnimalCommand(
            Guid.NewGuid(),
            "Max",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            null,
            "",
            false,
            false);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(
            r => r.Update(It.IsAny<Animal>()),
            Times.Never);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(It.IsAny<CancellationToken>()),
            Times.Never);
    }
}