namespace ProjectEvolution.Game;

// FIX #2: Single-parameter optimization - no interference!
public class SingleParamTuner
{
    public static void RunSingleParamOptimization()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    SINGLE-PARAMETER OPTIMIZATION - NO INTERFERENCE             ║");
        Console.WriteLine("║    Tests one parameter at a time - Press ESC to stop           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        // Load saved or use defaults
        var baseConfig = ConfigPersistence.LoadOptimalConfig() ?? new SimulationConfig
        {
            MobDetectionRange = 3,
            MaxMobs = 29,
            MinMobs = 5,
            PlayerStartHP = 9,
            PlayerStrength = 2,
            PlayerDefense = 1,
            ShowVisuals = false
        };

        Console.WriteLine("Testing baseline...");
        var baseSimulator = new GameSimulator(baseConfig);
        var baseStats = baseSimulator.RunSimulation(300);
        double baseScore = 100 - Math.Abs(baseStats.AverageTurnsPerRun - 75) * 2;

        Console.WriteLine($"Baseline: {baseStats.AverageTurnsPerRun:F1} turns, Score: {baseScore:F1}\n");

        // Test each parameter
        var bestDet = TestParameter("MobDetectionRange", 2, 10, baseConfig);
        var bestMobs = TestParameter("MaxMobs", 15, 60, baseConfig);
        var bestHP = TestParameter("PlayerStartHP", 3, 15, baseConfig);
        var bestDef = TestParameter("PlayerDefense", 0, 4, baseConfig);

        // Build and test combined optimal
        var optimalConfig = new SimulationConfig
        {
            MobDetectionRange = bestDet,
            MaxMobs = bestMobs,
            PlayerStartHP = bestHP,
            PlayerDefense = bestDef,
            PlayerStrength = baseConfig.PlayerStrength,
            MinMobs = baseConfig.MinMobs,
            ShowVisuals = false
        };

        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         TESTING COMBINED OPTIMAL CONFIG                      ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"Det={bestDet}, Mobs={bestMobs}, HP={bestHP}, Def={bestDef}\n");

        var finalSim = new GameSimulator(optimalConfig);
        var finalStats = finalSim.RunSimulation(300);
        double finalScore = 100 - Math.Abs(finalStats.AverageTurnsPerRun - 75) * 2;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ COMBINED RESULT: {finalStats.AverageTurnsPerRun:F1} turns, Score: {finalScore:F1}");

        // Save if better!
        ConfigPersistence.SaveOptimalConfig(optimalConfig, finalStats, finalScore, 60000);
        Console.WriteLine($"💾 Saved optimal config - [G] will use this!");
        Console.ResetColor();

        Console.WriteLine("\n\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }

    private static int TestParameter(string paramName, int min, int max, SimulationConfig baseConfig)
    {
        Console.WriteLine($"\n{'═',60}");
        Console.WriteLine($"Testing {paramName} ({min}-{max}):");
        Console.WriteLine($"{"Value",-8} {"AvgTurns",-12} {"Score",-10}");
        Console.WriteLine(new string('-', 40));

        double bestScore = -1000;
        int bestValue = min;

        for (int value = min; value <= max; value++)
        {
            var testConfig = CloneConfig(baseConfig);

            switch (paramName)
            {
                case "MobDetectionRange": testConfig.MobDetectionRange = value; break;
                case "MaxMobs": testConfig.MaxMobs = value; break;
                case "PlayerStartHP": testConfig.PlayerStartHP = value; break;
                case "PlayerDefense": testConfig.PlayerDefense = value; break;
            }

            var simulator = new GameSimulator(testConfig);
            var stats = simulator.RunSimulation(200);

            double avgTurns = stats.AverageTurnsPerRun;
            double score = 100.0 - Math.Abs(avgTurns - 75.0) * 2.0;

            if (score > bestScore)
            {
                bestScore = score;
                bestValue = value;
            }

            string indicator = value == bestValue && value > min ? "✅ BEST" : "";
            Console.WriteLine($"{value,-8} {avgTurns,-12:F1} {score,-10:F1} {indicator}");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n→ BEST {paramName}: {bestValue} (Score: {bestScore:F1})");
        Console.ResetColor();

        return bestValue;
    }

    private static SimulationConfig CloneConfig(SimulationConfig config)
    {
        return new SimulationConfig
        {
            MobDetectionRange = config.MobDetectionRange,
            MaxMobs = config.MaxMobs,
            MinMobs = config.MinMobs,
            PlayerStartHP = config.PlayerStartHP,
            PlayerStrength = config.PlayerStrength,
            PlayerDefense = config.PlayerDefense,
            ShowVisuals = false
        };
    }
}
