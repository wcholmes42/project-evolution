namespace ProjectEvolution.Game;

public class AutoTuner
{
    public static void RunAutoTuning(int cycles = 10)
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           AUTOMATED TUNING - 10 CYCLES                         ║");
        Console.WriteLine("║           Press ESC anytime to abort all cycles                ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        var config = new SimulationConfig
        {
            ShowVisuals = false, // Batch mode for speed
            MobDetectionRange = 3,
            MaxMobs = 20,
            MinMobs = 5,
            PlayerStartHP = 10,
            PlayerStrength = 2,
            PlayerDefense = 1
        };

        var allCycleResults = new List<(SimulationConfig config, SimulationStats stats, string assessment)>();

        for (int cycle = 0; cycle < cycles; cycle++)
        {
            Console.WriteLine($"\n{'═',60}");
            Console.WriteLine($"CYCLE {cycle + 1}/{cycles}");
            Console.WriteLine($"{'═',60}");
            Console.WriteLine($"Testing with: Detection={config.MobDetectionRange}, MaxMobs={config.MaxMobs}, HP={config.PlayerStartHP}, STR={config.PlayerStrength}, DEF={config.PlayerDefense}");

            var simulator = new GameSimulator(config);
            var stats = simulator.RunSimulation(20); // 20 runs per cycle

            // Analyze results
            string assessment = AnalyzeResults(stats);
            Console.WriteLine($"\n{assessment}");

            allCycleResults.Add((CloneConfig(config), stats, assessment));

            // Auto-tune based on results
            if (cycle < cycles - 1) // Don't tune after last cycle
            {
                AdjustConfig(config, stats);
                Console.WriteLine($"\n→ Adjusted settings for next cycle");
                Thread.Sleep(1000);
            }
        }

        // Final report
        PrintFinalReport(allCycleResults);
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

    private static string AnalyzeResults(SimulationStats stats)
    {
        double avgTurns = stats.AverageTurnsPerRun;
        double survivalRate = stats.SurvivalRate;

        if (avgTurns < 15)
            return $"❌ TOO BRUTAL! Avg {avgTurns:F1} turns - players dying instantly";
        else if (avgTurns < 30)
            return $"⚠️  TOO HARD: Avg {avgTurns:F1} turns - very challenging";
        else if (avgTurns < 80)
            return $"✅ BALANCED: Avg {avgTurns:F1} turns - good challenge!";
        else if (avgTurns < 150)
            return $"⚠️  Getting Easy: Avg {avgTurns:F1} turns";
        else
            return $"❌ TOO EASY: Avg {avgTurns:F1} turns - needs more challenge";
    }

    private static void AdjustConfig(SimulationConfig config, SimulationStats stats)
    {
        double avgTurns = stats.AverageTurnsPerRun;

        // Too hard - make easier
        if (avgTurns < 30)
        {
            if (avgTurns < 15) // Brutally hard
            {
                config.PlayerStartHP += 5;
                config.PlayerDefense += 1;
                config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                Console.WriteLine($"  ↑ HP +5, DEF +1, Detection -1 (brutal difficulty)");
            }
            else // Just hard
            {
                config.PlayerStartHP += 2;
                config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                Console.WriteLine($"  ↑ HP +2, Detection -1");
            }
        }
        // Too easy - make harder
        else if (avgTurns > 100)
        {
            config.MobDetectionRange = Math.Min(6, config.MobDetectionRange + 1);
            config.MaxMobs = Math.Min(30, config.MaxMobs + 3);
            Console.WriteLine($"  ↑ Detection +1, MaxMobs +3");
        }
        // Slightly too easy
        else if (avgTurns > 80)
        {
            config.MobDetectionRange = Math.Min(5, config.MobDetectionRange + 1);
            Console.WriteLine($"  ↑ Detection +1");
        }
        // Goldilocks zone - minor tweaks
        else
        {
            Console.WriteLine($"  → Settings are good, minor variance only");
        }
    }

    private static void PrintFinalReport(List<(SimulationConfig config, SimulationStats stats, string assessment)> results)
    {
        Console.Clear();
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              AUTO-TUNING FINAL REPORT                          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine("PROGRESSION ACROSS CYCLES:");
        Console.WriteLine($"{"Cycle",-8} {"AvgTurns",-12} {"Combats",-10} {"Assessment",-30}");
        Console.WriteLine(new string('-', 70));

        for (int i = 0; i < results.Count; i++)
        {
            var (cfg, stats, assessment) = results[i];
            Console.WriteLine($"{i + 1,-8} {stats.AverageTurnsPerRun,-12:F1} {stats.AverageCombatsWon,-10:F1} {assessment}");
        }

        var bestCycle = results.OrderByDescending(r =>
        {
            double avg = r.stats.AverageTurnsPerRun;
            // Score: prefer 40-80 range (sweet spot)
            if (avg >= 40 && avg <= 80) return 100 - Math.Abs(60 - avg);
            return 50 - Math.Abs(60 - avg);
        }).First();

        int bestIndex = results.IndexOf(bestCycle);

        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                 RECOMMENDED SETTINGS                           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine($"\nBest balance found in CYCLE {bestIndex + 1}:");
        Console.WriteLine($"  Mob Detection Range: {bestCycle.config.MobDetectionRange}");
        Console.WriteLine($"  Max Mobs: {bestCycle.config.MaxMobs}");
        Console.WriteLine($"  Player Start HP: {bestCycle.config.PlayerStartHP}");
        Console.WriteLine($"  Player Strength: {bestCycle.config.PlayerStrength}");
        Console.WriteLine($"  Player Defense: {bestCycle.config.PlayerDefense}");
        Console.WriteLine($"\n  Average Survival: {bestCycle.stats.AverageTurnsPerRun:F1} turns");
        Console.WriteLine($"  Average Combats: {bestCycle.stats.AverageCombatsWon:F1}");
        Console.WriteLine($"  {bestCycle.assessment}");

        Console.WriteLine("\n\nPress any key to return to main menu...");
        Console.ReadKey(intercept: true);
    }
}
