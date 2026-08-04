using Microsoft.AspNetCore.Mvc.Testing;

using TierMatch.Application.Authorization;
using TierMatch.Domain.Entities;

using Xunit;

namespace TierMatch.Api.Tests.Common;

[Collection(TestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected HttpClient Client { get; }

    protected CustomWebApplicationFactory Factory { get; }

    protected IntegrationTestBase(
        PostgreSqlContainerFixture postgresFixture)
    {
        Factory = postgresFixture.Factory;

        Client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        AuthenticateAsAdmin();
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual Task DisposeAsync()
    {
        Client.Dispose();

        return Task.CompletedTask;
    }

    /// <summary>
    /// Authentifiziert den Standard-Testclient als Administrator.
    /// </summary>
    protected void AuthenticateAsAdmin(
        Guid? userId = null)
    {
        SetTestIdentity(
            Client,
            userId ?? Guid.NewGuid(),
            [Roles.Admin],
            shelterId: null);
    }

    /// <summary>
    /// Authentifiziert den Standard-Testclient als Tierheim-Administrator.
    /// </summary>
    protected void AuthenticateAsShelterAdmin(
        Guid shelterId,
        Guid? userId = null)
    {
        SetTestIdentity(
            Client,
            userId ?? Guid.NewGuid(),
            [Roles.ShelterAdmin],
            shelterId);
    }

    /// <summary>
    /// Authentifiziert den Standard-Testclient als normalen Benutzer.
    /// </summary>
    protected void AuthenticateAsUser(
        Guid? userId = null)
    {
        SetTestIdentity(
            Client,
            userId ?? Guid.NewGuid(),
            [Roles.User],
            shelterId: null);
    }

    /// <summary>
    /// Entfernt die Authentifizierung vom Standard-Testclient.
    /// </summary>
    protected void AuthenticateAsAnonymous()
    {
        RemoveTestIdentity(Client);
    }

    /// <summary>
    /// Erstellt einen nicht authentifizierten Client.
    /// </summary>
    protected HttpClient CreateAnonymousClient()
    {
        return Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });
    }

    /// <summary>
    /// Erstellt einen authentifizierten Administrator-Client.
    /// </summary>
    protected HttpClient CreateAdminClient(
        Guid? userId = null)
    {
        var client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        SetTestIdentity(
            client,
            userId ?? Guid.NewGuid(),
            [Roles.Admin],
            shelterId: null);

        return client;
    }

    /// <summary>
    /// Erstellt einen authentifizierten Tierheim-Administrator-Client.
    /// </summary>
    protected HttpClient CreateShelterAdminClient(
        Guid shelterId,
        Guid? userId = null)
    {
        var client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        SetTestIdentity(
            client,
            userId ?? Guid.NewGuid(),
            [Roles.ShelterAdmin],
            shelterId);

        return client;
    }

    /// <summary>
    /// Erstellt einen authentifizierten normalen Benutzer-Client.
    /// </summary>
    protected HttpClient CreateUserClient(
        Guid? userId = null)
    {
        var client = Factory.CreateClient(
            new WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        SetTestIdentity(
            client,
            userId ?? Guid.NewGuid(),
            [Roles.User],
            shelterId: null);

        return client;
    }

    /// <summary>
    /// Erstellt ein gültiges Tierheim für Integrationstests
    /// und gibt dessen ID zurück.
    /// </summary>
    protected async Task<Guid> CreateTestShelterAsync()
    {
        var shelter = new Shelter
        {
            Name = "Tierheim Zwickau",
            Street = "Teststraße",
            HouseNumber = "10",
            PostalCode = "08056",
            City = "Zwickau",
            Country = "DE",
            PhoneNumber = "+49 375 123456",
            Email = "tierheim@test.de",
            Website = "https://tierheim-test.de",
            Description =
                "Tierheim für TierMatch-Integrationstests."
        };

        await Factory.SeedAsync(dbContext =>
        {
            dbContext.Shelters.Add(shelter);

            return Task.CompletedTask;
        });

        return shelter.Id;
    }

    private static void SetTestIdentity(
        HttpClient client,
        Guid userId,
        IReadOnlyCollection<string> roles,
        Guid? shelterId)
    {
        RemoveTestIdentity(client);

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserIdHeader,
            userId.ToString());

        client.DefaultRequestHeaders.Add(
            TestAuthHandler.RolesHeader,
            string.Join(',', roles));

        if (shelterId.HasValue)
        {
            client.DefaultRequestHeaders.Add(
                TestAuthHandler.ShelterIdHeader,
                shelterId.Value.ToString());
        }
    }

    private static void RemoveTestIdentity(
        HttpClient client)
    {
        client.DefaultRequestHeaders.Remove(
            TestAuthHandler.UserIdHeader);

        client.DefaultRequestHeaders.Remove(
            TestAuthHandler.RolesHeader);

        client.DefaultRequestHeaders.Remove(
            TestAuthHandler.ShelterIdHeader);
    }
}