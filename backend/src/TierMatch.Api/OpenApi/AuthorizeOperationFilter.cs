using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi.Models;

using Swashbuckle.AspNetCore.SwaggerGen;

namespace TierMatch.Api.OpenApi;

public sealed class AuthorizeOperationFilter : IOperationFilter
{
    public void Apply(
        OpenApiOperation operation,
        OperationFilterContext context)
    {
        var methodAttributes = context.MethodInfo
            .GetCustomAttributes(inherit: true);

        var controllerAttributes = context.MethodInfo
            .DeclaringType?
            .GetCustomAttributes(inherit: true)
            ?? Array.Empty<object>();

        var allowsAnonymous =
            methodAttributes.OfType<AllowAnonymousAttribute>().Any() ||
            controllerAttributes.OfType<AllowAnonymousAttribute>().Any();

        if (allowsAnonymous)
        {
            return;
        }

        var requiresAuthorization =
            methodAttributes.OfType<AuthorizeAttribute>().Any() ||
            controllerAttributes.OfType<AuthorizeAttribute>().Any();

        if (!requiresAuthorization)
        {
            return;
        }

        operation.Responses.TryAdd(
            StatusCodes.Status401Unauthorized.ToString(),
            new OpenApiResponse
            {
                Description = "Nicht authentifiziert."
            });

        operation.Responses.TryAdd(
            StatusCodes.Status403Forbidden.ToString(),
            new OpenApiResponse
            {
                Description = "Keine Berechtigung."
            });

        operation.Security ??=
            new List<OpenApiSecurityRequirement>();

        operation.Security.Add(
            new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
    }
}