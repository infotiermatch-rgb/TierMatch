using FluentAssertions;
using Moq;

using TierMatch.Application.Animals.Queries.GetAnimalById;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Queries.GetAnimalById;

public sealed class GetAnimalByIdHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly GetAnimalByIdHandler _handler;

    public GetAnimalByIdHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new GetAnimalByIdHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_AnimalDto_When_Animal_Exists()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var shelterId = Guid.NewGuid();

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
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        var query = new GetAnimalByIdQuery(animalId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();

        var dto = result.Value!;

        dto.Id.Should().Be(animal.Id);
        dto.Name.Should().Be(animal.Name);
        dto.Species.Should().Be(animal.Species.ToString());
        dto.Breed.Should().Be(animal.Breed);
        dto.Gender.Should().Be(animal.Gender.ToString());
        dto.Size.Should().Be(animal.Size.ToString());
        dto.BirthDate.Should().Be(animal.BirthDate);
        dto.Description.Should().Be(animal.Description);
        dto.IsVaccinated.Should().Be(animal.IsVaccinated);
        dto.IsCastrated.Should().Be(animal.IsCastrated);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var animalId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var query = new GetAnimalByIdQuery(animalId);

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}