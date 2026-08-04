using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

using TierMatch.Api.Extensions;
using TierMatch.Api.Middleware;
using TierMatch.Api.Services;
using TierMatch.Application;
using TierMatch.Application.Authorization;
using TierMatch.Application.Interfaces;
using TierMatch.Infrastructure;
using TierMatch.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

//
// Application und Infrastructure
//

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

//
// Aktueller Benutzer
//

builder.Services.AddHttpContextAccessor();

builder.Services.AddScoped<
    ICurrentUserService,
    CurrentUserService>();

//
// JWT Authentication
//

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT-Konfiguration wurde nicht gefunden.");

if (string.IsNullOrWhiteSpace(jwtOptions.SecretKey))
{
    throw new InvalidOperationException(
        "Der JWT-SecretKey wurde nicht konfiguriert.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Issuer))
{
    throw new InvalidOperationException(
        "Der JWT-Issuer wurde nicht konfiguriert.");
}

if (string.IsNullOrWhiteSpace(jwtOptions.Audience))
{
    throw new InvalidOperationException(
        "Die JWT-Audience wurde nicht konfiguriert.");
}

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
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(
                            jwtOptions.SecretKey)),

                ClockSkew = TimeSpan.Zero
            };
    });

//
// Authorization und Rollen-Policies
//

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
            policy.RequireRole(Roles.ShelterAdmin);
        });

    options.AddPolicy(
        Policies.User,
        policy =>
        {
            policy.RequireAuthenticatedUser();
            policy.RequireRole(Roles.User);
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

//
// Controller
//

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

//
// Swagger
//

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddTierMatchSwagger();

var app = builder.Build();

//
// Identity Seeder
//

var seedOptions = app.Services
    .GetRequiredService<IOptions<SeedOptions>>()
    .Value;

await IdentitySeeder.SeedAsync(
    app.Services,
    seedOptions);

//
// Middleware
//

app.UseGlobalExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthentication();
app.UseAuthorization();

//
// Endpoints
//

app.MapGet(
    "/",
    () => "🚀 TierMatch API läuft!");

app.MapGet(
    "/api/v1/health",
    () =>
    {
        return Results.Ok(new
        {
            success = true,
            service = "TierMatch API",
            version = "1.0.0",
            status = "online",
            timestamp = DateTime.UtcNow
        });
    });

app.MapControllers();

app.Run();

public partial class Program
{
}