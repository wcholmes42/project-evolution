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
            double score = 100 - Math.Abs(avgTurns - 75) * 2;

            // Track top unicorns
            string mutName = mutationType switch
            {
                0 => "🗡️ GLASS CANNON",
                1 => "🐝 SWARM",
                2 => "👁️ HUNTER",
                3 => "🛡️ TANK",
                _ => "🎲 CHAOS"
            };

            topUnicorns.Add((config, score, avgTurns, mutName));
            topUnicorns = topUnicorns.OrderByDescending(u => u.score).Take(10).ToList();

            // Display
            Console.Clear();
            DrawUnicornDisplay(mutations, topUnicorns, config, stats, score, mutName);

            // Save if unicorn found!
            if (score > bestScore)
            {
                bestScore = score;
                ConfigPersistence.SaveOptimalConfig(config, stats, score, mutations * 500);

                Console.SetCursorPosition(2, 20);
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"🦄 UNICORN FOUND! Score {score:F1} - SAVED!                              ");
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
