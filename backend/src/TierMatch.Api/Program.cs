using TierMatch.Application;
using TierMatch.Infrastructure;
using TierMatch.Api.Middleware;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new JsonStringEnumConverter());
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseGlobalExceptionHandling();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

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