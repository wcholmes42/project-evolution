namespace ProjectEvolution.Game;

public class GradientTuner
{
    private static Dictionary<string, double> _gradients = new Dictionary<string, double>();
    private static Dictionary<string, double> _momentum = new Dictionary<string, double>();
    private static double _learningRate = 2.0; // INCREASED: More aggressive adjustments!
    private static double _momentumFactor = 0.3; // REDUCED: Less smoothing initially
    private static List<double> _scoreHistory = new List<double>();
    private static int _cyclesRun = 0;
    private static double _targetScore = 50.0; // Target: 50 turns average

    public static void RunGradientTuning()
    {
        Console.Clear();
        Console.CursorVisible = false;

        var config = new FloatConfig
        {
            MobDetectionRange = 3.0,
            MaxMobs = 29.0,
            MinMobs = 5.0,
            PlayerStartHP = 9.0,
            PlayerStrength = 2.0,
            PlayerDefense = 1.0
        };

        // Initialize gradients and momentum
        InitializeGradients();

        DrawStaticUI();

        bool running = true;
        double lastAvgTurns = 0;

        while (running)
        {
            _cyclesRun++;

            // Run simulation (PARALLEL for speed!)
            var simulator = new GameSimulator(config.ToSimConfig());
            var stats = simulator.RunSimulation(100); // INCREASED: More samples, runs parallel!

            double avgTurns = stats.AverageTurnsPerRun;
            double error = avgTurns - _targetScore; // How far from ideal?

            _scoreHistory.Add(avgTurns);

            // Update display
            UpdateGradientDisplay(config, stats, error);

            // Calculate gradients (estimate parameter impact)
            if (_cyclesRun > 1)
            {
                double delta = avgTurns - lastAvgTurns;
                EstimateGradients(config, delta, error);
            }

            // Apply gradient descent
            if (_cyclesRun > 2) // Need 2+ cycles to estimate gradients
            {
                // Debug: log before adjustment
                var beforeConfig = $"Det={config.MobDetectionRange:F1} Mobs={config.MaxMobs:F1} HP={config.PlayerStartHP:F1} Def={config.PlayerDefense:F1}";

                ApplyGradientDescent(config, error);

                // Debug: log after adjustment
                var afterConfig = $"Det={config.MobDetectionRange:F1} Mobs={config.MaxMobs:F1} HP={config.PlayerStartHP:F1} Def={config.PlayerDefense:F1}";

                // Show changes on screen (debugging)
                Console.SetCursorPosition(2, 26);
                Console.Write($"DEBUG: Before={beforeConfig}                                               ");
                Console.SetCursorPosition(2, 27);
                Console.Write($"       After ={afterConfig}                                               ");
            }

            lastAvgTurns = avgTurns;

            // Check for ESC
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(intercept: true);
                if (key.Key == ConsoleKey.Escape)
                {
                    running = false;
                }
            }

            Thread.Sleep(100); // Brief pause between cycles
        }

        Console.CursorVisible = true;
        Console.Clear();

        // Get final stats
        var finalSimulator = new GameSimulator(config.ToSimConfig());
        var finalStats = finalSimulator.RunSimulation(30);

