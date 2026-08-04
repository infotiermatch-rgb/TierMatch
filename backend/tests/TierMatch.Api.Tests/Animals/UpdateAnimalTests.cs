using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.Commands.UpdateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Api.Tests.Animals;

public sealed class UpdateAnimalTests : IntegrationTestBase
{
    public UpdateAnimalTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task UpdateAnimal_Should_Update_Existing_Animal()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();

        var createCommand = new CreateAnimalCommand
        {
            Name = "Bello",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description = "Friendly dog",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            createCommand);

        var createResponseContent =
            await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"API-Antwort: {createResponseContent}");

        var id = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        var updateCommand = new UpdateAnimalCommand
        {
            Id = id,
            Name = "Rocky",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2021, 3, 20),
            Description = "Updated description",
            IsVaccinated = false,
            IsCastrated = true,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{id}",
            updateCommand);

        // Assert
        var updateResponseContent =
            await updateResponse.Content.ReadAsStringAsync();

        updateResponse.StatusCode.Should().Be(
            HttpStatusCode.NoContent,
            $"API-Antwort: {updateResponseContent}");

        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{id}");

        getResponse.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var animal = await getResponse.Content
            .ReadFromJsonAsync<AnimalDto>();

        animal.Should().NotBeNull();

        animal!.Id.Should().Be(id);
        animal.Name.Should().Be("Rocky");

        animal.Species.Should()
            .Be(AnimalSpecies.Dog.ToString());

        animal.Breed.Should()
            .Be("Golden Retriever");

        animal.Gender.Should()
            .Be(AnimalGender.Male.ToString());

        animal.Size.Should()
            .Be(AnimalSize.Large.ToString());

        animal.BirthDate.Should()
            .Be(new DateOnly(2021, 3, 20));

        animal.Description.Should()
            .Be("Updated description");

        animal.IsVaccinated.Should().BeFalse();
        animal.IsCastrated.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAnimal_Should_Return_NotFound_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();
        var id = Guid.NewGuid();

        var command = new UpdateAnimalCommand
        {
            Id = id,
            Name = "Rocky",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2021, 3, 20),
            Description = "Updated description",
            IsVaccinated = true,
            IsCastrated = true,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{id}",
            command);

        // Assert
        var responseContent =
            await response.Content.ReadAsStringAsync();

        response.StatusCode.Should().Be(
            HttpStatusCode.NotFound,
            $"API-Antwort: {responseContent}");
    }

    [Fact]
    public async Task UpdateAnimal_Should_Return_BadRequest_When_Ids_Do_Not_Match()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();
        var routeId = Guid.NewGuid();

        var command = new UpdateAnimalCommand
        {
            Id = Guid.NewGuid(),
            Name = "Rocky",
            Species = AnimalSpecies.Dog,
            Breed = "Golden Retriever",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Large,
            BirthDate = new DateOnly(2021, 3, 20),
            Description = "Updated description",
            IsVaccinated = true,
            IsCastrated = true,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{routeId}",
            command);

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);
    }
}