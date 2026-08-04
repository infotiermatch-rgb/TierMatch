using FluentAssertions;
using Moq;

using TierMatch.Application.Animals.Commands.DeleteAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using TierMatch.Application.Authorization;

using Xunit;

namespace TierMatch.Application.Tests.Animals.Commands.DeleteAnimal;

public sealed class DeleteAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly DeleteAnimalHandler _handler;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;

  public DeleteAnimalHandlerTests()
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

    _handler = new DeleteAnimalHandler(
        _repositoryMock.Object,
        _unitOfWorkMock.Object,
        _currentUserServiceMock.Object);
}

    [Fact]
    public async Task Handle_Should_Delete_Existing_Animal()
    {
        // Arrange
        var animalId = Guid.NewGuid();

        var animal = new Animal
        {
            Id = animalId,
            Name = "Bello"
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

        var command = new DeleteAnimalCommand(animalId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()),
            Times.Once);

        _repositoryMock.Verify(
            repository => repository.Delete(animal),
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

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                animalId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var command = new DeleteAnimalCommand(animalId);

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
            repository => repository.Delete(
                It.IsAny<Animal>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }
}