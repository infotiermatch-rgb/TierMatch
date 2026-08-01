using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TierMatch.Infrastructure.Data;
using TierMatch.Application.Interfaces;
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

    services.AddScoped<IAnimalRepository, AnimalRepository>();
    services.AddScoped<IShelterRepository, ShelterRepository>();
    services.AddScoped<IAnimalImageRepository, AnimalImageRepository>();
    services.AddScoped<IFileStorage, LocalFileStorage>();

    return services;
}
}