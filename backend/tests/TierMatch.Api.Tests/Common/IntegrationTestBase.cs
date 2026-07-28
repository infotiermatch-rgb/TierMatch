using System.Net.Http;
using Xunit;

namespace TierMatch.Api.Tests.Common;

[Collection(TestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly HttpClient Client;
    protected readonly CustomWebApplicationFactory Factory;

    protected IntegrationTestBase(PostgreSqlContainerFixture postgres)
    {
        Factory = new CustomWebApplicationFactory(postgres);

        Client = Factory.CreateClient();
    }

    public virtual async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public virtual async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
    }
}