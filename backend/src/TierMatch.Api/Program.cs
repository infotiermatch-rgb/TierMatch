using System.Text.Json.Serialization;
using TierMatch.Api.Middleware;
using TierMatch.Application;
using TierMatch.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .CreateLogger();
builder.Host.UseSerilog();

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

//
// Middleware
//

app.UseGlobalExceptionHandling();
app.UseSerilogRequestLogging();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.MapControllers();

// kommt später
// app.UseAuthentication();
// app.UseAuthorization();

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