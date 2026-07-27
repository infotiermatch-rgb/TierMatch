using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TierMatch.Infrastructure.Data;
using TierMatch.Application.Interfaces;
using TierMatch.Infrastructure.Repositories;
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

    return services;
}
}