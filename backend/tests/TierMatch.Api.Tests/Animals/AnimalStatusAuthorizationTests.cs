using System.Net;
using System.Net.Http.Json;

using FluentAssertions;
using Xunit;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimalStatus;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Tests.Animals;

[Collection(TestCollection.Name)]
public sealed class AnimalStatusAuthorizationTests
    : IntegrationTestBase
{
    public AnimalStatusAuthorizationTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "AnonymousStatus");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für anonymen Statustest");

        AuthenticateAsAnonymous();

        var command = new UpdateAnimalStatusCommand(
            animalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{animalId}/status",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_Forbidden_For_Normal_User()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "UserStatus");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für Benutzer-Statustest");

        AuthenticateAsUser();

        var command = new UpdateAnimalStatusCommand(
            animalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{animalId}/status",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Update_Status_Of_Own_Animal()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "OwnStatus");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Eigenes Tier für Statustest");

        AuthenticateAsShelterAdmin(shelterId);

        var command = new UpdateAnimalStatusCommand(
            animalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{animalId}/status",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Update_Status_Of_Foreign_Animal()
    {
        // Arrange
        var ownShelterId = await CreateShelterAsync(
            "OwnForeignStatus");

        var foreignShelterId = await CreateShelterAsync(
            "ForeignStatus");

        var foreignAnimalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier für Statustest");

        AuthenticateAsShelterAdmin(ownShelterId);

        var command = new UpdateAnimalStatusCommand(
            foreignAnimalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{foreignAnimalId}/status",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_Should_Update_Status_Of_Any_Animal()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "AdminStatus");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für Admin-Statustest");

        AuthenticateAsAdmin();

        var command = new UpdateAnimalStatusCommand(
            animalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{animalId}/status",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_NotFound_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var missingAnimalId = Guid.NewGuid();

        AuthenticateAsAdmin();

        var command = new UpdateAnimalStatusCommand(
            missingAnimalId,
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{missingAnimalId}/status",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateStatus_Should_Return_BadRequest_When_Url_And_Command_Ids_Differ()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "DifferentStatusIds");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für unterschiedliche IDs");

        AuthenticateAsAdmin();

        var command = new UpdateAnimalStatusCommand(
            Guid.NewGuid(),
            AnimalStatus.Available);

        // Act
        using var response = await Client.PatchAsJsonAsync(
            $"/api/v1/animals/{animalId}/status",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.BadRequest);
    }

    private async Task<Guid> CreateAnimalAsAdminAsync(
        Guid shelterId,
        string name)
    {
        AuthenticateAsAdmin();

        var command = new CreateAnimalCommand
        {
            Name = name,
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description =
                "Tier für einen Status-Autorisierungs-Integrationstest.",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        using var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"API-Antwort: {responseContent}");

        var animalId =
            await response.Content.ReadFromJsonAsync<Guid>();

        animalId.Should().NotBe(Guid.Empty);

        return animalId;
    }

    private async Task<Guid> CreateShelterAsync(
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
                $"Tierheim für Status-Autorisierungstests: {suffix}."
        };

        await Factory.SeedAsync(dbContext =>
        {
            dbContext.Shelters.Add(shelter);

            return Task.CompletedTask;
        });

        return shelter.Id;
    }
}