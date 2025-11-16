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

        var baseConfig = new SimulationConfig
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
        double baseScore = 100 - Math.Abs(baseStats.AverageTurnsPerRun - 50) * 2;

        Console.WriteLine($"Baseline: {baseStats.AverageTurnsPerRun:F1} turns, Score: {baseScore:F1}\n");

        // Test each parameter independently
        TestParameter("MobDetectionRange", 2, 6, baseConfig, baseStats.AverageTurnsPerRun);
        TestParameter("MaxMobs", 15, 40, baseConfig, baseStats.AverageTurnsPerRun);
        TestParameter("PlayerStartHP", 5, 15, baseConfig, baseStats.AverageTurnsPerRun);
        TestParameter("PlayerDefense", 0, 4, baseConfig, baseStats.AverageTurnsPerRun);

        Console.WriteLine("\n\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }

    private static void TestParameter(string paramName, int min, int max, SimulationConfig baseConfig, double baseAvg)
    {
        Console.WriteLine($"\n{'═',60}");
        Console.WriteLine($"Testing {paramName} ({min}-{max}):");
        Console.WriteLine($"{"Value",-8} {"AvgTurns",-12} {"Score",-10} {"vs Baseline"}");
        Console.WriteLine(new string('-', 50));

        double bestScore = 0;
        int bestValue = 0;

        for (int value = min; value <= max; value++)
        {
            var testConfig = CloneConfig(baseConfig);

            // Set the parameter being tested
            switch (paramName)
            {
                case "MobDetectionRange": testConfig.MobDetectionRange = value; break;
                case "MaxMobs": testConfig.MaxMobs = value; break;
                case "PlayerStartHP": testConfig.PlayerStartHP = value; break;
                case "PlayerDefense": testConfig.PlayerDefense = value; break;
            }

            var simulator = new GameSimulator(testConfig);
            var stats = simulator.RunSimulation(200); // Parallel execution
            double score = 100 - Math.Abs(stats.AverageTurnsPerRun - 50) * 2;

            if (score > bestScore)
            {
                bestScore = score;
                bestValue = value;
            }

            string indicator = score > bestScore - 1 ? "✅" : value == GetCurrentValue(baseConfig, paramName) ? "📍" : "  ";
            Console.WriteLine($"{value,-8} {stats.AverageTurnsPerRun,-12:F1} {score,-10:F1} {indicator}");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n→ BEST {paramName}: {bestValue} (Score: {bestScore:F1})");
        Console.ResetColor();
    }

    private static int GetCurrentValue(SimulationConfig config, string paramName)
    {
        return paramName switch
        {
            "MobDetectionRange" => config.MobDetectionRange,
            "MaxMobs" => config.MaxMobs,
            "PlayerStartHP" => config.PlayerStartHP,
            "PlayerDefense" => config.PlayerDefense,
            _ => 0
        };
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
