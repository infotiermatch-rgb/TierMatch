using System.Text;
using System.Text.Json.Serialization;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using TierMatch.Api.Middleware;
using TierMatch.Application;
using TierMatch.Infrastructure;
using TierMatch.Infrastructure.Authentication;

var builder = WebApplication.CreateBuilder(args);

//
// Services
//

builder.Services.AddApplication();

var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection");

Console.WriteLine("========================================");
Console.WriteLine(connectionString);
Console.WriteLine("========================================");

builder.Services.AddInfrastructure(builder.Configuration);

//
// JWT
//

var jwtOptions = builder.Configuration
    .GetSection(JwtOptions.SectionName)
    .Get<JwtOptions>()
    ?? throw new InvalidOperationException(
        "JWT-Konfiguration wurde nicht gefunden.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
                            jwtOptions.SecretKey))
            };
    });

builder.Services.AddAuthorization();

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

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "TierMatch API",
        Version = "v1"
    });

    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Description = "JWT Bearer Token",

        In = ParameterLocation.Header,

        Type = SecuritySchemeType.Http,

        Scheme = "bearer",

        BearerFormat = "JWT",

        Reference = new OpenApiReference
        {
            Type = ReferenceType.SecurityScheme,
            Id = JwtBearerDefaults.AuthenticationScheme
        }
    };

    options.AddSecurityDefinition(
        JwtBearerDefaults.AuthenticationScheme,
        securityScheme);

    options.AddSecurityRequirement(
        new OpenApiSecurityRequirement
        {
            {
                securityScheme,
                Array.Empty<string>()
            }
        });
});

var app = builder.Build();

//
// Seeder
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

app.MapGet("/", () => "🚀 TierMatch API läuft!");

app.MapGet("/api/v1/health", () =>
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