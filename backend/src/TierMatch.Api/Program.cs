using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using Serilog;

using TierMatch.Api.Middleware;
using TierMatch.Api.Services;
using TierMatch.Application;
using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;
using TierMatch.Infrastructure;

const string FrontendCorsPolicy = "Frontend";
const string SwaggerDocumentName = "v1";

var builder = WebApplication.CreateBuilder(args);

// ------------------------------------------------------------
// Logging
// ------------------------------------------------------------

builder.Host.UseSerilog(
    (context, services, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext();
    });

// ------------------------------------------------------------
// Controller und API-Dokumentation
// ------------------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        SwaggerDocumentName,
        new OpenApiInfo
        {
            Title = "TierMatch API",
            Version = "v1",
            Description =
                "REST API für TierMatch – Tiere und Menschen zusammenbringen."
        });

    const string bearerSchemeName =
        JwtBearerDefaults.AuthenticationScheme;

    options.AddSecurityDefinition(
        bearerSchemeName,
        new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Description =
                "JWT Access Token eingeben. Nur den Token einfügen – " +
                "Swagger ergänzt „Bearer“ automatisch.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT"
        });

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = bearerSchemeName
                    }
                },
                Array.Empty<string>()
            }
        });

    var xmlFileName =
        $"{typeof(Program).Assembly.GetName().Name}.xml";

    var xmlFilePath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFileName);

    if (File.Exists(xmlFilePath))
    {
        options.IncludeXmlComments(xmlFilePath);
    }
});

// ------------------------------------------------------------
// CORS für das React-/Vite-Frontend
// ------------------------------------------------------------

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        FrontendCorsPolicy,
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "http://127.0.0.1:5173")
                .AllowAnyHeader()
                .AllowAnyMethod();
        });
});

// ------------------------------------------------------------
// Application und Infrastructure
// ------------------------------------------------------------

builder.Services.AddApplication();

builder.Services.AddInfrastructure(
    builder.Configuration);

// ------------------------------------------------------------
// Aktuell angemeldeter Benutzer
// ------------------------------------------------------------

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

// ------------------------------------------------------------
// JWT-Konfiguration
// ------------------------------------------------------------

var jwtIssuer = GetRequiredConfigurationValue(
    builder.Configuration,
    "JWT-Issuer",
    "Jwt:Issuer",
    "JwtSettings:Issuer",
    "Authentication:Jwt:Issuer");

var jwtAudience = GetRequiredConfigurationValue(
    builder.Configuration,
    "JWT-Audience",
    "Jwt:Audience",
    "JwtSettings:Audience",
    "Authentication:Jwt:Audience");

var jwtSigningKey = GetRequiredConfigurationValue(
    builder.Configuration,
    "JWT-Signaturschlüssel",
    "Jwt:SigningKey",
    "Jwt:SecretKey",
    "Jwt:Secret",
    "Jwt:Key",
    "JwtSettings:SigningKey",
    "JwtSettings:SecretKey",
    "JwtSettings:Secret",
    "JwtSettings:Key",
    "Authentication:Jwt:SigningKey",
    "Authentication:Jwt:SecretKey",
    "Authentication:Jwt:Secret",
    "Authentication:Jwt:Key");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultChallengeScheme =
            JwtBearerDefaults.AuthenticationScheme;

        options.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.RequireHttpsMetadata =
            !builder.Environment.IsDevelopment();

        options.SaveToken = false;

        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = jwtIssuer,

                ValidateAudience = true,
                ValidAudience = jwtAudience,

                ValidateIssuerSigningKey = true,
                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtSigningKey)),

                ValidateLifetime = true,

                // Ein abgelaufener Access Token wird sofort ungültig.
                ClockSkew = TimeSpan.Zero,

                NameClaimType =
                    ClaimTypes.NameIdentifier,

                RoleClaimType =
                    ClaimTypes.Role
            };
    });

// ------------------------------------------------------------
// Berechtigungsrichtlinien
// ------------------------------------------------------------

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(
        Policies.Admin,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(Roles.Admin);
        });

    options.AddPolicy(
        Policies.ShelterAdmin,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.Admin,
                Roles.ShelterAdmin);
        });

    options.AddPolicy(
        Policies.User,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.Admin,
                Roles.ShelterAdmin,
                Roles.User);
        });

    options.AddPolicy(
        Policies.CanManageShelter,
        policy =>
        {
            policy.RequireAuthenticatedUser();

            policy.RequireRole(
                Roles.Admin,
                Roles.ShelterAdmin);
        });
});

// ------------------------------------------------------------
// Anwendung erstellen
// ------------------------------------------------------------

var app = builder.Build();

// ------------------------------------------------------------
// Middleware
// ------------------------------------------------------------

app.UseSerilogRequestLogging();

app.UseMiddleware<GlobalExceptionMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            $"/swagger/{SwaggerDocumentName}/swagger.json",
            "TierMatch API v1");

        options.DocumentTitle =
            "TierMatch API";

        options.DisplayRequestDuration();
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseRouting();

// CORS muss vor Authentifizierung und Autorisierung ausgeführt werden.
app.UseCors(FrontendCorsPolicy);

// Öffentlich erreichbare Tierbilder aus wwwroot.
app.UseStaticFiles();

app.UseAuthentication();

app.UseAuthorization();

// ------------------------------------------------------------
// Endpunkte
// ------------------------------------------------------------

app.MapControllers();

app.MapGet(
        "/api/v1/health",
        () =>
        {
            return Results.Ok(
                new
                {
                    success = true,
                    service = "TierMatch API",
                    version = "1.0.0",
                    status = "online",
                    timestamp = DateTimeOffset.UtcNow
                });
        })
    .AllowAnonymous()
    .WithName("GetHealth")
    .WithTags("System")
    .Produces(StatusCodes.Status200OK);

app.MapGet(
        "/",
        () => "🚀 TierMatch API läuft!")
    .AllowAnonymous()
    .ExcludeFromDescription();

app.Run();

// ------------------------------------------------------------
// Hilfsmethoden
// ------------------------------------------------------------

static string GetRequiredConfigurationValue(
    IConfiguration configuration,
    string description,
    params string[] possibleKeys)
{
    foreach (var key in possibleKeys)
    {
        var value = configuration[key];

        if (!string.IsNullOrWhiteSpace(value))
        {
            return value;
        }
    }

    throw new InvalidOperationException(
        $"Die Konfiguration für „{description}“ fehlt. " +
        $"Geprüfte Schlüssel: {string.Join(", ", possibleKeys)}.");
}

// Wird für WebApplicationFactory<Program> in den Integrationstests benötigt.
public partial class Program;