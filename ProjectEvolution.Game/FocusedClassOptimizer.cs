namespace ProjectEvolution.Game;

// Systematic class boosting - fight each to the top!
public class FocusedClassOptimizer
{
    private static Dictionary<string, List<double>> _classBestScores = new Dictionary<string, List<double>>();
    private static int _mutationsPerClass = 20; // Focus on each class for 20 mutations
    private static int _currentMutationInRound = 0;
    private static string _currentFocusClass = "";

    public static void RunFocusedOptimization()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║    🎯 FOCUSED CLASS OPTIMIZER - Boost Weakest to Top!         ║");
        Console.WriteLine("║    Each class gets dedicated optimization rounds               ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        string[] classes = { "🗡️ GLASS CANNON", "🐝 SWARM", "👁️ HUNTER", "🛡️ TANK", "🎲 CHAOS" };
        int totalMutations = 0;
        var allResults = new List<(SimulationConfig config, double score, double turns, string className)>();

        // Initialize tracking
        foreach (var className in classes)
        {
            _classBestScores[className] = new List<double>();
        }

        bool running = true;
        int round = 0;

        while (running)
        {
            round++;

            // Determine weakest class (skip if tested 100+ times and still bad!)
            string weakestClass = FindWeakestClass(classes, allResults);

            // VIABILITY BRIDGE: If class tested 100+ times with best < 50, find path to viability!
            var classResults = allResults.Where(r => r.className == weakestClass).ToList();
            if (classResults.Count >= 100)
            {
                var classBest = classResults.Any() ? classResults.Max(r => r.score) : 0;
                if (classBest < 50)
                {
                    Console.ForegroundColor = ConsoleColor.Cyan;
                    Console.WriteLine($"\n🔍 {weakestClass} non-viable after {classResults.Count} tests (best: {classBest:F1})");
                    Console.WriteLine($"   Finding path to viability by analyzing successful classes...\n");
                    Console.ResetColor();

                    // Find best viable class
                    var viableClasses = classes.Where(c => c != weakestClass).ToList();
                    var viableResults = allResults.Where(r => viableClasses.Contains(r.className) && r.score > 80).ToList();

                    if (viableResults.Any())
                    {
                        var viableTarget = viableResults.OrderByDescending(r => r.score).First();
                        var failedConfig = classResults.OrderByDescending(r => r.score).First().config;

                        Console.WriteLine($"   FAILED {weakestClass}: Det={failedConfig.MobDetectionRange} Mobs={failedConfig.MaxMobs} HP={failedConfig.PlayerStartHP} STR={failedConfig.PlayerStrength} DEF={failedConfig.PlayerDefense}");
                        Console.WriteLine($"   TARGET {viableTarget.className}: Det={viableTarget.config.MobDetectionRange} Mobs={viableTarget.config.MaxMobs} HP={viableTarget.config.PlayerStartHP} STR={viableTarget.config.PlayerStrength} DEF={viableTarget.config.PlayerDefense}");

                        // Generate bridge configs (interpolate toward viable)
                        Console.WriteLine($"\n   Testing 20 BRIDGE configs from {weakestClass} → {viableTarget.className}...\n");

                        for (int b = 0; b < 20; b++)
                        {
                            double alpha = b / 20.0; // 0 to 1
                            var bridgeConfig = new SimulationConfig
                            {
                                MobDetectionRange = (int)(failedConfig.MobDetectionRange * (1 - alpha) + viableTarget.config.MobDetectionRange * alpha),
                                MaxMobs = (int)(failedConfig.MaxMobs * (1 - alpha) + viableTarget.config.MaxMobs * alpha),
                                PlayerStartHP = (int)(failedConfig.PlayerStartHP * (1 - alpha) + viableTarget.config.PlayerStartHP * alpha),
                                PlayerStrength = (int)(failedConfig.PlayerStrength * (1 - alpha) + viableTarget.config.PlayerStrength * alpha),
                                PlayerDefense = (int)(failedConfig.PlayerDefense * (1 - alpha) + viableTarget.config.PlayerDefense * alpha),
                                MinMobs = 5,
                                ShowVisuals = false
                            };

                            var sim = new GameSimulator(bridgeConfig);
                            var bridgeStats = sim.RunSimulation(200);
                            double bridgeScore = 100 - Math.Abs(bridgeStats.AverageTurnsPerRun - 110) * 2;

                            allResults.Add((bridgeConfig, bridgeScore, bridgeStats.AverageTurnsPerRun, weakestClass));

                            if (bridgeScore > classBest)
                            {
                                Console.ForegroundColor = ConsoleColor.Green;
                                Console.WriteLine($"   ✅ Bridge {b}/20: {bridgeScore:F1} (improved from {classBest:F1}!)");
                                Console.ResetColor();
                                classBest = bridgeScore;

                                if (bridgeScore > 80)
                                {
                                    ConfigPersistence.SaveOptimalConfig(bridgeConfig, bridgeStats, bridgeScore, totalMutations * 300);
                                    Console.WriteLine($"   💾 Viable {weakestClass} config found!");
                                }
                            }
                            else
                            {
                                Console.Write($"\r   Bridge {b}/20: {bridgeScore:F1}");
                            }
                        }

                        Console.WriteLine($"\n\n   Bridge search complete. Best {weakestClass}: {classBest:F1}");
                        Thread.Sleep(2000);

                        // Continue with this class using improved ranges
                    }
                }
            }

            _currentFocusClass = weakestClass;
            _currentMutationInRound = 0;

            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine($"\n╔══════════════════════════════════════════════════════════════╗");
            Console.WriteLine($"║  ROUND {round}: BOOSTING {weakestClass.PadRight(20)}            ║");
            Console.WriteLine($"╚══════════════════════════════════════════════════════════════╝");
            Console.ResetColor();

            ShowClassStandings(classes, allResults);

            Console.WriteLine($"\nTesting {_mutationsPerClass} mutations of {weakestClass}...\n");

            // Focus on this class for N mutations
            for (int i = 0; i < _mutationsPerClass; i++)
            {
                _currentMutationInRound++;
                totalMutations++;

                // Generate config for focused class
                var config = GenerateConfigForClass(weakestClass);

                // Test it (parallel!)
                var simulator = new GameSimulator(config);
                var stats = simulator.RunSimulation(300);

                double avgTurns = stats.AverageTurnsPerRun;
                double score = 100 - Math.Abs(avgTurns - 110) * 2;

                allResults.Add((config, score, avgTurns, weakestClass));
                _classBestScores[weakestClass].Add(score);

                // Display progress
                var currentBest = allResults.Where(r => r.className == weakestClass).OrderByDescending(r => r.score).First();
                Console.Write($"\r{i+1}/{_mutationsPerClass}: Best {weakestClass} = {currentBest.score:F1} ({currentBest.turns:F1}t)    ");

                // Save if overall best
                var overallBest = allResults.OrderByDescending(r => r.score).First();
                if (score >= overallBest.score - 1)
                {
                    ConfigPersistence.SaveOptimalConfig(config, stats, score, totalMutations * 300);
                    Console.Write("💾 ");
                }

                // Check ESC
                if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
                {
                    running = false;
                    break;
                }
            }

            Console.WriteLine($"\n\n✅ Round {round} complete for {weakestClass}!");
            Thread.Sleep(1000);

            if (!running) break;
        }

        Console.Clear();
        PrintFinalClassStandings(classes, allResults, totalMutations);
    }

