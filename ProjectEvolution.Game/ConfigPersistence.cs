namespace ProjectEvolution.Game;

using System.Text.Json;

public class OptimalConfig
{
    public int MobDetectionRange { get; set; }
    public int MaxMobs { get; set; }
    public int MinMobs { get; set; }
    public int PlayerStartHP { get; set; }
    public int PlayerStrength { get; set; }
    public int PlayerDefense { get; set; }
    public double Score { get; set; }
    public double AvgTurns { get; set; }
    public int TotalGamesSimulated { get; set; }
    public DateTime Discovered { get; set; }
}

public static class ConfigPersistence
{
    private const string ConfigFile = "optimal_config.json";

    public static void SaveOptimalConfig(SimulationConfig config, SimulationStats stats, double score, int gamesSimulated)
    {
        var optimal = new OptimalConfig
        {
            MobDetectionRange = config.MobDetectionRange,
            MaxMobs = config.MaxMobs,
            MinMobs = config.MinMobs,
            PlayerStartHP = config.PlayerStartHP,
            PlayerStrength = config.PlayerStrength,
            PlayerDefense = config.PlayerDefense,
            Score = score,
            AvgTurns = stats.AverageTurnsPerRun,
            TotalGamesSimulated = gamesSimulated,
            Discovered = DateTime.Now
        };

        var json = JsonSerializer.Serialize(optimal, new JsonSerializerOptions { WriteIndented = true });
        File.WriteAllText(ConfigFile, json);

        Console.WriteLine($"\n💾 Saved optimal config (Score: {score:F1}) to {ConfigFile}");
    }

    public static SimulationConfig? LoadOptimalConfig()
    {
        if (!File.Exists(ConfigFile))
        {
            Console.WriteLine($"ℹ️  No saved config found - using defaults");
            return null;
        }

        try
        {
            var json = File.ReadAllText(ConfigFile);
            var optimal = JsonSerializer.Deserialize<OptimalConfig>(json);

            if (optimal == null) return null;

            Console.WriteLine($"\n📂 Loaded optimal config from {ConfigFile}");
            Console.WriteLine($"   Score: {optimal.Score:F1}, AvgTurns: {optimal.AvgTurns:F1}");
            Console.WriteLine($"   Based on {optimal.TotalGamesSimulated:N0} games");
            Console.WriteLine($"   Discovered: {optimal.Discovered:g}\n");

            return new SimulationConfig
            {
                MobDetectionRange = optimal.MobDetectionRange,
                MaxMobs = optimal.MaxMobs,
                MinMobs = optimal.MinMobs,
                PlayerStartHP = optimal.PlayerStartHP,
                PlayerStrength = optimal.PlayerStrength,
                PlayerDefense = optimal.PlayerDefense,
                ShowVisuals = false
            };
        }
        catch
        {
            Console.WriteLine("⚠️  Failed to load config - using defaults");
            return null;
        }
    }
}
