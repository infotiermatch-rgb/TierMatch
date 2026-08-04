using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using TierMatch.Infrastructure.Identity;

namespace TierMatch.Infrastructure.Authentication;

public static class IdentitySeeder
{
    public static async Task SeedAsync(
        IServiceProvider services,
        SeedOptions options)
    {
        using var scope = services.CreateScope();

        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

        var logger = scope.ServiceProvider
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger("IdentitySeeder");

        await SeedRolesAsync(
            roleManager,
            logger);

        await SeedAdminAsync(
            userManager,
            logger,
            options);
    }

    private static async Task SeedRolesAsync(
        RoleManager<IdentityRole<Guid>> roleManager,
        ILogger logger)
    {
        string[] roles =
        {
            "Admin",
            "ShelterAdmin",
            "User"
        };

        foreach (var role in roles)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                logger.LogDebug(
                    "Rolle '{Role}' existiert bereits.",
                    role);

                continue;
            }

            var result = await roleManager.CreateAsync(
                new IdentityRole<Guid>(role));

            if (result.Succeeded)
            {
                logger.LogInformation(
                    "Rolle '{Role}' wurde erstellt.",
                    role);
            }
            else
            {
                logger.LogError(
                    "Rolle '{Role}' konnte nicht erstellt werden: {Errors}",
                    role,
                    string.Join(", ",
                        result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedAdminAsync(
        UserManager<ApplicationUser> userManager,
        ILogger logger,
        SeedOptions options)
    {
        var admin = await userManager.FindByEmailAsync(
            options.AdminEmail);

        if (admin is not null)
        {
            logger.LogDebug(
                "Administrator existiert bereits.");

            return;
        }

        admin = new ApplicationUser
        {
            UserName = options.AdminEmail,
            Email = options.AdminEmail,

            FirstName = options.FirstName,
            LastName = options.LastName,

            EmailConfirmed = true,
            IsActive = true,

            CreatedAt = DateTime.UtcNow
        };

        var result = await userManager.CreateAsync(
            admin,
            options.AdminPassword);

        if (!result.Succeeded)
        {
            logger.LogError(
                "Administrator konnte nicht erstellt werden: {Errors}",
                string.Join(", ",
                    result.Errors.Select(e => e.Description)));

            return;
        }

        var roleResult = await userManager.AddToRoleAsync(
            admin,
            "Admin");

        if (!roleResult.Succeeded)
        {
            logger.LogError(
                "Administratorrolle konnte nicht vergeben werden: {Errors}",
                string.Join(", ",
                    roleResult.Errors.Select(e => e.Description)));

            return;
        }

        logger.LogInformation(
            "Administrator wurde erfolgreich erstellt.");
    }
}