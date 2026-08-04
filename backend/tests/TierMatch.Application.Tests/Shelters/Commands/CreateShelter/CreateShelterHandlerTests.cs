using FluentAssertions;
using Moq;
using Xunit;

using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;
using TierMatch.Application.Shelters.Commands.CreateShelter;
using TierMatch.Domain.Entities;

namespace TierMatch.Application.Tests.Shelters.Commands.CreateShelter;

public sealed class CreateShelterHandlerTests
{
    private readonly Mock<IShelterRepository> _repositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly Mock<ICurrentUserService> _currentUserServiceMock;
    private readonly CreateShelterHandler _handler;

    public CreateShelterHandlerTests()
    {
        _repositoryMock = new Mock<IShelterRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();
        _currentUserServiceMock =
            new Mock<ICurrentUserService>();

        // Standardbenutzer für bestehende Erfolgstests:
        // angemeldeter Administrator.
        _currentUserServiceMock
            .SetupGet(service => service.IsAuthenticated)
            .Returns(true);

        _currentUserServiceMock
            .Setup(service => service.IsInRole(Roles.Admin))
            .Returns(true);

        _handler = new CreateShelterHandler(
            _repositoryMock.Object,
            _unitOfWorkMock.Object,
            _currentUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Shelter_When_User_Is_Admin()
    {
        // Arrange
        var command = CreateValidCommand();

        _repositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Shelter>(),
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _unitOfWorkMock
            .Setup(unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(1);

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeTrue();

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Shelter>(shelter =>
                    shelter.Name == command.Name &&
                    shelter.Street == command.Street &&
                    shelter.HouseNumber == command.HouseNumber &&
                    shelter.PostalCode == command.PostalCode &&
                    shelter.City == command.City &&
                    shelter.Country == command.Country &&
                    shelter.PhoneNumber == command.PhoneNumber &&
                    shelter.Email == command.Email &&
                    shelter.Website == command.Website &&
                    shelter.Description == command.Description),
                It.IsAny<CancellationToken>()),
            Times.Once);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Fail_When_User_Is_Anonymous()
    {
        // Arrange
        _currentUserServiceMock
            .SetupGet(service => service.IsAuthenticated)
            .Returns(false);

        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Shelter>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
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

        var command = CreateValidCommand();

        // Act
        var result = await _handler.Handle(
            command,
            CancellationToken.None);

        // Assert
        result.IsSuccess.Should().BeFalse();

        _repositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Shelter>(),
                It.IsAny<CancellationToken>()),
            Times.Never);

        _unitOfWorkMock.Verify(
            unitOfWork => unitOfWork.SaveChangesAsync(
                It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private static CreateShelterCommand CreateValidCommand()
    {
        return new CreateShelterCommand
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
            Description = "Ein Tierheim für Integrationstests."
        };
    }
}