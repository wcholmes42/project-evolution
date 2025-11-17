namespace ProjectEvolution.Game;

public class EvolutionaryTuner
{
    private static SimulationConfig _bestConfig = null!;
    private static double _bestFitness = 0;
    private static int _generation = 0;
    private static int _totalTests = 0;
    private static List<(SimulationConfig config, double fitness)> _history = new();

    public static void RunContinuousEvolution()
    {
        Console.Clear();
        Console.CursorVisible = false;

        try
        {
            // Initialize with baseline config
            _bestConfig = new SimulationConfig
            {
                ShowVisuals = false,
                PlayerStartHP = 25,
                PlayerStrength = 3,
                PlayerDefense = 1,
                BaseEnemyHP = 5,
                BaseEnemyDamage = 2,
                EnemyHPScaling = 1.5,
                EnemyDamageScaling = 0.5,
                MobDetectionRange = 3,
                MaxMobs = 15
            };

            _generation = 0;
            _totalTests = 0;
            _history.Clear();

            Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
            Console.WriteLine("║         🧬 EVOLUTIONARY PROGRESSION TUNER 🧬                   ║");
            Console.WriteLine("║              Initializing baseline config...                   ║");
            Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");
            Console.WriteLine("Testing baseline fitness... (this may take 10-20 seconds)\n");

            _bestFitness = EvaluateFitness(_bestConfig);
            _totalTests = 1;

            Console.WriteLine($"✅ Baseline fitness: {_bestFitness:F2}");
            Console.WriteLine("\nStarting evolution... Press ESC to stop\n");
            Thread.Sleep(2000);

        while (true)
        {
            // Check for ESC
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                break;
            }

            _generation++;

            // Generate 5 mutations of best config
            var candidates = new List<(SimulationConfig config, double fitness)>();

            for (int i = 0; i < 5; i++)
            {
                var mutated = MutateConfig(_bestConfig, _generation);
                double fitness = EvaluateFitness(mutated);
                candidates.Add((mutated, fitness));
                _totalTests++;

                // Update if better
                if (fitness > _bestFitness)
                {
                    _bestFitness = fitness;
                    _bestConfig = CloneConfig(mutated);
                    _history.Add((_bestConfig, _bestFitness));
                }
            }

            // Render dashboard
            RenderDashboard(candidates);

            Thread.Sleep(100); // Brief pause between generations
        }

        }
        catch (Exception ex)
        {
            Console.CursorVisible = true;
            Console.Clear();
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"\n❌ ERROR: {ex.Message}");
            Console.WriteLine($"\nStack trace:\n{ex.StackTrace}");
            Console.ResetColor();
            Console.WriteLine("\nPress any key to return to menu...");
            Console.ReadKey();
            return;
        }

        Console.CursorVisible = true;
        Console.Clear();

        // Show final results
        Console.WriteLine("\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              EVOLUTIONARY TUNING COMPLETE!                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.WriteLine($"🏆 BEST CONFIGURATION FOUND (Fitness: {_bestFitness:F2}):\n");
        PrintConfig(_bestConfig);

        Console.WriteLine("\n💾 Save this configuration? [Y/N]");
        if (Console.ReadKey().Key == ConsoleKey.Y)
        {
            var stats = new SimulationStats
            {
                TotalRuns = _totalTests
            };
            ConfigPersistence.SaveOptimalConfig(_bestConfig, stats, _bestFitness, _generation);
            Console.WriteLine("\n✅ Saved to optimal_config.json!");
        }

        Console.WriteLine("\n\nPress any key to return to menu...");
        Console.ReadKey();
    }

    private static SimulationConfig MutateConfig(SimulationConfig parent, int generation)
    {
        var random = new Random();
        var mutated = CloneConfig(parent);

        // Mutation rate decreases with generations (fine-tuning over time)
        double mutationStrength = Math.Max(0.05, 1.0 / (1.0 + generation / 10.0));

        // Randomly select which parameters to mutate (1-3 params)
        int paramsToMutate = random.Next(1, 4);

        for (int i = 0; i < paramsToMutate; i++)
        {
            int param = random.Next(10); // 10 different parameters

            switch (param)
            {
                case 0: // PlayerStartHP
                    mutated.PlayerStartHP = Math.Clamp(
                        mutated.PlayerStartHP + random.Next(-5, 6),
                        10, 50);
                    break;

                case 1: // PlayerStrength
                    mutated.PlayerStrength = Math.Clamp(
                        mutated.PlayerStrength + random.Next(-2, 3),
                        1, 10);
                    break;

                case 2: // PlayerDefense
                    mutated.PlayerDefense = Math.Clamp(
                        mutated.PlayerDefense + random.Next(-2, 3),
                        1, 8);
                    break;

                case 3: // BaseEnemyHP
                    mutated.BaseEnemyHP = Math.Clamp(
                        mutated.BaseEnemyHP + random.Next(-2, 3),
                        3, 15);
                    break;

                case 4: // BaseEnemyDamage
                    mutated.BaseEnemyDamage = Math.Clamp(
                        mutated.BaseEnemyDamage + random.Next(-1, 2),
                        1, 5);
                    break;

                case 5: // EnemyHPScaling
                    mutated.EnemyHPScaling = Math.Clamp(
                        mutated.EnemyHPScaling + (random.NextDouble() - 0.5) * mutationStrength * 2,
                        0.5, 3.0);
                    break;

                case 6: // EnemyDamageScaling
                    mutated.EnemyDamageScaling = Math.Clamp(
                        mutated.EnemyDamageScaling + (random.NextDouble() - 0.5) * mutationStrength,
                        0.1, 1.5);
                    break;

                case 7: // MobDetectionRange
                    mutated.MobDetectionRange = Math.Clamp(
                        mutated.MobDetectionRange + random.Next(-1, 2),
                        2, 5);
                    break;

                case 8: // MaxMobs
                    mutated.MaxMobs = Math.Clamp(
                        mutated.MaxMobs + random.Next(-5, 6),
                        5, 35);
                    break;

                case 9: // HPPerLevel
                    mutated.HPPerLevel = Math.Clamp(
                        mutated.HPPerLevel + random.Next(-1, 2),
                        1, 5);
                    break;
            }
        }

        return mutated;
    }

    private static double EvaluateFitness(SimulationConfig config)
    {
        // SIMPLIFIED: Just test Level 1 for speed
        double totalScore = 0;
        int scenarios = 0;

        try
        {
            // Only test Level 1, Balanced build for speed
            var game = new RPGGame();
            game.SetPlayerStats(config.PlayerStrength, config.PlayerDefense);

            // Set HP from config
            var hpField = typeof(RPGGame).GetProperty("MaxPlayerHP");
            if (hpField != null && hpField.CanWrite)
            {
                hpField.SetValue(game, config.PlayerStartHP);
            }

            game.StartWorldExploration();

            // Quick combat test (2 runs only)
            int totalDeaths = 0;
            int totalCombats = 0;
            int totalTurns = 0;

            for (int run = 0; run < 2; run++)
            {
                // Reset HP
                game.SetHPForTesting(config.PlayerStartHP);

                // Spawn 2 enemies
                game.SpawnMobForTesting();
                game.SpawnMobForTesting();

                // Fight for 30 turns max
                int turns = 0;
                while (game.PlayerHP > 0 && turns < 30)
                {
                    turns++;

                    var mob = game.GetNearestMob();
                    if (mob != null)
                    {
                        // Move toward mob
                        int dx = Math.Sign(mob.X - game.PlayerX);
                        int dy = Math.Sign(mob.Y - game.PlayerY);

                        if (Math.Abs(dx) > Math.Abs(dy) && dx != 0)
                        {
                            if (dx > 0) game.MoveEast(); else game.MoveWest();
                        }
                        else if (dy != 0)
                        {
                            if (dy > 0) game.MoveSouth(); else game.MoveNorth();
                        }

                        // Check if adjacent
                        if (Math.Abs(mob.X - game.PlayerX) + Math.Abs(mob.Y - game.PlayerY) == 0)
                        {
                            game.TriggerMobEncounter(mob);

                            int combatRounds = 0;
                            while (!game.CombatEnded && combatRounds < 15)
                            {
                                game.ExecuteGameLoopRoundWithRandomHits(CombatAction.Attack, CombatAction.Attack);
                                combatRounds++;
                            }

                            if (game.IsWon)
                            {
                                game.ProcessGameLoopVictory();
                                game.RemoveMob(mob);
                                totalCombats++;
                            }

                            if (game.PlayerHP <= 0)
                            {
                                totalDeaths++;
                                break;
                            }
                        }
                    }
                }

                totalTurns += turns;
            }

            // Simple fitness: balance between survival and combat
            double survivalRate = 1.0 - (totalDeaths / 2.0);
            double combatRate = Math.Min(totalCombats / 2.0, 1.0);
            double turnEfficiency = Math.Min(totalTurns / 40.0, 1.0);

            totalScore = (survivalRate * 40) + (combatRate * 40) + (turnEfficiency * 20);
            scenarios = 1;
        }
        catch (Exception)
        {
            return 0; // Failed fitness = 0
        }

        return scenarios > 0 ? totalScore / scenarios : 0;
    }

    private static void SimulateProgressionToLevel(RPGGame game, int targetLevel, BuildArchetype build)
    {
        for (int level = 1; level < targetLevel; level++)
        {
            while (game.PlayerLevel < level + 1)
            {
                game.ProcessGameLoopVictory();
            }

            while (game.AvailableStatPoints > 0)
            {
                switch (build)
                {
                    case BuildArchetype.Balanced:
                        if (game.AvailableStatPoints >= 2)
                        {
                            game.SpendStatPoint(StatType.Strength);
                            game.SpendStatPoint(StatType.Defense);
                        }
                        else
                        {
                            game.SpendStatPoint(StatType.Strength);
                        }
                        break;

                    case BuildArchetype.StrengthFocus:
                        if (game.AvailableStatPoints % 5 == 0)
                            game.SpendStatPoint(StatType.Defense);
                        else
                            game.SpendStatPoint(StatType.Strength);
                        break;

                    case BuildArchetype.DefenseFocus:
                        if (game.AvailableStatPoints % 5 == 0)
                            game.SpendStatPoint(StatType.Strength);
                        else
                            game.SpendStatPoint(StatType.Defense);
                        break;
                }
            }
        }
    }

    private static void RenderDashboard(List<(SimulationConfig config, double fitness)> candidates)
    {
        Console.SetCursorPosition(0, 0);

        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║         🧬 EVOLUTIONARY PROGRESSION TUNER 🧬                   ║");
        Console.WriteLine("║              Press ESC to stop evolution                       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        Console.WriteLine($"Generation: {_generation,6}    Tests: {_totalTests,8}    Best Fitness: {_bestFitness,6:F2}");
        Console.WriteLine($"{"─",64}");
        Console.WriteLine();

        Console.WriteLine("🏆 CURRENT BEST CONFIGURATION:");
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Player: HP={_bestConfig.PlayerStartHP,2}  STR={_bestConfig.PlayerStrength,2}  DEF={_bestConfig.PlayerDefense,2}  (+{_bestConfig.HPPerLevel} HP/lvl)");
        Console.WriteLine($"  Enemy:  HP={_bestConfig.BaseEnemyHP,2} (+{_bestConfig.EnemyHPScaling:F1}/lvl)  DMG={_bestConfig.BaseEnemyDamage,2} (+{_bestConfig.EnemyDamageScaling:F1}/lvl)");
        Console.WriteLine($"  World:  Detection={_bestConfig.MobDetectionRange}  MaxMobs={_bestConfig.MaxMobs,2}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("📊 GENERATION CANDIDATES:");
        Console.WriteLine($"{"─",64}");

        for (int i = 0; i < candidates.Count; i++)
        {
            var (config, fitness) = candidates[i];
            Console.ForegroundColor = fitness > _bestFitness ? ConsoleColor.Yellow : ConsoleColor.Gray;
            Console.Write($"  #{i+1} ");
            Console.Write($"HP:{config.PlayerStartHP,2} STR:{config.PlayerStrength} DEF:{config.PlayerDefense} ");
            Console.Write($"EHP:{config.BaseEnemyHP,2} EDMG:{config.BaseEnemyDamage} ");
            Console.Write($"Fitness: {fitness,5:F1}");
            if (fitness > _bestFitness)
            {
                Console.Write(" 🌟 NEW BEST!");
            }
            Console.WriteLine();
        }
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("📈 EVOLUTION HISTORY (Last 5):");
        Console.WriteLine($"{"─",64}");

        var recentHistory = _history.TakeLast(5).ToList();
        for (int i = 0; i < recentHistory.Count; i++)
        {
            var (config, fitness) = recentHistory[i];
            Console.WriteLine($"  Gen {_generation - (recentHistory.Count - i - 1),6}: Fitness {fitness,6:F2}");
        }

        // Pad remaining lines to prevent scrolling
        for (int i = 0; i < 10; i++)
        {
            Console.WriteLine(new string(' ', 64));
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
            EnemyHPScaling = config.EnemyHPScaling,
            EnemyDamageScaling = config.EnemyDamageScaling,
            BaseEnemyHP = config.BaseEnemyHP,
            BaseEnemyDamage = config.BaseEnemyDamage,
            EquipmentDropRate = config.EquipmentDropRate,
            EquipmentBonusScaling = config.EquipmentBonusScaling,
            HPPerLevel = config.HPPerLevel,
            XPPerLevel = config.XPPerLevel,
            ShowVisuals = false
        };
    }

    private static void PrintConfig(SimulationConfig config)
    {
        Console.WriteLine($"  PlayerStartHP: {config.PlayerStartHP}");
        Console.WriteLine($"  PlayerStrength: {config.PlayerStrength}");
        Console.WriteLine($"  PlayerDefense: {config.PlayerDefense}");
        Console.WriteLine($"  HPPerLevel: {config.HPPerLevel}");
        Console.WriteLine();
        Console.WriteLine($"  BaseEnemyHP: {config.BaseEnemyHP}");
        Console.WriteLine($"  BaseEnemyDamage: {config.BaseEnemyDamage}");
        Console.WriteLine($"  EnemyHPScaling: {config.EnemyHPScaling:F2}");
        Console.WriteLine($"  EnemyDamageScaling: {config.EnemyDamageScaling:F2}");
        Console.WriteLine();
        Console.WriteLine($"  MobDetectionRange: {config.MobDetectionRange}");
        Console.WriteLine($"  MaxMobs: {config.MaxMobs}");
    }
}
