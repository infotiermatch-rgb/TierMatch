using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using TierMatch.Infrastructure.Data;

namespace TierMatch.Api.Tests.Common;

public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainerFixture _postgresFixture;

    public TestDatabase Database { get; private set; } = null!;

    public CustomWebApplicationFactory(PostgreSqlContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = _postgresFixture.ConnectionString
            });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(_postgresFixture.ConnectionString);
            });

            using var provider = services.BuildServiceProvider();

            using var scope = provider.CreateScope();

            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            db.Database.Migrate();

            Database = new TestDatabase(_postgresFixture.ConnectionString);

            Database.InitializeAsync()
                .GetAwaiter()
                .GetResult();
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await Database.ResetAsync();
    }

    public async Task SeedAsync(Func<AppDbContext, Task> seed)
    {
        await using var db = CreateDbContext();

        await seed(db);

        await db.SaveChangesAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();

        return scope.ServiceProvider.GetRequiredService<AppDbContext>();
    }
}