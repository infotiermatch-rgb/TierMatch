using FluentAssertions;
using Moq;
using Xunit;

using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Commands.DeleteShelter;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Tests.Shelters.Commands.DeleteShelter;

public sealed class DeleteShelterHandlerTests
{
    private readonly Mock<IShelterRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly DeleteShelterHandler _handler;

    public DeleteShelterHandlerTests()
    {
        _repositoryMock = new Mock<IShelterRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        // Standardbenutzer ist ein angemeldeter Administrator.
        _currentUserServiceMock
            .SetupGet(service => service.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsInRole(Roles.Admin))
            .Returns(true);

        _handler = new DeleteShelterHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Delete_Shelter_When_User_Is_Admin()
    {
        // Arrange
        var shelterId = Guid.NewGuid();

        var shelter = CreateShelter();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                shelterId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shelter);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = new DeleteShelterCommand(shelterId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(
            repository => repository.Delete(shelter),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Return_Failure_When_Shelter_Does_Not_Exist()
    {
        // Arrange
        var shelterId = Guid.NewGuid();

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                shelterId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Shelter?)null);

        var command = new DeleteShelterCommand(shelterId);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.Delete(
                It.IsAny<Shelter>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Anonymous()
    {
        // Arrange
        _currentUserServiceMock
            .SetupGet(service => service.IsAuthenticated)
            .Returns(false);

        var command = new DeleteShelterCommand(
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.Delete(
                It.IsAny<Shelter>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_ShelterAdmin()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.IsInRole(Roles.Admin))
            .Returns(false);

        _currentUserServiceMock
            .Setup(service =>
                service.IsInRole(Roles.ShelterAdmin))
            .Returns(true);

        var command = new DeleteShelterCommand(
            Guid.NewGuid());

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.GetByIdAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _repositoryMock.Verify(
            repository => repository.Delete(
                It.IsAny<Shelter>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static Shelter CreateShelter()
    {
        return new Shelter
        {
            Name = "Tierheim Zwickau",
            Street = "Teststraße",
            HouseNumber = "10",
            PostalCode = "08056",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 123456",
            Email = "kontakt@tierheim-zwickau.de",
            Website = "https://tierheim-zwickau.de",
            Description = "Tierheim für einen Unit-Test."
        };
    }
}