        PrintGradientSummary(config, finalStats);
    }

    private static void InitializeGradients()
    {
        _gradients["MobDetection"] = 0;
        _gradients["MaxMobs"] = 0;
        _gradients["PlayerHP"] = 0;
        _gradients["PlayerDefense"] = 0;

        _momentum["MobDetection"] = 0;
        _momentum["MaxMobs"] = 0;
        _momentum["PlayerHP"] = 0;
        _momentum["PlayerDefense"] = 0;
    }

    private static void EstimateGradients(FloatConfig config, double delta, double error)
    {
        // Estimate how much each parameter affects the outcome
        // Positive error = too many turns (too easy)
        // Negative error = too few turns (too hard)

        // Update gradient estimates with exponential moving average
        double alpha = 0.6; // INCREASED: React faster to changes

        if (Math.Abs(error) > 2) // React to smaller errors
        {
            // MobDetection: Higher detection → harder → fewer turns
            // INCREASED sensitivity (/10 → /3)
            _gradients["MobDetection"] = alpha * (-error / 3.0) + (1 - alpha) * _gradients["MobDetection"];

            // MaxMobs: More mobs → harder → fewer turns
            // INCREASED sensitivity (/20 → /8)
            _gradients["MaxMobs"] = alpha * (-error / 8.0) + (1 - alpha) * _gradients["MaxMobs"];

            // PlayerHP: More HP → easier → more turns
            // INCREASED sensitivity (/5 → /2)
            _gradients["PlayerHP"] = alpha * (error / 2.0) + (1 - alpha) * _gradients["PlayerHP"];

            // PlayerDefense: More defense → easier → more turns
            // INCREASED sensitivity (/8 → /4)
            _gradients["PlayerDefense"] = alpha * (error / 4.0) + (1 - alpha) * _gradients["PlayerDefense"];
        }
    }

    private static void ApplyGradientDescent(FloatConfig config, double error)
    {
        // Apply gradients with momentum
        double errorMagnitude = Math.Abs(error);

        // Adaptive learning rate: larger errors = larger adjustments
        // INCREASED: errorMagnitude/20 → errorMagnitude/10
        double adaptiveLR = _learningRate * Math.Min(1.0, errorMagnitude / 10.0);

        // Update each parameter
        config.MobDetectionRange = UpdateParameter(config.MobDetectionRange, "MobDetection", adaptiveLR, 2, 6);
        config.MaxMobs = UpdateParameter(config.MaxMobs, "MaxMobs", adaptiveLR, 10, 40);
        config.PlayerStartHP = UpdateParameter(config.PlayerStartHP, "PlayerHP", adaptiveLR, 5, 20);
        config.PlayerDefense = UpdateParameter(config.PlayerDefense, "PlayerDefense", adaptiveLR, 0, 5);
    }

    private static double UpdateParameter(double param, string name, double lr, double min, double max)
    {
        // Calculate momentum-adjusted update
        double gradient = _gradients[name];
        double momentumValue = _momentum[name];

        // Momentum update: new_momentum = momentum_factor * old_momentum + gradient
        double update = _momentumFactor * momentumValue + (1 - _momentumFactor) * gradient;
        _momentum[name] = update;

        // Apply update with learning rate
        param += lr * update;

        // Clamp to valid range
        return Math.Clamp(param, min, max);
    }


    private static void DrawStaticUI()
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         GRADIENT DESCENT OPTIMIZATION - BACKPROPAGATION STYLE              ║");
        Console.WriteLine("║                    Press ESC to stop                                       ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ CYCLE:           ERROR:           TARGET: 50 turns                         ║");
        Console.WriteLine("║ CURRENT CONFIG:                                                            ║");
        Console.WriteLine("║ RESULTS:                                                                   ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ GRADIENTS (Parameter Impact):                                             ║");
        Console.WriteLine("║   MobDetection:                                                            ║");
        Console.WriteLine("║   MaxMobs:                                                                 ║");
        Console.WriteLine("║   PlayerHP:                                                                ║");
        Console.WriteLine("║   PlayerDef:                                                               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ CONVERGENCE GRAPH (Last 60 Cycles) - Target: 50 turns:                    ║");
        Console.WriteLine("║ 100 ┤                                                                      ║");
        Console.WriteLine("║  80 ┤                                                                      ║");
        Console.WriteLine("║  60 ┤                                                                      ║");
        Console.WriteLine("║  50 ┤ ════════════════════ TARGET ═════════════════════════               ║");
        Console.WriteLine("║  40 ┤                                                                      ║");
        Console.WriteLine("║  20 ┤                                                                      ║");
        Console.WriteLine("║   0 ┤                                                                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");
    }

    private static void UpdateGradientDisplay(FloatConfig config, SimulationStats stats, double error)
    {
        // Cycle and error
        Console.SetCursorPosition(2, 4);
        Console.Write($"CYCLE: {_cyclesRun,4}    ERROR: {error,+6:F1} turns    TARGET: 50 turns                    ");

        // Config
        Console.SetCursorPosition(2, 5);
        Console.Write($"CURRENT CONFIG: Det={config.MobDetectionRange:F1} MaxMobs={config.MaxMobs:F0} HP={config.PlayerStartHP:F0} DEF={config.PlayerDefense:F1}                ");

        // Results
        Console.SetCursorPosition(2, 6);
        Console.ForegroundColor = Math.Abs(error) < 10 ? ConsoleColor.Green : Math.Abs(error) < 20 ? ConsoleColor.Yellow : ConsoleColor.Red;
        Console.Write($"RESULTS: Avg {stats.AverageTurnsPerRun:F1} turns, {stats.AverageCombatsWon:F1} combats                                ");
        Console.ResetColor();

        // Gradients
        Console.SetCursorPosition(2, 9);
        Console.Write($"  MobDetection: {_gradients["MobDetection"],+6:F3}  (impact on turns)                              ");
        Console.SetCursorPosition(2, 10);
        Console.Write($"  MaxMobs:      {_gradients["MaxMobs"],+6:F3}                                                 ");
        Console.SetCursorPosition(2, 11);
        Console.Write($"  PlayerHP:     {_gradients["PlayerHP"],+6:F3}                                                 ");
        Console.SetCursorPosition(2, 12);
        Console.Write($"  PlayerDef:    {_gradients["PlayerDefense"],+6:F3}                                                 ");

        // Draw convergence graph
        DrawConvergenceGraph();
    }

    private static void DrawConvergenceGraph()
    {
        int graphWidth = 60;
        int graphHeight = 7;
        int startX = 7;
        int startY = 15;

        // Clear graph
        for (int y = 0; y < graphHeight; y++)
        {
            Console.SetCursorPosition(startX, startY + y);
            Console.Write(new string(' ', graphWidth));
        }

        var recentScores = _scoreHistory.TakeLast(graphWidth).ToList();
        if (recentScores.Count == 0) return;

        // Draw target line (50 turns)
        int targetY = startY + 3; // Middle of graph
        Console.SetCursorPosition(startX, targetY);
        Console.ForegroundColor = ConsoleColor.DarkGreen;
        Console.Write(new string('═', Math.Min(graphWidth, recentScores.Count)));
        Console.ResetColor();

        // Plot scores
        for (int i = 0; i < recentScores.Count; i++)
        {
            double turns = recentScores[i];
            int x = startX + i;

            // Map turns to graph position (0-100 turns → 7 rows)
            int yPos = startY + (int)((100 - Math.Clamp(turns, 0, 100)) / 100.0 * graphHeight);
            yPos = Math.Clamp(yPos, startY, startY + graphHeight - 1);

            Console.SetCursorPosition(x, yPos);

            // Color based on proximity to target
            double errorAbs = Math.Abs(turns - 50);
            if (errorAbs < 5)
                Console.ForegroundColor = ConsoleColor.Green;
            else if (errorAbs < 15)
                Console.ForegroundColor = ConsoleColor.Yellow;
            else
                Console.ForegroundColor = ConsoleColor.Red;

            Console.Write("█");
            Console.ResetColor();
        }
    }

    private static void PrintGradientSummary(FloatConfig config, SimulationStats? stats)
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      GRADIENT DESCENT OPTIMIZATION - FINAL RESULTS             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
        Console.WriteLine($"Total Cycles: {_cyclesRun}");
        Console.WriteLine($"Total Games: {_cyclesRun * 30}\n");

        if (stats != null)
        {
            Console.WriteLine("FINAL CONFIGURATION:");
            Console.WriteLine($"  Mob Detection: {config.MobDetectionRange:F1}");
            Console.WriteLine($"  Max Mobs: {config.MaxMobs:F0}");
            Console.WriteLine($"  Player HP: {config.PlayerStartHP:F0}");
            Console.WriteLine($"  Defense: {config.PlayerDefense:F1}");
            Console.WriteLine($"\nFinal Performance:");
            Console.WriteLine($"  Average Turns: {stats.AverageTurnsPerRun:F1}");
            Console.WriteLine($"  Error from Target: {Math.Abs(stats.AverageTurnsPerRun - 50):F1} turns");
            Console.WriteLine($"\nLearned Gradients:");
            foreach (var kvp in _gradients)
            {
                Console.WriteLine($"  {kvp.Key}: {kvp.Value:F3}");
            }
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
