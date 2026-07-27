using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using TierMatch.Api.Tests.Builders;
using TierMatch.Application.Animals.Commands.CreateAnimal;

namespace TierMatch.Api.Tests.Helpers;

public static class AnimalTestFactory
{
    public static async Task<Guid> CreateAnimalAsync(HttpClient client)
    {
        return await CreateAnimalAsync(
            client,
            AnimalBuilder
                .Create()
                .Build());
    }

    public static async Task<Guid> CreateAnimalAsync(
        HttpClient client,
        CreateAnimalCommand command)
    {
        var response = await client.PostAsJsonAsync(
            "/api/v1/animals",
            command);

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var id = await response.Content.ReadFromJsonAsync<Guid>();

        id.Should().NotBe(Guid.Empty);

        return id;
    }
}