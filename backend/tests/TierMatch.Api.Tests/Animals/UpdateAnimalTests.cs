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

public class UpdateAnimalTests : IntegrationTestBase
{
    public UpdateAnimalTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task UpdateAnimal_Should_Update_Existing_Animal()
    {
        // Arrange
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
            IsCastrated = false
        };

        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            createCommand);

        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        var updateCommand = new UpdateAnimalCommand(
            id,
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            false,
            true);

        // Act
        var updateResponse = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{id}",
            updateCommand);

        // Assert
        updateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        var getResponse = await Client.GetAsync($"/api/v1/animals/{id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var animal = await getResponse.Content.ReadFromJsonAsync<AnimalDto>();

        animal.Should().NotBeNull();

        animal!.Id.Should().Be(id);
        animal.Name.Should().Be("Rocky");
        animal.Species.Should().Be(AnimalSpecies.Dog.ToString());
        animal.Breed.Should().Be("Golden Retriever");
        animal.Gender.Should().Be(AnimalGender.Male.ToString());
        animal.Size.Should().Be(AnimalSize.Large.ToString());
        animal.BirthDate.Should().Be(new DateOnly(2021, 3, 20));
        animal.Description.Should().Be("Updated description");
        animal.IsVaccinated.Should().BeFalse();
        animal.IsCastrated.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateAnimal_Should_Return_NotFound_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        var command = new UpdateAnimalCommand(
            id,
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            true,
            true);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{id}",
            command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateAnimal_Should_Return_BadRequest_When_Ids_Do_Not_Match()
    {
        // Arrange
        var routeId = Guid.NewGuid();

        var command = new UpdateAnimalCommand(
            Guid.NewGuid(),
            "Rocky",
            AnimalSpecies.Dog,
            "Golden Retriever",
            AnimalGender.Male,
            AnimalSize.Large,
            new DateOnly(2021, 3, 20),
            "Updated description",
            true,
            true);

        // Act
        var response = await Client.PutAsJsonAsync(
            $"/api/v1/animals/{routeId}",
            command);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}