using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using FluentAssertions;
using Xunit;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Domain.Entities;
using TierMatch.Domain.Enums;

namespace TierMatch.Api.Tests.Animals;

[Collection(TestCollection.Name)]
public sealed class AnimalImageAuthorizationTests
    : IntegrationTestBase
{
    private static readonly byte[] TestImageBytes =
        Convert.FromBase64String(
            "iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAQAAAC1HAwCAAAAC0lEQVR42mP8/x8AAusB9Y9Zl0sAAAAASUVORK5CYII=");

    public AnimalImageAuthorizationTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task UploadImage_Should_Return_Unauthorized_When_User_Is_Anonymous()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "AnonymousUpload");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für anonymen Upload");

        AuthenticateAsAnonymous();

        // Act
        using var response = await UploadImageAsync(
            animalId,
            $"anonymous-{Guid.NewGuid():N}.png");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UploadImage_Should_Return_Forbidden_For_Normal_User()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "UserUpload");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Tier für Benutzer-Upload");

        AuthenticateAsUser();

        // Act
        using var response = await UploadImageAsync(
            animalId,
            $"user-{Guid.NewGuid():N}.png");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Upload_Image_For_Own_Animal()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "OwnUpload");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Eigenes Tier mit Bild");

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        using var response = await UploadImageAsync(
            animalId,
            $"own-upload-{Guid.NewGuid():N}.png");

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var imageId =
            await response.Content.ReadFromJsonAsync<Guid>();

        imageId.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Upload_Image_For_Foreign_Animal()
    {
        // Arrange
        var ownShelterId = await CreateShelterAsync(
            "OwnForeignUpload");

        var foreignShelterId = await CreateShelterAsync(
            "ForeignUpload");

        var foreignAnimalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier für Upload");

        AuthenticateAsShelterAdmin(ownShelterId);

        // Act
        using var response = await UploadImageAsync(
            foreignAnimalId,
            $"foreign-upload-{Guid.NewGuid():N}.png");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Set_Primary_Image_For_Own_Animal()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "OwnPrimary");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Eigenes Tier mit mehreren Bildern");

        _ = await UploadImageAsAdminAsync(
            animalId,
            $"primary-first-{Guid.NewGuid():N}.png");

        var secondImageId = await UploadImageAsAdminAsync(
            animalId,
            $"primary-second-{Guid.NewGuid():N}.png");

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        using var response = await Client.PatchAsync(
            $"/api/v1/animals/{animalId}/images/{secondImageId}/primary",
            content: null);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Set_Primary_Image_For_Foreign_Animal()
    {
        // Arrange
        var ownShelterId = await CreateShelterAsync(
            "OwnForeignPrimary");

        var foreignShelterId = await CreateShelterAsync(
            "ForeignPrimary");

        var foreignAnimalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier für Hauptbild");

        var foreignImageId = await UploadImageAsAdminAsync(
            foreignAnimalId,
            $"foreign-primary-{Guid.NewGuid():N}.png");

        AuthenticateAsShelterAdmin(ownShelterId);

        // Act
        using var response = await Client.PatchAsync(
            $"/api/v1/animals/{foreignAnimalId}/images/{foreignImageId}/primary",
            content: null);

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Delete_Image_From_Own_Animal()
    {
        // Arrange
        var shelterId = await CreateShelterAsync(
            "OwnDeleteImage");

        var animalId = await CreateAnimalAsAdminAsync(
            shelterId,
            "Eigenes Tier für Bildlöschung");

        var imageId = await UploadImageAsAdminAsync(
            animalId,
            $"own-delete-{Guid.NewGuid():N}.png");

        AuthenticateAsShelterAdmin(shelterId);

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/animals/{animalId}/images/{imageId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task ShelterAdmin_Should_Not_Delete_Image_From_Foreign_Animal()
    {
        // Arrange
        var ownShelterId = await CreateShelterAsync(
            "OwnForeignDeleteImage");

        var foreignShelterId = await CreateShelterAsync(
            "ForeignDeleteImage");

        var foreignAnimalId = await CreateAnimalAsAdminAsync(
            foreignShelterId,
            "Fremdes Tier für Bildlöschung");

        var foreignImageId = await UploadImageAsAdminAsync(
            foreignAnimalId,
            $"foreign-delete-{Guid.NewGuid():N}.png");

        AuthenticateAsShelterAdmin(ownShelterId);

        // Act
        using var response = await Client.DeleteAsync(
            $"/api/v1/animals/{foreignAnimalId}/images/{foreignImageId}");

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.Forbidden);
    }

    private async Task<Guid> UploadImageAsAdminAsync(
        Guid animalId,
        string fileName)
    {
        AuthenticateAsAdmin();

        using var response = await UploadImageAsync(
            animalId,
            fileName);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            $"API-Antwort: {responseContent}");

        var imageId =
            await response.Content.ReadFromJsonAsync<Guid>();

        imageId.Should().NotBe(Guid.Empty);

        return imageId;
    }

    private async Task<HttpResponseMessage> UploadImageAsync(
        Guid animalId,
        string fileName)
    {
        using var multipartContent =
            new MultipartFormDataContent();

        var fileContent =
            new ByteArrayContent(TestImageBytes);

        fileContent.Headers.ContentType =
            new MediaTypeHeaderValue("image/png");

        multipartContent.Add(
            fileContent,
            "file",
            fileName);

        return await Client.PostAsync(
            $"/api/v1/animals/{animalId}/images",
            multipartContent);
    }

    private async Task<Guid> CreateAnimalAsAdminAsync(
        Guid shelterId,
        string name)
    {
        AuthenticateAsAdmin();

        var command = CreateAnimalCommandFor(
            shelterId,
            name);

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
                $"Tierheim für Bild-Autorisierungstests: {suffix}."
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
        string name)
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
                "Tier für einen Bild-Autorisierungs-Integrationstest.",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };
    }
}