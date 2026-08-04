using FluentAssertions;
using Moq;

using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;
using TierMatch.Application.Authorization;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.CreateAnimal;

public sealed class CreateAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateAnimalHandler _handler;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

    public CreateAnimalHandlerTests()
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

    _handler = new CreateAnimalHandler(
        _repositoryMock.Object,
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);
}

    [Fact]
    public async Task Handle_Should_Create_New_Animal()
    {
        // Arrange
        var shelterId = Guid.NewGuid();
        Animal? savedAnimal = null;

        _repositoryMock
    .Setup(repository => repository.AddAsync(
        It.IsAny<Animal>(),
        It.IsAny<CancellationToken>()))
    .Callback<Animal, CancellationToken>((animal, _) =>
    {
        animal.Id = Guid.NewGuid();
        savedAnimal = animal;
    })
    .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

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
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().NotBe(Guid.Empty);

        savedAnimal.Should().NotBeNull();

        savedAnimal!.Id.Should().Be(result.Value);
        savedAnimal.Name.Should().Be(command.Name);
        savedAnimal.Species.Should().Be(command.Species);
        savedAnimal.Breed.Should().Be(command.Breed);
        savedAnimal.Gender.Should().Be(command.Gender);
        savedAnimal.Size.Should().Be(command.Size);
        savedAnimal.BirthDate.Should().Be(command.BirthDate);
        savedAnimal.Description.Should().Be(command.Description);
        savedAnimal.IsVaccinated.Should().Be(command.IsVaccinated);
        savedAnimal.IsCastrated.Should().Be(command.IsCastrated);
        savedAnimal.Status.Should().Be(command.Status);
        savedAnimal.ShelterId.Should().Be(command.ShelterId);

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Animal>(),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}