    private static string FindWeakestClass(string[] classes, List<(SimulationConfig config, double score, double turns, string className)> results)
    {
        if (results.Count < 10) return classes[0]; // Not enough data, start with first

        // Find class with worst best score in top 20
        var top20 = results.OrderByDescending(r => r.score).Take(20).ToList();
        var classCounts = new Dictionary<string, int>();
        var classBestScores = new Dictionary<string, double>();
        var classTestCounts = new Dictionary<string, int>();

        foreach (var className in classes)
        {
            var classResults = top20.Where(r => r.className == className).ToList();
            var allClassResults = results.Where(r => r.className == className).ToList();

            classCounts[className] = classResults.Count;
            classBestScores[className] = classResults.Any() ? classResults.Max(r => r.score) : 0;
            classTestCounts[className] = allClassResults.Count;
        }

        // IMPROVED LOGIC: Don't keep testing already-strong classes!
        // Filter out classes that are "good enough" (score > 95 AND 2+ in top 10)
        var needsWork = classes.Where(c =>
        {
            var bestScore = classBestScores.GetValueOrDefault(c, 0);
            var inTop10 = classCounts.GetValueOrDefault(c, 0);

            // Good enough? Skip it!
            if (bestScore > 95 && inTop10 >= 2) return false;

            // Way overtested? Skip it!
            if (classTestCounts[c] > 200 && bestScore > 90) return false;

            return true; // Needs work!
        }).ToArray();

        // If all classes are good, use round-robin
        if (needsWork.Length == 0)
        {
            // Round-robin: pick least tested
            return classes.OrderBy(c => classTestCounts.GetValueOrDefault(c, 0)).First();
        }

        // Among classes that need work, pick weakest
        return needsWork.OrderBy(c => classCounts.GetValueOrDefault(c, 0))
                       .ThenBy(c => classBestScores.GetValueOrDefault(c, 0))
                       .First();
    }

