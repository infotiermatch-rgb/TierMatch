using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Queries.GetAnimals;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Queries.GetAnimals;

public class GetAnimalsHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly GetAnimalsHandler _handler;

    public GetAnimalsHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();
        _handler = new GetAnimalsHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Return_All_Animals()
    {
        // Arrange
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
                IsCastrated = false
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
                IsCastrated = true
            }
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(animals);

        var query = new GetAnimalsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().HaveCount(2);

        result[0].Name.Should().Be("Bello");
        result[0].Species.Should().Be("Dog");
        result[0].Breed.Should().Be("Labrador");

        result[1].Name.Should().Be("Mimi");
        result[1].Species.Should().Be("Cat");
        result[1].Breed.Should().Be("British Shorthair");

        _repositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Empty_List_When_No_Animals_Exist()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Animal>());

        var query = new GetAnimalsQuery();

        // Act
        var result = await _handler.Handle(
            query,
            CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();

        _repositoryMock.Verify(
            r => r.GetAllAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }
}