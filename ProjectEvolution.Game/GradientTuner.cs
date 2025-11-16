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
    private static double _bestScore = 0;
    private static int _cyclesSinceImprovement = 0;
    private static double _mutationVariance = 1.0; // Adaptive exploration strength
    private static List<double> _recentScores = new List<double>(); // Last 10 for progress rate

    private class LeaderboardEntry
    {
        public int Cycle { get; set; }
        public FloatConfig Config { get; set; } = null!;
        public double AvgTurns { get; set; }
        public double Score { get; set; }
        public int Combats { get; set; }
    }

    private static List<LeaderboardEntry> _leaderboard = new List<LeaderboardEntry>();

    public static void RunGradientTuning()
    {
        Console.Clear();
        Console.CursorVisible = false;

        // Load saved optimal config or use defaults
        var savedConfig = ConfigPersistence.LoadOptimalConfig();

        // CRITICAL: Initialize _bestScore from saved config!
        if (savedConfig != null && File.Exists("optimal_config.json"))
        {
            var savedOptimal = System.Text.Json.JsonSerializer.Deserialize<OptimalConfig>(File.ReadAllText("optimal_config.json"));
            if (savedOptimal != null)
            {
                _bestScore = savedOptimal.Score;
                Console.WriteLine($"📊 Previous best score: {_bestScore:F1} - must beat this to save!");
                Thread.Sleep(2000);
            }
        }

        var config = savedConfig != null ? new FloatConfig
        {
            MobDetectionRange = savedConfig.MobDetectionRange,
            MaxMobs = savedConfig.MaxMobs,
            MinMobs = savedConfig.MinMobs,
            PlayerStartHP = savedConfig.PlayerStartHP,
            PlayerStrength = savedConfig.PlayerStrength,
            PlayerDefense = savedConfig.PlayerDefense
        } : new FloatConfig
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
            // FIX #1: MUCH larger sample size for stable gradients
            var simulator = new GameSimulator(config.ToSimConfig());
            var stats = simulator.RunSimulation(300); // TRIPLED: Reduce variance!

            double avgTurns = stats.AverageTurnsPerRun;
            double error = avgTurns - _targetScore; // How far from ideal?
            double score = 100 - Math.Abs(error) * 2; // Score out of 100

            _scoreHistory.Add(avgTurns);
            _recentScores.Add(score);
            if (_recentScores.Count > 10) _recentScores.RemoveAt(0);

            // Calculate progress rate (improvement over last 10 cycles)
            double progressRate = 0;
            if (_recentScores.Count >= 5)
            {
                double firstHalf = _recentScores.Take(5).Average();
                double secondHalf = _recentScores.Skip(_recentScores.Count - 5).Average();
                progressRate = secondHalf - firstHalf; // Positive = improving
            }

            // ADAPTIVE MUTATION based on progress rate (not just best score!)
            if (progressRate > 1.0) // Good progress!
            {
                _cyclesSinceImprovement = 0;
                _mutationVariance = Math.Max(0.3, _mutationVariance * 0.85); // Cool down (fine-tune)
            }
            else if (progressRate < -1.0) // Regressing!
            {
                _cyclesSinceImprovement++;
                _mutationVariance = Math.Min(8.0, _mutationVariance * 1.5); // Heat up! (explore more)
            }
            else if (Math.Abs(progressRate) < 0.5) // Stuck/plateaued
            {
                _cyclesSinceImprovement++;
                if (_cyclesSinceImprovement > 3)
                {
                    _mutationVariance = Math.Min(8.0, _mutationVariance * 1.4); // Stuck → heat up!
                }
            }
            else
            {
                // Moderate progress - maintain current variance
                _cyclesSinceImprovement = Math.Max(0, _cyclesSinceImprovement - 1);
            }

            // Add to leaderboard
            var entry = new LeaderboardEntry
            {
                Cycle = _cyclesRun,
                Config = CloneConfig(config),
                AvgTurns = avgTurns,
                Score = score,
                Combats = (int)stats.AverageCombatsWon
            };

            _leaderboard.Add(entry);
            _leaderboard = _leaderboard.OrderByDescending(e => e.Score).Take(10).ToList();

            // Save ONLY if truly better than previous best!
            if (score > _bestScore)
            {
                var oldBest = _bestScore;
                _bestScore = score;

                Console.SetCursorPosition(2, 25);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.Write($"💾 NEW RECORD! {oldBest:F1} → {score:F1} - SAVING...                                      ");
                Console.ResetColor();

                ConfigPersistence.SaveOptimalConfig(config.ToSimConfig(), stats, score, _cyclesRun * 300);

                Thread.Sleep(1000); // Let user see the save
            }

            // Update display with progress metrics
            UpdateGradientDisplay(config, stats, error, progressRate);

            // Calculate gradients (estimate parameter impact)
            if (_cyclesRun > 1)
            {
                double delta = avgTurns - lastAvgTurns;
                EstimateGradients(config, delta, error);
            }

            // Apply gradient descent with EXPLORATION
            if (_cyclesRun > 2) // Need 2+ cycles to estimate gradients
            {
                // Debug: log before adjustment
                var beforeConfig = $"Det={config.MobDetectionRange:F1} Mobs={config.MaxMobs:F1} HP={config.PlayerStartHP:F1} Def={config.PlayerDefense:F1}";

                // ADAPTIVE EXPLORATION: Vary jump size based on progress!
                if (_cyclesRun % 10 == 0)
                {
                    var random = new Random();

                    // Use mutation variance (increases when stuck, decreases when improving)
                    int detJump = (int)(_mutationVariance * random.Next(-2, 3));
                    int mobsJump = (int)(_mutationVariance * random.Next(-8, 9));
                    int hpJump = (int)(_mutationVariance * random.Next(-3, 4));
                    int defJump = (int)(_mutationVariance * random.Next(-1, 2));

                    config.MobDetectionRange += detJump;
                    config.MaxMobs += mobsJump;
                    config.PlayerStartHP += hpJump;
                    config.PlayerDefense += defJump;

                    // Clamp with EXPANDED RANGES!
                    config.MobDetectionRange = Math.Clamp(config.MobDetectionRange, 2, 10);
                    config.MaxMobs = Math.Clamp(config.MaxMobs, 10, 60);
                    config.PlayerStartHP = Math.Clamp(config.PlayerStartHP, 3, 20);
                    config.PlayerDefense = Math.Clamp(config.PlayerDefense, 0, 5);
                }
                else
                {
                    ApplyGradientDescent(config, error);
                }

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
            // CRITICAL FIX: SIGNS WERE BACKWARDS!
            // Positive error = too easy → need to make HARDER
            // Negative error = too hard → need to make EASIER

            // MobDetection: Higher detection → harder → fewer turns
            // If too easy (+error): INCREASE detection (+ gradient)
            _gradients["MobDetection"] = alpha * (+error / 3.0) + (1 - alpha) * _gradients["MobDetection"];

            // MaxMobs: More mobs → harder → fewer turns
            // If too easy (+error): INCREASE mobs (+ gradient)
            _gradients["MaxMobs"] = alpha * (+error / 8.0) + (1 - alpha) * _gradients["MaxMobs"];

            // PlayerHP: More HP → easier → more turns
            // If too easy (+error): DECREASE HP (- gradient)
            _gradients["PlayerHP"] = alpha * (-error / 2.0) + (1 - alpha) * _gradients["PlayerHP"];

            // PlayerDefense: More defense → easier → more turns
            // If too easy (+error): DECREASE defense (- gradient)
            _gradients["PlayerDefense"] = alpha * (-error / 4.0) + (1 - alpha) * _gradients["PlayerDefense"];
        }
    }

    private static void ApplyGradientDescent(FloatConfig config, double error)
    {
        // Apply gradients with momentum
        double errorMagnitude = Math.Abs(error);

        // Adaptive learning rate: larger errors = larger adjustments
        // INCREASED: errorMagnitude/20 → errorMagnitude/10
        double adaptiveLR = _learningRate * Math.Min(1.0, errorMagnitude / 10.0);

        // Update each parameter with EXPANDED RANGES to break the barrier!
        config.MobDetectionRange = UpdateParameter(config.MobDetectionRange, "MobDetection", adaptiveLR, 2, 10); // Was 6, now 10!
        config.MaxMobs = UpdateParameter(config.MaxMobs, "MaxMobs", adaptiveLR, 10, 60); // Was 40, now 60!
        config.PlayerStartHP = UpdateParameter(config.PlayerStartHP, "PlayerHP", adaptiveLR, 3, 20); // Was 5, now 3!
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
        Console.WriteLine("║ 🏆 TOP 10 LEADERBOARD (Best Balanced Configs):                            ║");
        Console.WriteLine("║ #  Cycle  Score   AvgTurns  Det Mobs HP Def  Combats                      ║");
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine("║                                                                            ║");
        }
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ GRADIENTS (Parameter Impact):                                             ║");
        Console.WriteLine("║   MobDetection:                                                            ║");
        Console.WriteLine("║   MaxMobs:                                                                 ║");
        Console.WriteLine("║   PlayerHP:                                                                ║");
        Console.WriteLine("║   PlayerDef:                                                               ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ PROGRESS TRACKING:                                                         ║");
        Console.WriteLine("║ SAVE STATUS:                                                               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════════════════╝");
    }

    private static void UpdateGradientDisplay(FloatConfig config, SimulationStats stats, double error, double progressRate)
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

        // Draw leaderboard
        DrawLeaderboard();

        // Gradients
        Console.SetCursorPosition(2, 19);
        Console.Write($"  MobDetection: {_gradients["MobDetection"],+6:F3}  (impact on turns)                              ");
        Console.SetCursorPosition(2, 20);
        Console.Write($"  MaxMobs:      {_gradients["MaxMobs"],+6:F3}                                                 ");
        Console.SetCursorPosition(2, 21);
        Console.Write($"  PlayerHP:     {_gradients["PlayerHP"],+6:F3}                                                 ");
        Console.SetCursorPosition(2, 22);
        Console.Write($"  PlayerDef:    {_gradients["PlayerDefense"],+6:F3}                                                 ");

        // Progress tracking with color coding
        Console.SetCursorPosition(2, 24);

        // Color based on progress
        if (progressRate > 1.0)
            Console.ForegroundColor = ConsoleColor.Green; // Excellent progress!
        else if (progressRate > 0)
            Console.ForegroundColor = ConsoleColor.DarkGreen; // Some progress
        else if (progressRate > -1.0)
            Console.ForegroundColor = ConsoleColor.Yellow; // Stagnant
        else
            Console.ForegroundColor = ConsoleColor.Red; // Regressing!

        string status = progressRate > 1 ? "🚀 IMPROVING" : progressRate < -1 ? "⚠️ REGRESSING" : progressRate > 0 ? "↗️ Progress" : "━ Stuck";

        Console.Write($"{status} Rate:{progressRate,+5:F1} | NoImprove:{_cyclesSinceImprovement,2} | Mutation:{_mutationVariance,4:F1}x            ");
        Console.ResetColor();
    }

    private static void DrawLeaderboard()
    {
        int startY = 9;

        for (int i = 0; i < 10; i++)
        {
            Console.SetCursorPosition(2, startY + i);

            if (i < _leaderboard.Count)
            {
                var entry = _leaderboard[i];

                // Color by rank
                if (i == 0)
                    Console.ForegroundColor = ConsoleColor.Yellow; // Gold
                else if (i == 1)
                    Console.ForegroundColor = ConsoleColor.Gray; // Silver
                else if (i == 2)
                    Console.ForegroundColor = ConsoleColor.DarkYellow; // Bronze
                else
                    Console.ForegroundColor = ConsoleColor.White;

                string rank = (i + 1).ToString().PadLeft(2);
                string cycle = entry.Cycle.ToString().PadLeft(5);
                string score = entry.Score.ToString("F1").PadLeft(6);
                string turns = entry.AvgTurns.ToString("F1").PadLeft(8);
                string det = entry.Config.MobDetectionRange.ToString("F0").PadLeft(3);
                string mobs = entry.Config.MaxMobs.ToString("F0").PadLeft(4);
                string hp = entry.Config.PlayerStartHP.ToString("F0").PadLeft(2);
                string def = entry.Config.PlayerDefense.ToString("F0").PadLeft(3);
                string combats = entry.Combats.ToString().PadLeft(7);

                Console.Write($"{rank} {cycle} {score}  {turns}   {det} {mobs} {hp}  {def}  {combats}                  ");
                Console.ResetColor();
            }
            else
            {
                Console.Write("                                                                            ");
            }
        }
    }

    private static FloatConfig CloneConfig(FloatConfig config)
    {
        return new FloatConfig
        {
            MobDetectionRange = config.MobDetectionRange,
            MaxMobs = config.MaxMobs,
            MinMobs = config.MinMobs,
            PlayerStartHP = config.PlayerStartHP,
            PlayerStrength = config.PlayerStrength,
            PlayerDefense = config.PlayerDefense
        };
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