    private static void ShowClassStandings(string[] classes, List<(SimulationConfig, double, double, string)> results)
    {
        Console.WriteLine("\n📊 CURRENT CLASS STANDINGS:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        var top10 = results.OrderByDescending(r => r.Item2).Take(10).ToList();

        foreach (var className in classes)
        {
            var classResults = results.Where(r => r.Item4 == className).ToList();
            var inTop10 = top10.Count(r => r.Item4 == className);
            var bestScore = classResults.Any() ? classResults.Max(r => r.Item2) : 0;
            var tested = classResults.Count;

            ConsoleColor color = inTop10 >= 2 ? ConsoleColor.Green : inTop10 == 1 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.ForegroundColor = color;

            string status = inTop10 >= 2 ? "✅ Strong" : inTop10 == 1 ? "⚠️  Viable" : "❌ Weak";
            Console.WriteLine($"{className,-20} {status,-12} Top10:{inTop10}/10  Best:{bestScore,5:F1}  Tested:{tested,4}");
            Console.ResetColor();
        }
    }

    private static SimulationConfig GenerateConfigForClass(string className)
    {
        var random = new Random();

        return className switch
        {
            "🗡️ GLASS CANNON" => new SimulationConfig
            {
                MobDetectionRange = random.Next(6, 11),
                MaxMobs = random.Next(40, 71),
                PlayerStartHP = random.Next(3, 6),
                PlayerStrength = random.Next(3, 7),
                PlayerDefense = random.Next(0, 2),
                MinMobs = 5,
                ShowVisuals = false
            },

            "🐝 SWARM" => new SimulationConfig
            {
                MobDetectionRange = random.Next(4, 9),
                MaxMobs = random.Next(45, 71),
                PlayerStartHP = random.Next(9, 14),
                PlayerStrength = random.Next(2, 4),
                PlayerDefense = random.Next(0, 3),
                MinMobs = random.Next(10, 21),
                ShowVisuals = false
            },

            "👁️ HUNTER" => new SimulationConfig
            {
                MobDetectionRange = random.Next(6, 11),
                MaxMobs = random.Next(30, 51),
                PlayerStartHP = random.Next(7, 13),
                PlayerStrength = random.Next(2, 4),
                PlayerDefense = random.Next(0, 2),
                MinMobs = 5,
                ShowVisuals = false
            },

            "🛡️ TANK" => new SimulationConfig
            {
                MobDetectionRange = random.Next(6, 11),  // HIGHER to compensate for tankiness!
                MaxMobs = random.Next(50, 71),            // MORE mobs to challenge tank!
                PlayerStartHP = random.Next(10, 15),      // REDUCED: Was 12-19 (too tanky!)
                PlayerStrength = random.Next(1, 3),
                PlayerDefense = random.Next(1, 4),        // REDUCED: Was 2-5 (too much armor!)
                MinMobs = random.Next(8, 16),             // Higher baseline threat!
                ShowVisuals = false
            },

            _ => new SimulationConfig // CHAOS
            {
                MobDetectionRange = random.Next(2, 16),
                MaxMobs = random.Next(10, 101),
                PlayerStartHP = random.Next(3, 26),
                PlayerStrength = random.Next(1, 7),
                PlayerDefense = random.Next(0, 8),
                MinMobs = random.Next(5, 21),
                ShowVisuals = false
            }
        };
    }

    private static void PrintFinalClassStandings(string[] classes, List<(SimulationConfig, double, double, string)> results, int totalMutations)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         FOCUSED OPTIMIZATION - FINAL RESULTS                   ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine($"Total Mutations: {totalMutations}");
        Console.WriteLine($"Total Games: {totalMutations * 300:N0}\n");

        ShowClassStandings(classes, results);

        Console.WriteLine("\n🏆 OVERALL TOP 10:");
        var top10 = results.OrderByDescending(r => r.Item2).Take(10);
        int rank = 1;
        foreach (var (cfg, score, turns, className) in top10)
        {
            Console.WriteLine($"{rank}. {score:F1} | {turns:F1}t | {className} | Det{cfg.MobDetectionRange} Mobs{cfg.MaxMobs} HP{cfg.PlayerStartHP} STR{cfg.PlayerStrength} DEF{cfg.PlayerDefense}");
            rank++;
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
