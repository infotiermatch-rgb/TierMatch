using Testcontainers.PostgreSql;

using Xunit;

namespace TierMatch.Api.Tests.Common;

/// <summary>
/// Verwaltet einen PostgreSQL-Testcontainer und eine gemeinsame
/// WebApplicationFactory für alle Integrationstests.
/// </summary>
public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _container;

    public PostgreSqlContainerFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:16-alpine")
            .WithDatabase("tiermatch_test")
            .WithUsername("postgres")
            .WithPassword("postgres")
            .WithCleanUp(true)
            .Build();
    }

    public string ConnectionString =>
        _container.GetConnectionString();

    public PostgreSqlContainer Container =>
        _container;

    public CustomWebApplicationFactory Factory { get; private set; } =
        null!;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        /*
         * Eine einzige Factory wird für die komplette
         * Integrationstest-Collection verwendet.
         */
        Factory = new CustomWebApplicationFactory(this);
    }

    public async Task DisposeAsync()
    {
        Factory?.Dispose();

        await _container.DisposeAsync();
    }
}