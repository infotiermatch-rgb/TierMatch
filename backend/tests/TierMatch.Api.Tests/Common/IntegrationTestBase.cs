using System.Net.Http;

namespace TierMatch.Api.Tests.Common;

public abstract class IntegrationTestBase
    : IClassFixture<CustomWebApplicationFactory>
{
    protected readonly HttpClient Client;

    protected IntegrationTestBase(CustomWebApplicationFactory factory)
    {
        Client = factory.CreateClient();
    }
}