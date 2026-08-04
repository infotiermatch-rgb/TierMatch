using FluentAssertions;
using Moq;

using TierMatch.Application.Animals.Queries.GetAnimals;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Queries.GetAnimals;

public sealed class GetAnimalsHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly GetAnimalsHandler _handler;

    public GetAnimalsHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new GetAnimalsHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Animals()
    {
        // Arrange
        var shelterId = Guid.NewGuid();

        var animals = new List<Animal>
        {
            new()
            {
                Id = Guid.NewGuid(),
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
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Mimi",
                Species = AnimalSpecies.Cat,
                Breed = "British Shorthair",
                Gender = AnimalGender.Female,
                Size = AnimalSize.Small,
                BirthDate = new DateOnly(2021, 8, 20),
                Description = "Very calm cat",
                IsVaccinated = true,
                IsCastrated = true,
                Status = AnimalStatus.Available,
                ShelterId = shelterId
            }
        };

        _repositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<AnimalStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animals);

        var query = new GetAnimalsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().HaveCount(2);

        var animalDtos = result.Value!;

        animalDtos[0].Name.Should().Be("Bello");
        animalDtos[0].Species.Should().Be("Dog");
        animalDtos[0].Breed.Should().Be("Labrador");

        animalDtos[1].Name.Should().Be("Mimi");
        animalDtos[1].Species.Should().Be("Cat");
        animalDtos[1].Breed.Should().Be("British Shorthair");

        _repositoryMock.Verify(
            repository => repository.GetAllAsync(
                It.IsAny<AnimalStatus?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Animals_Exist()
    {
        // Arrange
        _repositoryMock
            .Setup(repository => repository.GetAllAsync(
                It.IsAny<AnimalStatus?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Animal>());

        var query = new GetAnimalsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBeNull();
        result.Value.Should().BeEmpty();

        _repositoryMock.Verify(
            repository => repository.GetAllAsync(
                It.IsAny<AnimalStatus?>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}