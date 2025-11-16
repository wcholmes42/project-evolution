namespace ProjectEvolution.Game;

// 🦄 X-MEN MUTATION MODE - EXTREME EXPLORATION FOR UNICORN CONFIGS!
public class XMenMutationMode
{
    public static void UnleashTheMutants()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     🦄 X-MEN MUTATION MODE - UNLEASH THE CHAOS! 🦄            ║");
        Console.WriteLine("║     Extreme mutations, wild configs, unicorn hunting!          ║");
        Console.WriteLine("║     Press ESC to stop the madness                              ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        var random = new Random();
        var topUnicorns = new List<(SimulationConfig config, double score, double turns, string mutation)>();
        int mutations = 0;
        double bestScore = 0;

        bool running = true;
        while (running)
        {
            mutations++;

            // EXTREME RANDOM MUTATIONS!
            int mutationType = random.Next(5);

            SimulationConfig config = mutationType switch
            {
                0 => GenerateGlassCannon(random),      // Ultra low HP, ultra high damage
                1 => GenerateSwarmMode(random),        // MASSIVE mob counts
                2 => GenerateHunterMode(random),       // Extreme detection ranges
                3 => GenerateTankMode(random),         // High HP/Defense
                4 => GenerateChaosMode(random),        // Completely random!
                _ => GenerateChaosMode(random)
            };

            // Test the mutant!
            var simulator = new GameSimulator(config);
            var stats = simulator.RunSimulation(500); // BIG sample for rare configs

            double avgTurns = stats.AverageTurnsPerRun;

            // Track top unicorns
            string mutName = mutationType switch
            {
                0 => "🗡️ GLASS CANNON",
                1 => "🐝 SWARM",
                2 => "👁️ HUNTER",
                3 => "🛡️ TANK",
                _ => "🎲 CHAOS"
            };

            // ADJUSTED: Target based on empirical data (averaging ~110 turns)
            double targetTurns = 110.0; // REALISTIC based on your 437K games!

            // MULTI-OBJECTIVE SCORING: Balance FIRST, then Diversity!
            double balanceScore = 100 - Math.Abs(avgTurns - targetTurns) * 2; // How balanced (0-100)
            double diversityBonus = CalculateDiversityBonus(topUnicorns, mutName); // Class diversity (-8 to +8)
            double viabilityScore = avgTurns > 30 && avgTurns < 200 ? 10 : 0; // Viable range bonus

            // REBALANCED: Balance (85%) + Diversity (10%) + Viability (5%)
            // Balance is PRIMARY, diversity is tiebreaker!
            double finalScore = (balanceScore * 0.85) + ((diversityBonus + 8) * 1.25) + (viabilityScore * 0.5);
            // Diversity now contributes max 20 points (was 60!)

            topUnicorns.Add((config, finalScore, avgTurns, mutName));
            topUnicorns = topUnicorns.OrderByDescending(u => u.score).Take(10).ToList();

            // Display
            Console.Clear();
            DrawUnicornDisplay(mutations, topUnicorns, config, stats, finalScore, mutName);

            // Save if unicorn found!
            if (finalScore > bestScore)
            {
                bestScore = finalScore;
                ConfigPersistence.SaveOptimalConfig(config, stats, finalScore, mutations * 500);

                Console.SetCursorPosition(2, 20);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"🦄 UNICORN FOUND! Score {finalScore:F1} - SAVED!                              ");
                Console.ResetColor();
                Thread.Sleep(1500);
            }

            // Check ESC
            if (Console.KeyAvailable && Console.ReadKey(intercept: true).Key == ConsoleKey.Escape)
            {
                running = false;
            }

            Thread.Sleep(100);
        }

