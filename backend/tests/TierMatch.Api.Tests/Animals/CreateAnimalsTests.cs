using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.Commands.CreateAnimal;
using TierMatch.Application.Animals.DTOs;
using TierMatch.Domain.Enums;
using Xunit;

namespace TierMatch.Api.Tests.Animals;

public class CreateAnimalTests : IntegrationTestBase
{
    public CreateAnimalTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task CreateAnimal_Should_Create_Animal_And_Return_It()
    {
        // Arrange
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
            IsCastrated = false
        };

        // Act 1 - Animal erstellen
        var createResponse = await Client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        // GUID auslesen
        var id = await createResponse.Content.ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        // Act 2 - Tier wieder abrufen
        var getResponse = await Client.GetAsync($"/api/v1/animals/{id}");

        // Assert
        getResponse.StatusCode.Should().Be(HttpStatusCode.OK);

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
        IsCastrated = false
    };

    // Act
    var response = await Client.PostAsJsonAsync(
        "/api/v1/animals",
        command);

// Assert
response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

var content = await response.Content.ReadAsStringAsync();
Console.WriteLine(content);

// Den Assert aktivieren wir, sobald wir das tatsächliche JSON kennen.
// content.Should().Contain("Name");
}
}