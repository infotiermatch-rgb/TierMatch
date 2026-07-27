using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using TierMatch.Application.Abstractions.Behaviors;
using Mapster;
using TierMatch.Application.Common.Mapping;

namespace TierMatch.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly);
        });

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(ValidationBehavior<,>));

        return services;
    }
}