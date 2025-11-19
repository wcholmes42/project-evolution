using Microsoft.AspNetCore.Builder;
using ProjectEvolution.Game;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON to use PascalCase (not camelCase)
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = null; // Use PascalCase
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors();

// Serve Ultima dashboard
app.MapGet("/", () =>
{
    var htmlPath = File.Exists("wwwroot/ultima.html") ? "wwwroot/ultima.html" : "/app/webapi/wwwroot/ultima.html";
    return Results.Content(File.ReadAllText(htmlPath), "text/html");
});

// Get current tuner state (from shared memory) + framework data
app.MapGet("/api/state", () =>
{
    var state = TunerWebState.GetCurrent();

    // Load current best framework from disk
    ProgressionFrameworkData? currentFramework = null;
    try
    {
        var frameworkPath = "/data/progression_framework.json";
        if (!File.Exists(frameworkPath)) frameworkPath = "progression_framework.json";
        if (File.Exists(frameworkPath))
        {
            var json = File.ReadAllText(frameworkPath);
            currentFramework = System.Text.Json.JsonSerializer.Deserialize<ProgressionFrameworkData>(json);
        }
    }
    catch { }

    return Results.Json(new
    {
        evolution = new
        {
            generation = state.Generation,
            best_fitness = state.BestFitness,
            avg_fitness = state.AvgFitness,
            gen_per_sec = state.GenPerSec,
            stuck_gens = state.StuckGens,
            phase = state.Phase,
            population_size = state.PopulationSize,
            champion_fitness = state.ChampionFitness,
            champion_gen = state.ChampionGen,
            resets = state.Resets,
            device = state.Device,
            elapsed = state.Elapsed.ToString(@"hh\:mm\:ss")
        },
        framework = currentFramework, // FULL FRAMEWORK DATA
        hardware = new
        {
            cpu_percent = 0.0,
            gpu_percent = 0,
            gpu_temp = 0,
            ram_used_gb = 0.0
        },
        timestamp = DateTime.Now,
        last_update = TunerWebState.LastUpdate
    });
});

Console.WriteLine("🌐 WebApi running on http://localhost:8000");

// Auto-start tuner in background thread (Docker mode)
if (args.Length > 0 && args[0] == "auto" || Console.IsInputRedirected)
{
    Console.WriteLine("🐳 Auto-starting tuner in background...");
    _ = Task.Run(() =>
    {
        try
        {
            ProgressionFrameworkResearcher.RunContinuousResearchHeadless();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Tuner error: {ex.Message}");
        }
    });
}

app.Run("http://0.0.0.0:8000");
