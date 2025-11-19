using System.Collections.Concurrent;
using System.Text.Json;

namespace ProjectEvolution.Game;

/// <summary>
/// Shared state between C# tuner and web API
/// Thread-safe, updated by tuner, read by WebApi
/// </summary>
public static class TunerWebState
{
    private static readonly object _lock = new();
    private static TunerStateData _currentState = new();

    public static void Update(TunerStateData state)
    {
        lock (_lock)
        {
            _currentState = state;
            LastUpdate = DateTime.Now;
        }
    }

    public static TunerStateData GetCurrent()
    {
        lock (_lock)
        {
            return _currentState;
        }
    }

    public static DateTime LastUpdate { get; private set; } = DateTime.Now;
}

public class TunerStateData
{
    public int Generation { get; set; }
    public double BestFitness { get; set; }
    public double AvgFitness { get; set; }
    public double GenPerSec { get; set; }
    public int StuckGens { get; set; }
    public string Phase { get; set; } = "";
    public int PopulationSize { get; set; }
    public double ChampionFitness { get; set; }
    public int ChampionGen { get; set; }
    public int Resets { get; set; }
    public string Device { get; set; } = "CPU";
    public TimeSpan Elapsed { get; set; }
}
