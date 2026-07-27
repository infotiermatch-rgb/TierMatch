using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Queries.GetAnimalById;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Queries.GetAnimalById;

public class GetAnimalByIdHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly GetAnimalByIdHandler _handler;

    public GetAnimalByIdHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();
        _handler = new GetAnimalByIdHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_AnimalDto_When_Animal_Exists()
    {
        // Arrange
        var animalId = Guid.NewGuid();

        var animal = new Animal
        {
            Id = animalId,
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

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var query = new GetAnimalByIdQuery(animalId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();

        result!.Id.Should().Be(animal.Id);
        result.Name.Should().Be(animal.Name);
        result.Species.Should().Be(animal.Species.ToString());
        result.Breed.Should().Be(animal.Breed);
        result.Gender.Should().Be(animal.Gender.ToString());
        result.Size.Should().Be(animal.Size.ToString());
        result.BirthDate.Should().Be(animal.BirthDate);
        result.Description.Should().Be(animal.Description);
        result.IsVaccinated.Should().BeTrue();
        result.IsCastrated.Should().BeFalse();

        _repositoryMock.Verify(
            r => r.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Null_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var animalId = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var query = new GetAnimalByIdQuery(animalId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().BeNull();

        _repositoryMock.Verify(
            r => r.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}