using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Api.Tests.Animals;

public class DeleteAnimalTests : IntegrationTestBase
{
    public DeleteAnimalTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task DeleteAnimal_Should_Delete_Existing_Animal()
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

        // Act
        var deleteResponse = await Client.DeleteAsync(
            $"/api/v1/animals/{id}");

        // Assert
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Prüfen, dass das Tier wirklich gelöscht wurde
        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{id}");

        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAnimal_Should_Return_NotFound_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.DeleteAsync(
            $"/api/v1/animals/{id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}