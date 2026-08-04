using System.Net;
using System.Net.Http.Json;

using FluentAssertions;
using Xunit;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Tests.Animals;

[Collection(TestCollection.Name)]
public sealed class AnimalAuthorizationTests
    : IntegrationTestBase
{
    public AnimalAuthorizationTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task CreateAnimal_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var shelterId = await CreateShelterAsync("Anonymous");

        AuthenticateAsAnonymous();

        var command = CreateAnimalCommandFor(shelterId);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateAnimal_Should_Return_Forbidden_For_Normal_User()
    {
        // Arrange
        var shelterId = await CreateShelterAsync("User");

        AuthenticateAsUser();

        var command = CreateAnimalCommandFor(shelterId);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Create_Animal_For_Own_Shelter()
    {
        // Arrange
        var shelterId = await CreateShelterAsync("Own");

        AuthenticateAsShelterAdmin(shelterId);

        var command = CreateAnimalCommandFor(shelterId);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Created);

        var animalId =
            await response.Content.ReadFromJsonAsync<Guid>();

        animalId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Create_Animal_For_Foreign_Shelter()
    {
        // Arrange
        var ownShelterId =
            await CreateShelterAsync("Own");

        var foreignShelterId =
            await CreateShelterAsync("Foreign");

        AuthenticateAsShelterAdmin(ownShelterId);

        var command =
            CreateAnimalCommandFor(foreignShelterId);

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Update_Animal_From_Own_Shelter()
    {
        // Arrange
        var shelterId =
            await CreateShelterAsync("OwnUpdate");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Bello");

        AuthenticateAsShelterAdmin(shelterId);

        var command = CreateUpdateCommand(
            animalId,
            shelterId,
            "Bello aktualisiert");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{animalId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Update_Animal_From_Foreign_Shelter()
    {
        // Arrange
        var ownShelterId =
            await CreateShelterAsync("OwnUpdate");

        var foreignShelterId =
            await CreateShelterAsync("ForeignUpdate");

        var animalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier");

        AuthenticateAsShelterAdmin(ownShelterId);

        var command = CreateUpdateCommand(
            animalId,
            foreignShelterId,
            "Unzulässige Änderung");

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{animalId}",
            command);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);

        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{animalId}");

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Delete_Animal_From_Own_Shelter()
    {
        // Arrange
        var shelterId =
            await CreateShelterAsync("OwnDelete");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier zum Löschen");

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        var response = await Client.DeleteAsync(
            $"/api/v1/animals/{animalId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{animalId}");

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Delete_Animal_From_Foreign_Shelter()
    {
        // Arrange
        var ownShelterId =
            await CreateShelterAsync("OwnDelete");

        var foreignShelterId =
            await CreateShelterAsync("ForeignDelete");

        var animalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier");

        AuthenticateAsShelterAdmin(ownShelterId);

        // Act
        var response = await Client.DeleteAsync(
            $"/api/v1/animals/{animalId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);

        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{animalId}");

        getResponse.StatusCode.Should().Be(
            HttpStatusCode.OK);
    }

    private async Task<Guid> CreateAnimalAsAdminAsync(
        Guid shelterId,
        string name)
    {
        AuthenticateAsAdmin();

        var command = CreateAnimalCommandFor(
            shelterId,
            name);

        var response = await Client.PostAsJsonAsync(
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
                $"Tierheim für Autorisierungstests: {suffix}."
        };

        await Factory.SeedAsync(dbContext =>
        {
            dbContext.Shelters.Add(shelter);

            return Task.CompletedTask;
        });

        return shelter.Id;
    }

    private static CreateAnimalCommand CreateAnimalCommandFor(
        Guid shelterId,
        string name = "Bello")
    {
        return new CreateAnimalCommand
        {
            Name = name,
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description =
                "Tier für einen Autorisierungs-Integrationstest.",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };
    }

    private static UpdateAnimalCommand CreateUpdateCommand(
        Guid animalId,
        Guid shelterId,
        string name)
    {
        return new UpdateAnimalCommand
        {
            Id = animalId,
            Name = name,
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2022, 5, 10),
            Description =
                "Aktualisiertes Tier aus einem Autorisierungstest.",
            IsVaccinated = true,
            IsCastrated = true,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };
    }
}