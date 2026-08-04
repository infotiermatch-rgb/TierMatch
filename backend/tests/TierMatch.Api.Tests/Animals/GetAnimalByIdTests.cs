using System.Net;
using System.Net.Http.Json;

using FluentAssertions;

using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Enums;

using Xunit;

namespace TierMatch.Api.Tests.Animals;

public sealed class GetAnimalByIdTests : IntegrationTestBase
{
    public GetAnimalByIdTests(
        PostgreSqlContainerFixture postgresFixture)
        : base(postgresFixture)
    {
    }

    [Fact]
    public async Task GetAnimalById_Should_Return_Animal_When_It_Exists()
    {
        // Arrange
        var shelterId = await CreateTestShelterAsync();

        var command = new CreateAnimalCommand
        {
            Name = "Luna",
            Species = AnimalSpecies.Cat,
            Breed = "Maine Coon",
            Gender = AnimalGender.Female,
            Size = AnimalSize.Medium,
            BirthDate = new DateOnly(2021, 8, 15),
            Description = "Very friendly",
            IsVaccinated = true,
            IsCastrated = true,
            Status = AnimalStatus.Available,
            ShelterId = shelterId
        };

        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        var createResponseContent =
            await createResponse.Content.ReadAsStringAsync();

        createResponse.StatusCode.Should().Be(
            HttpStatusCode.Created,
            $"API-Antwort: {createResponseContent}");

        var id = await createResponse.Content
            .ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        // Act
        var response = await Client.GetAsync(
            $"/api/v1/animals/{id}");

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.OK);

        var animal = await response.Content
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
        animal.IsVaccinated.Should().BeTrue();
        animal.IsCastrated.Should().BeTrue();
    }

    [Fact]
    public async Task GetAnimalById_Should_Return_NotFound_When_Animal_Does_Not_Exist()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync(
            $"/api/v1/animals/{id}");

        // Assert
        response.StatusCode.Should()
            .Be(HttpStatusCode.NotFound);
    }
}