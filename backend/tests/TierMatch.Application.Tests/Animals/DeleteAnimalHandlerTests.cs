using FluentAssertions;
using Moq;
using TierMatch.Application.Animals.Commands.DeleteAnimal;
using TierMatch.Application.Interfaces;
using TierMatch.Domain.Entities;
using Xunit;

namespace TierMatch.Application.Tests.Animals;

public class DeleteAnimalHandlerTests
{
    private readonly Mock<IAnimalRepository> _repositoryMock;
    private readonly DeleteAnimalHandler _handler;

    public DeleteAnimalHandlerTests()
    {
        _repositoryMock = new Mock<IAnimalRepository>();

        _handler = new DeleteAnimalHandler(
            _repositoryMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Animal_And_Return_True()
    {
        // Arrange
        var id = Guid.NewGuid();

        var animal = new Animal
        {
            Id = id,
            Name = "Bello"
        };

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(animal);

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteAnimalCommand(id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().BeTrue();

        _repositoryMock.Verify(
            r => r.Delete(animal),
            Times.Once);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_False_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Animal?)null);

        var command = new DeleteAnimalCommand(id);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.Should().BeFalse();

        _repositoryMock.Verify(
            r => r.Delete(It.IsAny<Animal>()),
            Times.Never);

        _repositoryMock.Verify(
            r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Call_GetById_Exactly_Once()
    {
        // Arrange
        var id = Guid.NewGuid();

        _repositoryMock
            .Setup(r => r.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Animal());

        _repositoryMock
            .Setup(r => r.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteAnimalCommand(id);

        // Act
        await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        _repositoryMock.Verify(
            r => r.GetByIdAsync(
                id,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}