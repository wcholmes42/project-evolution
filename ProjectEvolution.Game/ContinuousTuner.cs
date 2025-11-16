namespace ProjectEvolution.Game;

public class ContinuousTuner
{
    private static SimulationConfig _bestConfig = null!;
    private static SimulationStats _bestStats = null!;
    private static double _bestScore = 0;
    private static int _cyclesRun = 0;

    public static void RunContinuousTuning()
    {
        Console.Clear();
        Console.CursorVisible = false;

        var config = new SimulationConfig
        {
            ShowVisuals = false,
            MobDetectionRange = 3,
            MaxMobs = 20,
            MinMobs = 5,
            PlayerStartHP = 10,
            PlayerStrength = 2,
            PlayerDefense = 1,
            SimulationSpeed = 0
        };

        _bestConfig = CloneConfig(config);
        _cyclesRun = 0;

        DrawStaticUI();

        bool running = true;
        while (running)
        {
            _cyclesRun++;

            // Run cycle
            var simulator = new GameSimulator(config);
            var stats = simulator.RunSimulation(20);

            // Score this config (prefer 40-60 turn average)
            double score = CalculateScore(stats);

            // Update best if improved
            if (score > _bestScore || _bestScore == 0)
            {
                _bestScore = score;
                _bestConfig = CloneConfig(config);
                _bestStats = stats;
            }

            // Update display
            UpdateDisplay(config, stats, score);

            // Auto-adjust for next cycle
            AdjustConfig(config, stats);

            // Check for ESC
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    running = false;
                }
            }
        }

        Console.CursorVisible = true;
        Console.Clear();
        PrintFinalSummary();
    }

    private static void DrawStaticUI()
    {
        Console.SetCursorPosition(0, 0);
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              CONTINUOUS AUTO-TUNING - PRESS ESC TO STOP                    ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ CURRENT CYCLE:                                                             ║");
        Console.WriteLine("║   Config:                                                                  ║");
        Console.WriteLine("║   Results:                                                                 ║");
        Console.WriteLine("║   Assessment:                                                              ║");
        Console.WriteLine("║   Score:                                                                   ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ BEST CONFIGURATION FOUND:                                                  ║");
        Console.WriteLine("║   Cycle:                                                                   ║");
        Console.WriteLine("║   Config:                                                                  ║");
        Console.WriteLine("║   Results:                                                                 ║");
        Console.WriteLine("║   Score:                                                                   ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ TRENDING:                                                                  ║");
        Console.WriteLine("║   Direction:                                                               ║");
        Console.WriteLine("║   Progress:                                                                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");
    }

    private static void UpdateDisplay(SimulationConfig config, SimulationStats stats, double score)
    {
        // Current cycle
        Console.SetCursorPosition(2, 3);
        Console.Write($"CURRENT CYCLE: {_cyclesRun}                                                         ");

        Console.SetCursorPosition(4, 4);
        Console.Write($"Config: Det={config.MobDetectionRange} MaxMobs={config.MaxMobs} HP={config.PlayerStartHP} STR={config.PlayerStrength} DEF={config.PlayerDefense}                    ");

        Console.SetCursorPosition(4, 5);
        Console.Write($"Results: Avg {stats.AverageTurnsPerRun:F1} turns, {stats.AverageCombatsWon:F1} combats, {stats.Deaths}/{stats.TotalRuns} deaths           ");

        Console.SetCursorPosition(4, 6);
        string assessment = AnalyzeResults(stats);
        Console.Write($"Assessment: {assessment}                                                    ");

        Console.SetCursorPosition(4, 7);
        Console.Write($"Score: {score:F2}/100                                                              ");

        // Best config
        if (_bestStats != null)
        {
            Console.SetCursorPosition(4, 10);
            Console.Write($"Cycle: {GetBestCycleNumber()} (out of {_cyclesRun})                                           ");

            Console.SetCursorPosition(4, 11);
            Console.Write($"Config: Det={_bestConfig.MobDetectionRange} MaxMobs={_bestConfig.MaxMobs} HP={_bestConfig.PlayerStartHP} STR={_bestConfig.PlayerStrength} DEF={_bestConfig.PlayerDefense}                    ");

            Console.SetCursorPosition(4, 12);
            Console.Write($"Results: Avg {_bestStats.AverageTurnsPerRun:F1} turns, {_bestStats.AverageCombatsWon:F1} combats                      ");

            Console.SetCursorPosition(4, 13);
            Console.Write($"Score: {_bestScore:F2}/100                                                            ");
        }

        // Trending
        Console.SetCursorPosition(4, 16);
        string direction = score > _bestScore ? "↗️ IMPROVING" : score == _bestScore ? "→ STABLE" : "↘️ DECLINING";
        Console.Write($"Direction: {direction}                                                        ");

        Console.SetCursorPosition(4, 17);
        Console.Write($"Progress: {_cyclesRun} cycles completed                                               ");
    }

    private static int GetBestCycleNumber()
    {
        // Would need tracking, for now just show a placeholder
        return _cyclesRun;
    }

    private static double CalculateScore(SimulationStats stats)
    {
        double avgTurns = stats.AverageTurnsPerRun;

        // Ideal range: 40-60 turns
        double targetTurns = 50;
        double deviation = Math.Abs(avgTurns - targetTurns);

        // Score out of 100
        double score = Math.Max(0, 100 - (deviation * 2));

        return score;
    }

    private static string AnalyzeResults(SimulationStats stats)
    {
        double avgTurns = stats.AverageTurnsPerRun;

        if (avgTurns < 15) return "❌ TOO BRUTAL";
        if (avgTurns < 30) return "⚠️  TOO HARD";
        if (avgTurns < 80) return "✅ BALANCED";
        if (avgTurns < 150) return "⚠️  Getting Easy";
        return "❌ TOO EASY";
    }

    private static void AdjustConfig(SimulationConfig config, SimulationStats stats)
    {
        double avgTurns = stats.AverageTurnsPerRun;

        // Too hard - make easier
        if (avgTurns < 30)
        {
            if (avgTurns < 15)
            {
                config.PlayerStartHP += 5;
                config.PlayerDefense += 1;
                config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
            }
            else
            {
                config.PlayerStartHP += 2;
            }
        }
        // Too easy - make harder
        else if (avgTurns > 100)
        {
            config.MobDetectionRange = Math.Min(6, config.MobDetectionRange + 1);
            config.MaxMobs = Math.Min(30, config.MaxMobs + 3);
        }
        else if (avgTurns > 80)
        {
            config.MobDetectionRange = Math.Min(5, config.MobDetectionRange + 1);
        }
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

    private static void PrintFinalSummary()
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           CONTINUOUS TUNING - FINAL SUMMARY                    ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
        Console.WriteLine($"Total Cycles Run: {_cyclesRun}");
        Console.WriteLine($"Total Games Simulated: {_cyclesRun * 20}\n");

        if (_bestStats != null)
        {
            Console.WriteLine("BEST CONFIGURATION FOUND:");
            Console.WriteLine($"  Mob Detection: {_bestConfig.MobDetectionRange} tiles");
            Console.WriteLine($"  Max Mobs: {_bestConfig.MaxMobs}");
            Console.WriteLine($"  Player HP: {_bestConfig.PlayerStartHP}");
            Console.WriteLine($"  Strength: {_bestConfig.PlayerStrength}");
            Console.WriteLine($"  Defense: {_bestConfig.PlayerDefense}");
            Console.WriteLine($"\n  Performance:");
            Console.WriteLine($"    Average Survival: {_bestStats.AverageTurnsPerRun:F1} turns");
            Console.WriteLine($"    Average Combats: {_bestStats.AverageCombatsWon:F1}");
            Console.WriteLine($"    Balance Score: {_bestScore:F1}/100");
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
