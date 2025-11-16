namespace ProjectEvolution.Game;

public class ContinuousTuner
{
    private static SimulationConfig _bestConfig = null!;
    private static SimulationStats _bestStats = null!;
    private static double _bestScore = 0;
    private static int _cyclesRun = 0;
    private static List<double> _scoreHistory = new List<double>();
    private static int _bestCycleNumber = 0;
    private static double _lastScore = 0;
    private static string _lastChange = "Initial";
    private static int _improvementStreak = 0;
    private static int _regressionCount = 0;

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
        _scoreHistory.Clear();

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
            _scoreHistory.Add(score);

            // Update best if improved
            if (score > _bestScore || _bestScore == 0)
            {
                _bestScore = score;
                _bestConfig = CloneConfig(config);
                _bestStats = stats;
                _bestCycleNumber = _cyclesRun;
            }

            // Update display
            UpdateDisplay(config, stats, score);

            // LEARNING: Only adjust if we have a baseline (skip cycle 1)
            if (_cyclesRun == 1)
            {
                _lastScore = score;
                _lastChange = "Baseline established";
                continue; // Don't adjust on first cycle!
            }

            bool improved = score > _lastScore;
            bool regressed = score < _lastScore - 5; // Significant regression

            if (improved)
            {
                _improvementStreak++;
                _regressionCount = 0;
                // Keep going in same direction!
                AdjustConfigSmart(config, stats, "continue");
            }
            else if (regressed)
            {
                _regressionCount++;
                _improvementStreak = 0;
                // Reverse last change or try different approach
                AdjustConfigSmart(config, stats, "reverse");
            }
            else
            {
                // Stable - try small perturbation
                AdjustConfigSmart(config, stats, "explore");
            }

            _lastScore = score;

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
        Console.WriteLine("║ BALANCE CONVERGENCE GRAPH (Last 30 Cycles):                               ║");
        Console.WriteLine("║ 100 ┤                                                                      ║");
        Console.WriteLine("║  80 ┤ ▓▓▓▓▓▓ IDEAL ZONE ▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓▓              ║");
        Console.WriteLine("║  60 ┤                                                                      ║");
        Console.WriteLine("║  40 ┤                                                                      ║");
        Console.WriteLine("║  20 ┤                                                                      ║");
        Console.WriteLine("║   0 ┤                                                                      ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ TRENDING:                                                                  ║");
        Console.WriteLine("║   Direction:                                                               ║");
        Console.WriteLine("║   Progress:                                                                ║");
        Console.WriteLine("║   Learning:                                                                ║");
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

        // Draw graph
        DrawTrendingGraph();

        // Trending
        Console.SetCursorPosition(4, 23);
        string direction = score > _bestScore ? "↗️ IMPROVING" : score == _bestScore ? "→ STABLE" : "↘️ DECLINING";
        Console.Write($"Direction: {direction}                                                        ");

        Console.SetCursorPosition(4, 24);
        Console.Write($"Progress: {_cyclesRun} cycles, {_cyclesRun * 20} games | Last: {_lastChange}                               ");

