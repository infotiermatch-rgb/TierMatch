using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TierMatch.Application.Authentication.Interfaces;
using TierMatch.Application.Interfaces;
using TierMatch.Infrastructure.Authentication;
using TierMatch.Infrastructure.Data;
using TierMatch.Infrastructure.Identity;
using TierMatch.Infrastructure.Repositories;
using TierMatch.Infrastructure.Storage;

namespace TierMatch.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString =
            configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException(
                "Die Connection-String-Konfiguration " +
                "'DefaultConnection' wurde nicht gefunden.");

        //
        // Datenbank
        //

        services.AddDbContext<AppDbContext>(options =>
        {
            options.UseNpgsql(connectionString);
        });

        services.AddScoped<IUnitOfWork, UnitOfWork>();

        //
        // Konfiguration
        //

        services.Configure<JwtOptions>(
            configuration.GetSection(
                JwtOptions.SectionName));

        services.Configure<SeedOptions>(
            configuration.GetSection(
                SeedOptions.SectionName));

        //
        // ASP.NET Core Identity
        //

        services
            .AddIdentity<ApplicationUser, IdentityRole<Guid>>(options =>
            {
                // Passwortregeln
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequiredLength = 8;
                options.Password.RequiredUniqueChars = 1;

                // Benutzer
                options.User.RequireUniqueEmail = true;

                // Sperrung nach fehlgeschlagenen Anmeldungen
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        //
        // Authentifizierung
        //

        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IIdentityService, IdentityService>();

        services.AddScoped<
            IRefreshTokenService,
            RefreshTokenService>();

        services.AddScoped<
            IRefreshTokenRepository,
            RefreshTokenRepository>();

        //
        // Repositories
        //

        services.AddScoped<
            IAnimalRepository,
            AnimalRepository>();

        services.AddScoped<
            IShelterRepository,
            ShelterRepository>();

        services.AddScoped<
            IAnimalImageRepository,
            AnimalImageRepository>();

        services.AddScoped<
            IAdoptionRequestRepository,
            AdoptionRequestRepository>();

        //
        // Dateispeicher
        //

        services.AddScoped<
            IFileStorage,
            LocalFileStorage>();

        return services;
    }
}