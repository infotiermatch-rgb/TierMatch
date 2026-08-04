using Microsoft.OpenApi.Models;

using TierMatch.Api.OpenApi;

namespace TierMatch.Api.Extensions;

public static class SwaggerServiceExtensions
{
    public static IServiceCollection AddTierMatchSwagger(
        this IServiceCollection services)
    {
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc(
                "v1",
                new OpenApiInfo
                {
                    Title = "TierMatch API",
                    Version = "v1",
                    Description =
                        "Backend-API für die TierMatch-Anwendung."
                });

            options.AddSecurityDefinition(
                "Bearer",
                new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Description =
                        "JWT Access Token eingeben. " +
                        "Nur den Token einfügen, ohne das Wort Bearer.",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT"
                });

            options.OperationFilter<AuthorizeOperationFilter>();
        });

        return services;
    }
}