using Testcontainers.PostgreSql;
using Xunit;

namespace TierMatch.Api.Tests.Common;

/// <summary>
/// Verwaltet einen PostgreSQL-Testcontainer für alle Integrationstests.
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

    /// <summary>
    /// ConnectionString des laufenden Containers.
    /// </summary>
    public string ConnectionString => _container.GetConnectionString();

    /// <summary>
    /// Zugriff auf den Container (optional für Debugging).
    /// </summary>
    public PostgreSqlContainer Container => _container;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}