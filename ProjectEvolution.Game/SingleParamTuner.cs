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

        // Test each parameter independently with EXPANDED RANGES!
        TestParameter("MobDetectionRange", 2, 10, baseConfig, baseStats.AverageTurnsPerRun); // Was 6, now 10!
        TestParameter("MaxMobs", 15, 60, baseConfig, baseStats.AverageTurnsPerRun); // Was 40, now 60!
        TestParameter("PlayerStartHP", 3, 15, baseConfig, baseStats.AverageTurnsPerRun); // Was 5, now 3!
        TestParameter("PlayerDefense", 0, 4, baseConfig, baseStats.AverageTurnsPerRun);

        // Test each parameter
        var (bestDet, scoreDet) = TestParameter("MobDetectionRange", 2, 10, baseConfig, ref bestOverall, ref bestOverallScore, ref bestOverallStats);
        var (bestMobs, scoreMobs) = TestParameter("MaxMobs", 15, 60, baseConfig, ref bestOverall, ref bestOverallScore, ref bestOverallStats);
        var (bestHP, scoreHP) = TestParameter("PlayerStartHP", 3, 15, baseConfig, ref bestOverall, ref bestOverallScore, ref bestOverallStats);
        var (bestDef, scoreDef) = TestParameter("PlayerDefense", 0, 4, baseConfig, ref bestOverall, ref bestOverallScore, ref bestOverallStats);

        // Build combined optimal
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
        Console.WriteLine("║         TESTING COMBINED OPTIMAL                             ║");
        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
        var finalSim = new GameSimulator(optimalConfig);
        var finalStats = finalSim.RunSimulation(300);
        double finalScore = 100 - Math.Abs(finalStats.AverageTurnsPerRun - 75) * 2;

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"✅ RESULT: {finalStats.AverageTurnsPerRun:F1} turns, Score: {finalScore:F1}");

        if (finalScore > baseScore)
        {
            ConfigPersistence.SaveOptimalConfig(optimalConfig, finalStats, finalScore, 50000);
            Console.WriteLine($"💾 Saved for [G] to use!");
        }
        Console.ResetColor();

        Console.WriteLine("\n\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }

    private static (int bestValue, double bestScore) TestParameter(string paramName, int min, int max, SimulationConfig baseConfig,
        ref SimulationConfig bestOverall, ref double bestOverallScore, ref SimulationStats bestOverallStats)
    {
        Console.WriteLine($"\n{'═',60}");
        Console.WriteLine($"Testing {paramName} ({min}-{max}):");
        Console.WriteLine($"{"Value",-8} {"AvgTurns",-12} {"Score",-10} {"vs Baseline"}");
        Console.WriteLine(new string('-', 50));

        double bestScore = -1000; // Start very low
        int bestValue = min;

        for (int i = min, value = min; value <= max; value++, i++)
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

            double avgTurns = stats.AverageTurnsPerRun;
            double score = 100.0 - Math.Abs(avgTurns - 75.0) * 2.0; // Use realistic target!

            // Track best overall config across all parameters
            if (score > bestOverallScore)
            {
                bestOverall = CloneConfig(testConfig);
                bestOverallScore = score;
                bestOverallStats = stats;
            }

            // Track best for this parameter
            if (i == min || score > bestScore)
            {
                if (score > bestScore)
                {
                    bestScore = score;
                    bestValue = value;
                }
            }

            // Indicator
            string indicator = "";
            if (value == bestValue && i > min) indicator = "✅ BEST";
            else if (value == GetCurrentValue(baseConfig, paramName)) indicator = "📍 CURRENT";
            else if (Math.Abs(score - bestScore) < 2) indicator = "⭐ GOOD";

            Console.WriteLine($"{value,-8} {avgTurns,-12:F1} {score,-10:F1} {indicator}");
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n→ BEST {paramName}: {bestValue} (Score: {bestScore:F1})");
        Console.ResetColor();

        return (bestValue, bestScore);
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
