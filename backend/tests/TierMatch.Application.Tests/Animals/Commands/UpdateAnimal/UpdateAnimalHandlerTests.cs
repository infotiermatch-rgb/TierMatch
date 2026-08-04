using FluentAssertions;
using Moq;

using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Application.Authorization;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.UpdateAnimal;

public sealed class UpdateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateAnimalHandler _handler;

 public UpdateAnimalHandlerTests()
{
    _repositoryMock = new Mock<IAnimalRepository>();
    _unitOfWorkMock = new Mock<IUnitOfWork>();
    _currentUserServiceMock = new Mock<ICurrentUserService>();

    _currentUserServiceMock
        .SetupGet(service => service.IsAuthenticated)
        .Returns(true);

    _currentUserServiceMock
        .Setup(service => service.IsInRole(Roles.Admin))
        .Returns(true);

    _handler = new UpdateAnimalHandler(
        _repositoryMock.Object,
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);
}

    [Fact]
    public async Task Handle_Should_Update_Existing_Animal()
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
            Description = "Friendly dog",
            IsVaccinated = false,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new UpdateAnimalCommand
        {
            Id = animalId,
            Name = "Max",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2021, 1, 1),
            Description = "Updated description",
            IsVaccinated = true,
            IsCastrated = true,
            Status = AnimalStatus.Reserved,
            ShelterId = shelterId
        };

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        animal.Id.Should().Be(animalId);
        animal.Name.Should().Be(command.Name);
        animal.Species.Should().Be(command.Species);
        animal.Breed.Should().Be(command.Breed);
        animal.Gender.Should().Be(command.Gender);
        animal.Size.Should().Be(command.Size);
        animal.BirthDate.Should().Be(command.BirthDate);
        animal.Description.Should().Be(command.Description);
        animal.IsVaccinated.Should().Be(command.IsVaccinated);
        animal.IsCastrated.Should().Be(command.IsCastrated);
        animal.Status.Should().Be(command.Status);
        animal.ShelterId.Should().Be(command.ShelterId);

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.Update(animal),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var animalId = Guid.NewGuid();
        var shelterId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var command = new UpdateAnimalCommand
        {
            Id = animalId,
            Name = "Max",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = null,
            Description = string.Empty,
            IsVaccinated = false,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.Update(
                It.IsAny<Animal>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}