using System.Data;
using Npgsql;
using Respawn;

namespace TierMatch.Api.Tests.Common;

/// <summary>
/// Verwaltet das Zurücksetzen der Testdatenbank mittels Respawn.
/// </summary>
public sealed class TestDatabase : IAsyncDisposable
{
    private readonly string _connectionString;

    private Respawner? _respawner;

    public TestDatabase(string connectionString)
    {
        _connectionString = connectionString;
    }

    public async Task InitializeAsync()
    {
        await using var connection = CreateConnection();

        await connection.OpenAsync();

        _respawner = await Respawner.CreateAsync(connection, new RespawnerOptions
        {
            DbAdapter = DbAdapter.Postgres,

            SchemasToInclude =
            [
                "public"
            ]
        });
    }

    public async Task ResetAsync()
    {
        if (_respawner is null)
        {
            throw new InvalidOperationException(
                "TestDatabase wurde nicht initialisiert. Rufe zuerst InitializeAsync() auf.");
        }

        await using var connection = CreateConnection();

        await connection.OpenAsync();

        await _respawner.ResetAsync(connection);
    }

    public NpgsqlConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }

    public ValueTask DisposeAsync()
    {
        return ValueTask.CompletedTask;
    }
}