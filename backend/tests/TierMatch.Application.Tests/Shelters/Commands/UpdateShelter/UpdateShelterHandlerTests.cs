using FluentAssertions;
using Moq;
using Xunit;

using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Commands.UpdateShelter;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Tests.Shelters.Commands.UpdateShelter;

public sealed class UpdateShelterHandlerTests
{
    private readonly Mock<IShelterRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly UpdateShelterHandler _handler;

    public UpdateShelterHandlerTests()
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

        _handler = new UpdateShelterHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Update_Shelter_When_User_Is_Admin()
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

        var command = CreateValidCommand(
            shelterId,
            "Aktualisiertes Tierheim");

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        shelter.Name.Should().Be(command.Name);
        shelter.Street.Should().Be(command.Street);
        shelter.HouseNumber.Should().Be(command.HouseNumber);
        shelter.PostalCode.Should().Be(command.PostalCode);
        shelter.City.Should().Be(command.City);
        shelter.Country.Should().Be(command.Country);
        shelter.PhoneNumber.Should().Be(command.PhoneNumber);
        shelter.Email.Should().Be(command.Email);
        shelter.Website.Should().Be(command.Website);
        shelter.Description.Should().Be(command.Description);

        _repositoryMock.Verify(
            repository => repository.Update(shelter),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Update_Own_Shelter_When_User_Is_ShelterAdmin()
    {
        // Arrange
        var shelterId = Guid.NewGuid();
        var shelter = CreateShelter();

        ConfigureAsShelterAdmin(shelterId);

        _repositoryMock
            .Setup(repository => repository.GetByIdAsync(
                shelterId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(shelter);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        var command = CreateValidCommand(
            shelterId,
            "Eigenes aktualisiertes Tierheim");

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();
        shelter.Name.Should().Be(command.Name);

        _repositoryMock.Verify(
            repository => repository.Update(shelter),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_ShelterAdmin_Updates_Foreign_Shelter()
    {
        // Arrange
        var ownShelterId = Guid.NewGuid();
        var foreignShelterId = Guid.NewGuid();

        ConfigureAsShelterAdmin(ownShelterId);

        var command = CreateValidCommand(
            foreignShelterId,
            "Fremdes Tierheim");

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
            repository => repository.Update(
                It.IsAny<Shelter>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_ShelterAdmin_Has_No_ShelterId()
    {
        // Arrange
        ConfigureAsShelterAdmin(null);

        var command = CreateValidCommand(
            Guid.NewGuid(),
            "Tierheim ohne Claim");

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
            repository => repository.Update(
                It.IsAny<Shelter>()),
            Times.Never);
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

        var command = CreateValidCommand(
            shelterId,
            "Nicht vorhandenes Tierheim");

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.Update(
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

        var command = CreateValidCommand(
            Guid.NewGuid(),
            "Anonymer Zugriff");

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
            repository => repository.Update(
                It.IsAny<Shelter>()),
            Times.Never);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Has_No_Allowed_Role()
    {
        // Arrange
        _currentUserServiceMock
            .Setup(service => service.IsInRole(Roles.Admin))
            .Returns(false);

        _currentUserServiceMock
            .Setup(service =>
                service.IsInRole(Roles.ShelterAdmin))
            .Returns(false);

        var command = CreateValidCommand(
            Guid.NewGuid(),
            "Unberechtigter Zugriff");

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
            repository => repository.Update(
                It.IsAny<Shelter>()),
            Times.Never);
    }

    private void ConfigureAsShelterAdmin(
        Guid? shelterId)
    {
        _currentUserServiceMock
            .Setup(service => service.IsInRole(Roles.Admin))
            .Returns(false);

        _currentUserServiceMock
            .Setup(service =>
                service.IsInRole(Roles.ShelterAdmin))
            .Returns(true);

        _currentUserServiceMock
            .SetupGet(service => service.ShelterId)
            .Returns(shelterId);
    }

    private static Shelter CreateShelter()
    {
        return new Shelter
        {
            Name = "Altes Tierheim",
            Street = "Alte Straße",
            HouseNumber = "1",
            PostalCode = "08056",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 111111",
            Email = "alt@tierheim.de",
            Website = "https://alt-tierheim.de",
            Description = "Alter Tierheim-Datensatz."
        };
    }

    private static UpdateShelterCommand CreateValidCommand(
        Guid shelterId,
        string name)
    {
        return new UpdateShelterCommand
        {
            Id = shelterId,
            Name = name,
            Street = "Neue Straße",
            HouseNumber = "25",
            PostalCode = "08058",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 987654",
            Email = "neu@tierheim.de",
            Website = "https://neu-tierheim.de",
            Description = "Aktualisierte Tierheimbeschreibung."
        };
    }
}