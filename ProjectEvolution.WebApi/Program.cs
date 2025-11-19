using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Add CORS for web dashboard
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();

// Serve the Ultima dashboard
app.MapGet("/", () => Results.Content(File.ReadAllText("wwwroot/ultima.html"), "text/html"));

// Stream tuner state (reads from file written by C# tuner)
app.MapGet("/api/tuner/state", () =>
{
    try
    {
        var statePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "tuner_state.json");
        if (File.Exists(statePath))
        {
            var json = File.ReadAllText(statePath);
            return Results.Content(json, "application/json");
        }
        return Results.Json(new { error = "Tuner not running" });
    }
    catch (Exception ex)
    {
        return Results.Json(new { error = ex.Message });
    }
});

app.Run("http://0.0.0.0:8000");
