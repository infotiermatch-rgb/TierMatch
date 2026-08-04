using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Api.Tests.Animals;

public sealed class CreateAnimalTests : IntegrationTestBase
{
    public CreateAnimalTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task CreateAnimal_Should_Create_Animal_And_Return_It()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();

        var command = new CreateAnimalCommand
        {
            Name = "Bello",
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description = "Friendly family dog",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        var createResponseContent =
            await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"API-Antwort: {createResponseContent}");

        var id = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        var getResponse = await Client.GetAsync(
            $"/api/v1/animals/{id}");

        getResponse.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var animal = await getResponse.Content
            .ReadFromJsonAsync<AnimalDto>();

        animal.Should().NotBeNull();

        animal!.Id.Should().Be(id);
        animal.Name.Should().Be(command.Name);
        animal.Species.Should().Be(command.Species.ToString());
        animal.Breed.Should().Be(command.Breed);
        animal.Gender.Should().Be(command.Gender.ToString());
        animal.Size.Should().Be(command.Size.ToString());
        animal.BirthDate.Should().Be(command.BirthDate);
        animal.Description.Should().Be(command.Description);
        animal.IsVaccinated.Should().Be(command.IsVaccinated);
        animal.IsCastrated.Should().Be(command.IsCastrated);
    }

    [Fact]
    public async Task CreateAnimal_Should_Return_BadRequest_When_Name_Is_Empty()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();

        var command = new CreateAnimalCommand
        {
            Name = string.Empty,
            Species = AnimalSpecies.Dog,
            Breed = "Labrador",
            Gender = AnimalGender.Male,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2022, 5, 10),
            Description = "Friendly family dog",
            IsVaccinated = true,
            IsCastrated = false,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        // Act
        var response = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.BadRequest);

        var responseContent =
            await response.Content.ReadAsStringAsync();

        responseContent.Should()
            .Contain("Der Name ist erforderlich.");
    }
}