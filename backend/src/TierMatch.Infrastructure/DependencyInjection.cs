using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using TierMatch.Application.Interfaces;

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
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(
                configuration.GetConnectionString("DefaultConnection")));

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

                // Lockout
                options.Lockout.DefaultLockoutTimeSpan =
                    TimeSpan.FromMinutes(15);

                options.Lockout.MaxFailedAccessAttempts = 5;

                options.Lockout.AllowedForNewUsers = true;
            })
            .AddEntityFrameworkStores<AppDbContext>()
            .AddDefaultTokenProviders();

        //
        // Repositories
        //

        services.AddScoped<IAnimalRepository, AnimalRepository>();

        services.AddScoped<IShelterRepository, ShelterRepository>();

        services.AddScoped<IAnimalImageRepository, AnimalImageRepository>();

        services.AddScoped<IAdoptionRequestRepository, AdoptionRequestRepository>();

        //
        // Storage
        //

        services.AddScoped<IFileStorage, LocalFileStorage>();

        return services;
    }
}