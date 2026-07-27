using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.CreateAnimal;

public class CreateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly CreateAnimalHandler _handler;

    public CreateAnimalHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new CreateAnimalHandler(_repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_New_Animal()
    {
        // Arrange

        Animal? savedAnimal = null;

        _repositoryMock
            .Setup(r => r.AddAsync(
                It.IsAny<Animal>(),
                It.IsAny<CancellationToken>()))
            .Callback<Animal, CancellationToken>((animal, _) =>
            {
                savedAnimal = animal;
            })
            .Returns(Task.CompletedTask);

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

        var id = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert

        id.Should().NotBe(Guid.Empty);

        savedAnimal.Should().NotBeNull();

        savedAnimal!.Name.Should().Be(command.Name);
        savedAnimal.Species.Should().Be(command.Species);
        savedAnimal.Breed.Should().Be(command.Breed);
        savedAnimal.Gender.Should().Be(command.Gender);
        savedAnimal.Size.Should().Be(command.Size);
        savedAnimal.BirthDate.Should().Be(command.BirthDate);
        savedAnimal.Description.Should().Be(command.Description);
        savedAnimal.IsVaccinated.Should().Be(command.IsVaccinated);
        savedAnimal.IsCastrated.Should().Be(command.IsCastrated);

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
}