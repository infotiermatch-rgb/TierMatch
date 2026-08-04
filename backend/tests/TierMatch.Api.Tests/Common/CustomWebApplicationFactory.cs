using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

using TierMatch.Infrastructure.Data;

namespace TierMatch.Api.Tests.Common;

public sealed class CustomWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainerFixture _postgresFixture;

    public TestDatabase Database { get; private set; } = null!;

    public CustomWebApplicationFactory(
        PostgreSqlContainerFixture postgresFixture)
    {
        _postgresFixture = postgresFixture;
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] =
                        _postgresFixture.ConnectionString
                });
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseNpgsql(
                    _postgresFixture.ConnectionString);
            });

            using var provider =
                services.BuildServiceProvider();

            using var scope = provider.CreateScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<AppDbContext>();

            dbContext.Database.Migrate();

            Database = new TestDatabase(
                _postgresFixture.ConnectionString);

            Database.InitializeAsync()
                .GetAwaiter()
                .GetResult();
        });

        builder.ConfigureTestServices(services =>
        {
            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme =
                        TestAuthHandler.AuthenticationScheme;

                    options.DefaultChallengeScheme =
                        TestAuthHandler.AuthenticationScheme;

                    options.DefaultForbidScheme =
                        TestAuthHandler.AuthenticationScheme;
                })
                .AddScheme<
                    AuthenticationSchemeOptions,
                    TestAuthHandler>(
                    TestAuthHandler.AuthenticationScheme,
                    _ =>
                    {
                    });
        });
    }

    public async Task ResetDatabaseAsync()
    {
        await Database.ResetAsync();
    }

    public async Task SeedAsync(
        Func<AppDbContext, Task> seed)
    {
        await using var dbContext = CreateDbContext();

        await seed(dbContext);

        await dbContext.SaveChangesAsync();
    }

    public AppDbContext CreateDbContext()
    {
        var scope = Services.CreateScope();

        return scope.ServiceProvider
            .GetRequiredService<AppDbContext>();
    }
}