        // Show learning stats
        Console.SetCursorPosition(4, 25);
        Console.Write($"Learning: {_improvementStreak} improvements, {_regressionCount} regressions                    ");
    }

    private static void DrawTrendingGraph()
    {
        // Graph shows last 30 cycles
        int graphWidth = 60;
        int graphHeight = 6;
        int startX = 7;
        int startY = 17;

        // Get last 30 scores (or all if less)
        var recentScores = _scoreHistory.TakeLast(graphWidth).ToList();
        if (recentScores.Count == 0) return;

        // Clear graph area first
        for (int y = 0; y < graphHeight; y++)
        {
            Console.SetCursorPosition(startX, startY + y);
            Console.Write(new string(' ', graphWidth));
        }

        // Plot each score
        for (int i = 0; i < recentScores.Count; i++)
        {
            double score = recentScores[i];
            int x = startX + i;

            // Map score (0-100) to graph height (0-5)
            int barHeight = (int)(score / 100.0 * graphHeight);
            barHeight = Math.Clamp(barHeight, 0, graphHeight - 1);

            // Draw from bottom up
            for (int h = 0; h <= barHeight; h++)
            {
                int y = startY + (graphHeight - 1 - h);

                Console.SetCursorPosition(x, y);

                // Color based on score
                if (score >= 80)
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (score >= 60)
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else
                    Console.ForegroundColor = ConsoleColor.Red;

                // Different characters for different heights
                if (h == barHeight)
                    Console.Write("█");
                else
                    Console.Write("█");

                Console.ResetColor();
            }
        }

        // Draw ideal zone line (at score 80)
        int idealY = startY + (int)((1.0 - 0.80) * graphHeight);
        Console.SetCursorPosition(startX, idealY);
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        for (int x = 0; x < Math.Min(graphWidth, recentScores.Count); x++)
        {
            Console.SetCursorPosition(startX + x, idealY);
            if (recentScores[x] >= 80)
                Console.Write("█"); // Bar already there
            else
                Console.Write("─"); // Show threshold
        }
        Console.ResetColor();
    }

    private static int GetBestCycleNumber()
    {
        return _bestCycleNumber;
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

    private static void AdjustConfigSmart(SimulationConfig config, SimulationStats stats, string strategy)
    {
        double avgTurns = stats.AverageTurnsPerRun;
        var random = new Random();

        if (strategy == "reverse")
        {
            // Last change made things worse - try opposite!
            if (_lastChange.Contains("HP +"))
            {
                config.PlayerStartHP = Math.Max(5, config.PlayerStartHP - 2);
                _lastChange = "HP -2 (reversing)";
            }
            else if (_lastChange.Contains("Detection +"))
            {
                config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                _lastChange = "Detection -1 (reversing)";
            }
            else
            {
                // Try random adjustment
                int adjust = random.Next(3);
                if (adjust == 0)
                {
                    config.PlayerStartHP += 1;
                    _lastChange = "HP +1 (exploring)";
                }
                else if (adjust == 1)
                {
                    config.PlayerDefense += 1;
                    _lastChange = "DEF +1 (exploring)";
                }
                else
                {
                    config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                    _lastChange = "Detection -1 (exploring)";
                }
            }
        }
        else if (strategy == "continue")
        {
            // Last change helped - do more of the same!
            if (_lastChange.Contains("HP"))
            {
                if (_lastChange.Contains("+"))
                {
                    config.PlayerStartHP += 1;
                    _lastChange = "HP +1 (continuing success)";
                }
                else
                {
                    config.PlayerStartHP = Math.Max(5, config.PlayerStartHP - 1);
                    _lastChange = "HP -1 (continuing success)";
                }
            }
            else if (_lastChange.Contains("Detection"))
            {
                if (_lastChange.Contains("+"))
                {
                    config.MobDetectionRange = Math.Min(6, config.MobDetectionRange + 1);
                    _lastChange = "Detection +1 (continuing success)";
                }
                else
                {
                    config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                    _lastChange = "Detection -1 (continuing success)";
                }
            }
            else
            {
                // Initial success, pick direction based on results
                if (avgTurns < 45)
                {
                    config.PlayerStartHP += 1;
                    _lastChange = "HP +1 (below target)";
                }
                else if (avgTurns > 55)
                {
                    config.MobDetectionRange = Math.Min(6, config.MobDetectionRange + 1);
                    _lastChange = "Detection +1 (above target)";
                }
            }
        }
        else // "explore"
        {
            // Stable score - try small perturbations to find improvements
            // Hill climbing: try one change at a time
            if (_cyclesRun % 5 == 0)
            {
                config.PlayerStartHP += 1;
                _lastChange = "HP +1 (exploring)";
            }
            else if (_cyclesRun % 5 == 1)
            {
                config.PlayerStartHP = Math.Max(5, config.PlayerStartHP - 1);
                _lastChange = "HP -1 (exploring)";
            }
            else if (_cyclesRun % 5 == 2)
            {
                config.MobDetectionRange = Math.Min(6, config.MobDetectionRange + 1);
                _lastChange = "Detection +1 (exploring)";
            }
            else if (_cyclesRun % 5 == 3)
            {
                config.MobDetectionRange = Math.Max(2, config.MobDetectionRange - 1);
                _lastChange = "Detection -1 (exploring)";
            }
            else
            {
                config.MaxMobs = config.MaxMobs + random.Next(-2, 3);
                config.MaxMobs = Math.Clamp(config.MaxMobs, 10, 30);
                _lastChange = "MaxMobs adjust (exploring)";
            }
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
