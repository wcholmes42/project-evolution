using Microsoft.AspNetCore.Builder;
using ProjectEvolution.Game;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors();

// Serve Ultima dashboard
app.MapGet("/", () => Results.Content(File.ReadAllText("wwwroot/ultima.html"), "text/html"));

// Get current tuner state (from shared memory)
app.MapGet("/api/state", () =>
{
    var state = TunerWebState.GetCurrent();
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
            device = state.Device
        },
        hardware = new
        {
            cpu_percent = 0.0,
            gpu_percent = 0,
            gpu_temp = 0,
            ram_used_gb = 0.0
        },
        timestamp = DateTime.Now
    });
});

Console.WriteLine("🌐 WebApi running on http://localhost:8000");
app.Run("http://0.0.0.0:8000");