        Console.Clear();
        PrintUnicornSummary(topUnicorns, mutations);
    }

    private static double CalculateDiversityBonus(List<(SimulationConfig config, double score, double turns, string mutation)> topConfigs, string newMutationType)
    {
        if (topConfigs.Count < 3) return 0; // Not enough data yet

        // Count how many of each type in top 5
        var top5 = topConfigs.Take(5).ToList();
        var typeCounts = top5.GroupBy(u => u.mutation).ToDictionary(g => g.Key, g => g.Count());

        // If this type is underrepresented, give bonus!
        int currentTypeCount = typeCounts.ContainsKey(newMutationType) ? typeCounts[newMutationType] : 0;

        // Diversity bonus: REDUCED impact - tiebreaker only!
        double bonus = currentTypeCount switch
        {
            0 => 4.0,  // Not in top 5 - moderate boost
            1 => 2.0,  // Only 1 in top 5, slight boost
            2 => 0.0,  // Balanced representation
            3 => -2.0, // Overrepresented, slight penalty
            _ => -4.0  // Dominated top 5, penalty
        };

        return bonus;
    }

    private static SimulationConfig GenerateGlassCannon(Random random)
    {
        return new SimulationConfig
        {
            MobDetectionRange = random.Next(6, 11),  // High detection
            MaxMobs = random.Next(40, 71),           // Lots of mobs
            PlayerStartHP = random.Next(3, 6),       // VERY low HP!
            PlayerStrength = random.Next(3, 7),      // HIGH damage!
            PlayerDefense = random.Next(0, 2),       // Low defense
            MinMobs = 5,
            ShowVisuals = false
        };
    }

    private static SimulationConfig GenerateSwarmMode(Random random)
    {
        return new SimulationConfig
        {
            MobDetectionRange = random.Next(3, 8),
            MaxMobs = random.Next(60, 101),          // INSANE mob counts!
            PlayerStartHP = random.Next(8, 13),
            PlayerStrength = random.Next(2, 4),
            PlayerDefense = random.Next(1, 4),
            MinMobs = random.Next(20, 31),           // High minimum too!
            ShowVisuals = false
        };
    }

    private static SimulationConfig GenerateHunterMode(Random random)
    {
        return new SimulationConfig
        {
            MobDetectionRange = random.Next(8, 16),  // EXTREME detection!
            MaxMobs = random.Next(20, 41),
            PlayerStartHP = random.Next(5, 11),
            PlayerStrength = random.Next(2, 4),
            PlayerDefense = random.Next(1, 3),
            MinMobs = 5,
            ShowVisuals = false
        };
    }

    private static SimulationConfig GenerateTankMode(Random random)
    {
        return new SimulationConfig
        {
            MobDetectionRange = random.Next(5, 11),
            MaxMobs = random.Next(30, 61),
            PlayerStartHP = random.Next(15, 26),     // MEGA HP!
            PlayerStrength = random.Next(1, 3),      // Low damage
            PlayerDefense = random.Next(3, 8),       // MEGA defense!
            MinMobs = 5,
            ShowVisuals = false
        };
    }

    private static SimulationConfig GenerateChaosMode(Random random)
    {
        return new SimulationConfig
        {
            MobDetectionRange = random.Next(2, 16),  // Anything goes!
            MaxMobs = random.Next(10, 101),
            PlayerStartHP = random.Next(3, 26),
            PlayerStrength = random.Next(1, 7),
            PlayerDefense = random.Next(0, 8),
            MinMobs = random.Next(5, 21),
            ShowVisuals = false
        };
    }

    private static void DrawUnicornDisplay(int mutations, List<(SimulationConfig config, double score, double turns, string mutation)> unicorns,
        SimulationConfig current, SimulationStats stats, double score, string mutName)
    {
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║          🦄 UNICORN HUNT - X-MEN MUTATION MODE 🦄             ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.WriteLine($"\nMutations Tested: {mutations}  |  Games Simulated: {mutations * 500:N0}");
        Console.WriteLine($"\nCurrent Mutant: {mutName}");
        Console.WriteLine($"Config: Det={current.MobDetectionRange} Mobs={current.MaxMobs} HP={current.PlayerStartHP} STR={current.PlayerStrength} DEF={current.PlayerDefense}");
        Console.WriteLine($"Result: {stats.AverageTurnsPerRun:F1} turns, Score: {score:F1}");

        // Show class distribution and DIVERSITY GOAL
        if (unicorns.Count >= 5)
        {
            var top5Types = unicorns.Take(5).GroupBy(u => u.mutation).Select(g => $"{g.Key}({g.Count()})");
            var uniqueTypes = unicorns.Take(5).Select(u => u.mutation).Distinct().Count();

            Console.ForegroundColor = uniqueTypes >= 4 ? ConsoleColor.Green : uniqueTypes >= 3 ? ConsoleColor.Yellow : ConsoleColor.Red;
            Console.WriteLine($"\n⚖️ META DIVERSITY: {uniqueTypes}/5 classes in top 5 | {string.Join(", ", top5Types)}");
            Console.ResetColor();

            if (uniqueTypes >= 4)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("✅ HEALTHY META - All playstyles viable!");
                Console.ResetColor();
            }
        }

        Console.WriteLine("\n🏆 TOP 10 UNICORNS DISCOVERED:");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");

        for (int i = 0; i < Math.Min(10, unicorns.Count); i++)
        {
            var (cfg, sc, trns, mut) = unicorns[i];

            ConsoleColor color = sc switch
            {
                >= 92 => ConsoleColor.Green,
                >= 85 => ConsoleColor.DarkGreen,
                >= 70 => ConsoleColor.Yellow,
                _ => ConsoleColor.Red
            };

            Console.ForegroundColor = color;
            string scoreLabel = sc >= 92 ? "🎯" : sc >= 85 ? "✅" : sc >= 70 ? "👍" : "⚠️";
            Console.WriteLine($"{i+1,2}. {scoreLabel} {sc,5:F1} | {trns,5:F1}t | {mut,-15} | Det{cfg.MobDetectionRange,2} Mobs{cfg.MaxMobs,3} HP{cfg.PlayerStartHP,2} STR{cfg.PlayerStrength} DEF{cfg.PlayerDefense}");
            Console.ResetColor();
        }

        Console.WriteLine("\n[ESC] to stop | 🎲 Generating random mutants...");
    }

    private static void PrintUnicornSummary(List<(SimulationConfig config, double score, double turns, string mutation)> unicorns, int mutations)
    {
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              🦄 UNICORN HUNT COMPLETE! 🦄                      ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine($"Total Mutations Tested: {mutations}");
        Console.WriteLine($"Total Games Simulated: {mutations * 500:N0}\n");

        if (unicorns.Count > 0)
        {
            var best = unicorns[0];
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("🏆 BEST UNICORN FOUND:");
            Console.WriteLine($"  Type: {best.mutation}");
            Console.WriteLine($"  Score: {best.score:F1}");
            Console.WriteLine($"  Avg Turns: {best.turns:F1}");
            Console.WriteLine($"  Config: Det={best.config.MobDetectionRange} Mobs={best.config.MaxMobs} HP={best.config.PlayerStartHP} STR={best.config.PlayerStrength} DEF={best.config.PlayerDefense}");
            Console.ResetColor();
        }

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
