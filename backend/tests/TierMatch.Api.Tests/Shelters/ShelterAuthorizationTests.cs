using System.Net;
using System.Net.Http.Json;

using FluentAssertions;
using Xunit;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Shelters.Commands.CreateShelter;
using TierMatch.Application.Shelters.Commands.UpdateShelter;
using TierMatch.Domain.Entities;

namespace TierMatch.Api.Tests.Shelters;

[Collection(TestCollection.Name)]
public sealed class ShelterAuthorizationTests
    : IntegrationTestBase
{
    public ShelterAuthorizationTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task Create_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        AuthenticateAsAnonymous();

        var command = CreateValidCreateCommand(
            "Anonymes Tierheim");

        // Act
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/shelters",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Create_Should_Return_Forbidden_When_User_Is_Normal_User()
    {
        // Arrange
        AuthenticateAsUser();

        var command = CreateValidCreateCommand(
            "Tierheim eines Benutzers");

        // Act
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/shelters",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Create_Should_Return_Forbidden_When_User_Is_ShelterAdmin()
    {
        // Arrange
        var existingShelterId =
            await SeedShelterAsync("ShelterAdminCreate");

        AuthenticateAsShelterAdmin(existingShelterId);

        var command = CreateValidCreateCommand(
            "Unerlaubtes neues Tierheim");

        // Act
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/shelters",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Create_Shelter()
    {
        // Arrange
        AuthenticateAsAdmin();

        var command = CreateValidCreateCommand(
            "Neues Admin-Tierheim");

        // Act
        using var response = await Client.PostAsJsonAsync(
            "/api/v1/shelters",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var shelterId =
            await response.Content.ReadFromJsonAsync<Guid>();

        shelterId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task Update_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("AnonymousUpdate");

        AuthenticateAsAnonymous();

        var command = CreateValidUpdateCommand(
            shelterId,
            "Anonym aktualisiertes Tierheim");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{shelterId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Update_Should_Return_Forbidden_When_User_Is_Normal_User()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("UserUpdate");

        AuthenticateAsUser();

        var command = CreateValidUpdateCommand(
            shelterId,
            "Vom Benutzer aktualisiertes Tierheim");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{shelterId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Update_Own_Shelter()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("OwnUpdate");

        AuthenticateAsShelterAdmin(shelterId);

        var command = CreateValidUpdateCommand(
            shelterId,
            "Eigenes aktualisiertes Tierheim");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{shelterId}",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Update_Foreign_Shelter()
    {
        // Arrange
        var ownShelterId =
            await SeedShelterAsync("OwnForeignUpdate");

        var foreignShelterId =
            await SeedShelterAsync("ForeignUpdate");

        AuthenticateAsShelterAdmin(ownShelterId);

        var command = CreateValidUpdateCommand(
            foreignShelterId,
            "Unerlaubt aktualisiertes Tierheim");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{foreignShelterId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Update_Any_Shelter()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("AdminUpdate");

        AuthenticateAsAdmin();

        var command = CreateValidUpdateCommand(
            shelterId,
            "Vom Admin aktualisiertes Tierheim");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{shelterId}",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task Update_Should_Return_BadRequest_When_Url_And_Command_Ids_Differ()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("DifferentIds");

        AuthenticateAsAdmin();

        var command = CreateValidUpdateCommand(
            Guid.NewGuid(),
            "Tierheim mit abweichender ID");

        // Act
        using var response = await Client.PutAsJsonAsync(
            $"/api/v1/shelters/{shelterId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Delete_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("AnonymousDelete");

        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/shelters/{shelterId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Delete_Shelter()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("ShelterAdminDelete");

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/shelters/{shelterId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Delete_Shelter()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("AdminDelete");

        AuthenticateAsAdmin();

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/shelters/{shelterId}");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        using var getResponse = await Client.GetAsync(
            $"/api/v1/shelters/{shelterId}");

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAll_Should_Be_Accessible_Without_Authentication()
    {
        // Arrange
        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.GetAsync(
            "/api/v1/shelters");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetById_Should_Be_Accessible_Without_Authentication()
    {
        // Arrange
        var shelterId =
            await SeedShelterAsync("PublicGetById");

        AuthenticateAsAnonymous();

        // Act
        using var response = await Client.GetAsync(
            $"/api/v1/shelters/{shelterId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    private async Task<Guid> SeedShelterAsync(
        string suffix)
    {
        var shelter = new Shelter
        {
            Name = $"Tierheim {suffix}",
            Street = "Teststraße",
            HouseNumber = "10",
            PostalCode = "08056",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber =
                $"+49 375 {Random.Shared.Next(100000, 999999)}",
            Email =
                $"tierheim-{Guid.NewGuid():N}@test.de",
            Website =
                $"https://tierheim-{Guid.NewGuid():N}.test",
            Description =
                $"Tierheim für Autorisierungstests: {suffix}."
        };

        await Factory.SeedAsync(dbContext =>
        {
            dbContext.Shelters.Add(shelter);

            return Task.CompletedTask;
        });

        return shelter.Id;
    }

    private static CreateShelterCommand CreateValidCreateCommand(
        string name)
    {
        var uniqueValue = Guid.NewGuid().ToString("N");

        return new CreateShelterCommand
        {
            Name = name,
            Street = "Neue Straße",
            HouseNumber = "25",
            PostalCode = "08058",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 987654",
            Email = $"neu-{uniqueValue}@tierheim.test",
            Website = $"https://tierheim-{uniqueValue}.test",
            Description =
                "Neu angelegtes Tierheim aus einem Autorisierungstest."
        };
    }

    private static UpdateShelterCommand CreateValidUpdateCommand(
        Guid shelterId,
        string name)
    {
        var uniqueValue = Guid.NewGuid().ToString("N");

        return new UpdateShelterCommand
        {
            Id = shelterId,
            Name = name,
            Street = "Aktualisierte Straße",
            HouseNumber = "30",
            PostalCode = "08060",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 654321",
            Email =
                $"aktualisiert-{uniqueValue}@tierheim.test",
            Website =
                $"https://aktualisiert-{uniqueValue}.test",
            Description =
                "Aktualisiertes Tierheim aus einem Autorisierungstest."
        };
    }
}