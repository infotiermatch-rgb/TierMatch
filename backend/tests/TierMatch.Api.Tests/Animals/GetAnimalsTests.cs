using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TierMatch.Api.Tests.Common;
using TierMatch.Application.Animals.DTOs;
using Xunit;

namespace TierMatch.Api.Tests.Animals;

public class GetAnimalsTests : IntegrationTestBase
{
    public GetAnimalsTests(CustomWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetAnimals_Should_Return_Empty_List_When_No_Animals_Exist()
    {
        // Act
        var response = await Client.GetAsync("/api/v1/animals");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var animals = await response.Content.ReadFromJsonAsync<List<AnimalDto>>();

        animals.Should().NotBeNull();
        animals.Should().BeEmpty();
    }

}