namespace ProjectEvolution.Game;

// FIX #3: Random search with restarts - escape local minima!
public class RandomSearchTuner
{
    private static List<(SimulationConfig config, double score, double avgTurns)> _allResults = new List<(SimulationConfig, double, double)>();

    public static void RunRandomSearch(int iterations = 50)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         RANDOM SEARCH - EXPLORE THE SOLUTION SPACE             ║");
        Console.WriteLine("║         Press ESC to stop                                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        var random = new Random();
        double bestScore = 0;
        SimulationConfig? bestConfig = null;

        for (int i = 0; i < iterations; i++)
        {
            // Generate random configuration with EXPANDED RANGES!
            var config = new SimulationConfig
            {
                MobDetectionRange = random.Next(2, 11), // Expanded to 10!
                MaxMobs = random.Next(15, 61), // Expanded to 60!
                MinMobs = 5,
                PlayerStartHP = random.Next(3, 16), // Lowered to 3!
                PlayerStrength = random.Next(1, 5),
                PlayerDefense = random.Next(0, 4),
                ShowVisuals = false
            };

            // Test it
            var simulator = new GameSimulator(config);
            var stats = simulator.RunSimulation(200);

            double avgTurns = stats.AverageTurnsPerRun;
            double score = 100 - Math.Abs(avgTurns - 75) * 2; // Use realistic target!

            _allResults.Add((config, score, avgTurns));

            if (score > bestScore)
            {
                bestScore = score;
                bestConfig = config;

                // Save to persist across runs!
                ConfigPersistence.SaveOptimalConfig(config, stats, score, (i + 1) * 200);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"✨ NEW BEST! Iteration {i+1}: Score {score:F1}, Avg {avgTurns:F1} turns");
                Console.WriteLine($"   Det={config.MobDetectionRange} Mobs={config.MaxMobs} HP={config.PlayerStartHP} STR={config.PlayerStrength} DEF={config.PlayerDefense}");
                Console.ResetColor();
            }
            else
            {
                Console.Write($"\rIteration {i+1}/{iterations}: Score {score:F1} (Best: {bestScore:F1})    ");
            }

            // Check for ESC
            if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
            {
                break;
            }
        }

        // Print results
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                  RANDOM SEARCH RESULTS                         ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        if (bestConfig != null)
        {
            Console.WriteLine("BEST CONFIGURATION FOUND:");
            Console.WriteLine($"  Score: {bestScore:F1}/100");
            Console.WriteLine($"  Detection: {bestConfig.MobDetectionRange}");
            Console.WriteLine($"  MaxMobs: {bestConfig.MaxMobs}");
            Console.WriteLine($"  PlayerHP: {bestConfig.PlayerStartHP}");
            Console.WriteLine($"  Strength: {bestConfig.PlayerStrength}");
            Console.WriteLine($"  Defense: {bestConfig.PlayerDefense}");
        }

        // Show top 10
        Console.WriteLine("\n🏆 TOP 10 FROM RANDOM SEARCH:");
        var top10 = _allResults.OrderByDescending(r => r.score).Take(10);
        int rank = 1;
        foreach (var (cfg, score, avgTurns) in top10)
        {
            Console.WriteLine($"{rank}. Score {score:F1}: Det={cfg.MobDetectionRange} Mobs={cfg.MaxMobs} HP={cfg.PlayerStartHP} STR={cfg.PlayerStrength} DEF={cfg.PlayerDefense} → {avgTurns:F1} turns");
            rank++;
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
