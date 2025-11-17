namespace ProjectEvolution.Game;

public enum BuildArchetype
{
    Balanced,      // 50/50 STR/DEF allocation
    StrengthFocus, // 80% STR, 20% DEF
    DefenseFocus   // 20% STR, 80% DEF
}

public class ProgressionMilestone
{
    public int TargetLevel { get; set; }
    public BuildArchetype Build { get; set; }
    public int CombatsWon { get; set; }
    public int Deaths { get; set; }
    public double AverageTurns { get; set; }
    public string Assessment { get; set; } = "";
}

public class ProgressionTuner
{
    public static void RunProgressionTuning()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           PROGRESSION-BASED TUNING SYSTEM                      ║");
        Console.WriteLine("║    Testing balance across levels & build archetypes            ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        var config = new SimulationConfig
        {
            ShowVisuals = false,
            PlayerStartHP = 20,      // Reasonable starting value
            PlayerStrength = 3,      // Base strength
            PlayerDefense = 1,       // Base defense
            BaseEnemyHP = 5,
            BaseEnemyDamage = 2,
            EnemyHPScaling = 1.5,    // Enemy HP grows with player level
            EnemyDamageScaling = 0.5  // Enemy damage grows with player level
        };

        var milestones = new List<ProgressionMilestone>();

        // Test at different progression points with different builds
        int[] testLevels = { 1, 3, 5, 7 };
        BuildArchetype[] builds = { BuildArchetype.Balanced, BuildArchetype.StrengthFocus, BuildArchetype.DefenseFocus };

        Console.WriteLine("🎯 Testing " + (testLevels.Length * builds.Length) + " different scenarios...\n");

        foreach (var level in testLevels)
        {
            foreach (var build in builds)
            {
                Console.WriteLine($"\n{'═',60}");
                Console.WriteLine($"Testing: Level {level} - {build} Build");
                Console.WriteLine($"{'═',60}");

                var milestone = TestMilestone(config, level, build);
                milestones.Add(milestone);

                Console.WriteLine($"Results: {milestone.CombatsWon} wins, {milestone.Deaths} deaths, {milestone.AverageTurns:F1} avg turns");
                Console.WriteLine($"Assessment: {milestone.Assessment}");

                Thread.Sleep(500);
            }
        }

        // Analyze overall progression balance
        Console.WriteLine("\n\n╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    PROGRESSION ANALYSIS                        ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        PrintProgressionReport(milestones, config);
        PrintRecommendations(milestones, config);
    }

    private static ProgressionMilestone TestMilestone(SimulationConfig config, int targetLevel, BuildArchetype build)
    {
        int totalCombats = 0;
        int totalDeaths = 0;
        int totalTurns = 0;
        int runsCompleted = 0;
        int testRuns = 10; // Runs per milestone

        for (int run = 0; run < testRuns; run++)
        {
            var game = new RPGGame();

            // Set base stats
            game.SetPlayerStats(config.PlayerStrength, config.PlayerDefense);
            game.StartWorldExploration();

            // Simulate progression to target level
            SimulateProgressionToLevel(game, targetLevel, build);

            // Now test combat at this level using GameSimulator approach
            int turns = 0;
            int maxTurns = 100;
            int combatRounds = 0;

            // Spawn some enemies to fight
            for (int i = 0; i < 5; i++)
            {
                game.SpawnMobForTesting();
            }

            while (game.PlayerHP > 0 && turns < maxTurns)
            {
                turns++;

                // Move toward nearest mob or explore
                var nearestMob = game.GetNearestMob();
                if (nearestMob != null)
                {
                    // Move toward mob
                    int dx = Math.Sign(nearestMob.X - game.PlayerX);
                    int dy = Math.Sign(nearestMob.Y - game.PlayerY);

                    if (dx != 0)
                    {
                        if (dx > 0) game.MoveEast(); else game.MoveWest();
                    }
                    else if (dy != 0)
                    {
                        if (dy > 0) game.MoveSouth(); else game.MoveNorth();
                    }

                    // Check if we're on a mob
                    var mob = game.GetMobAt(game.PlayerX, game.PlayerY);
                    if (mob != null)
                    {
                        // Trigger combat!
                        game.TriggerMobEncounter(mob);

                        // Auto-fight
                        while (!game.CombatEnded && combatRounds < 50)
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
                else
                {
                    // No mobs, spawn more
                    game.SpawnMobForTesting();
                }
            }

            totalTurns += turns;
            runsCompleted++;
        }

        double avgTurns = runsCompleted > 0 ? (double)totalTurns / runsCompleted : 0;
        double deathRate = runsCompleted > 0 ? (double)totalDeaths / runsCompleted * 100 : 0;

        string assessment = "";
        if (deathRate > 80)
            assessment = $"❌ TOO BRUTAL ({deathRate:F0}% death rate)";
        else if (deathRate > 50)
            assessment = $"⚠️  Very Hard ({deathRate:F0}% death rate)";
        else if (deathRate > 20)
            assessment = $"✅ Challenging ({deathRate:F0}% death rate)";
        else
            assessment = $"⚠️  Too Easy ({deathRate:F0}% death rate)";

        return new ProgressionMilestone
        {
            TargetLevel = targetLevel,
            Build = build,
            CombatsWon = totalCombats / runsCompleted,
            Deaths = totalDeaths,
            AverageTurns = avgTurns,
            Assessment = assessment
        };
    }

    private static void SimulateProgressionToLevel(RPGGame game, int targetLevel, BuildArchetype build)
    {
        // Grant levels and allocate stats according to build archetype
        for (int level = 1; level < targetLevel; level++)
        {
            // Grant level (give XP)
            while (game.PlayerLevel < level + 1)
            {
                int xpNeeded = game.XPForNextLevel - game.PlayerXP;
                // Add XP directly (would need to expose this method)
                for (int i = 0; i < xpNeeded / 10; i++)
                {
                    game.ProcessGameLoopVictory(); // Simulates winning combats
                }
            }

            // Allocate stat points based on build
            while (game.AvailableStatPoints > 0)
            {
                switch (build)
                {
                    case BuildArchetype.Balanced:
                        // 50/50 split
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
                        // 80% STR, 20% DEF
                        if (game.AvailableStatPoints % 5 == 0)
                            game.SpendStatPoint(StatType.Defense);
                        else
                            game.SpendStatPoint(StatType.Strength);
                        break;

                    case BuildArchetype.DefenseFocus:
                        // 20% STR, 80% DEF
                        if (game.AvailableStatPoints % 5 == 0)
                            game.SpendStatPoint(StatType.Strength);
                        else
                            game.SpendStatPoint(StatType.Defense);
                        break;
                }
            }
        }
    }

    private static void PrintProgressionReport(List<ProgressionMilestone> milestones, SimulationConfig config)
    {
        Console.WriteLine("📊 CURRENT BALANCE SETTINGS:");
        Console.WriteLine($"   Starting HP: {config.PlayerStartHP}");
        Console.WriteLine($"   Starting STR: {config.PlayerStrength}, DEF: {config.PlayerDefense}");
        Console.WriteLine($"   Enemy Base HP: {config.BaseEnemyHP}, Damage: {config.BaseEnemyDamage}");
        Console.WriteLine($"   Enemy Scaling: +{config.EnemyHPScaling} HP, +{config.EnemyDamageScaling} DMG per level\n");

        Console.WriteLine("📈 PROGRESSION CURVE:\n");

        foreach (var level in milestones.Select(m => m.TargetLevel).Distinct().OrderBy(l => l))
        {
            Console.WriteLine($"  LEVEL {level}:");
            var levelMilestones = milestones.Where(m => m.TargetLevel == level).ToList();

            foreach (var milestone in levelMilestones)
            {
                Console.WriteLine($"    {milestone.Build,-15} - {milestone.Assessment}");
            }
            Console.WriteLine();
        }
    }

    private static void PrintRecommendations(List<ProgressionMilestone> milestones, SimulationConfig config)
    {
        Console.WriteLine("\n💡 RECOMMENDATIONS:\n");

        // Check early game (Level 1)
        var earlyGame = milestones.Where(m => m.TargetLevel == 1).ToList();
        int earlyDeaths = earlyGame.Sum(m => m.Deaths);
        if (earlyDeaths > earlyGame.Count * 5) // >50% death rate
        {
            Console.WriteLine("⚠️  EARLY GAME TOO HARD:");
            Console.WriteLine($"   → Increase PlayerStartHP from {config.PlayerStartHP} to {config.PlayerStartHP + 5}");
            Console.WriteLine($"   → Reduce BaseEnemyDamage from {config.BaseEnemyDamage} to {config.BaseEnemyDamage - 1}");
        }

        // Check mid game (Level 3-5)
        var midGame = milestones.Where(m => m.TargetLevel >= 3 && m.TargetLevel <= 5).ToList();
        int midDeaths = midGame.Sum(m => m.Deaths);
        if (midDeaths < midGame.Count * 2) // <20% death rate
        {
            Console.WriteLine("⚠️  MID GAME TOO EASY:");
            Console.WriteLine($"   → Increase EnemyHPScaling from {config.EnemyHPScaling} to {config.EnemyHPScaling + 0.5}");
            Console.WriteLine($"   → Increase EnemyDamageScaling from {config.EnemyDamageScaling} to {config.EnemyDamageScaling + 0.3}");
        }

        // Check build diversity
        var balancedBuilds = milestones.Where(m => m.Build == BuildArchetype.Balanced);
        var strBuilds = milestones.Where(m => m.Build == BuildArchetype.StrengthFocus);
        var defBuilds = milestones.Where(m => m.Build == BuildArchetype.DefenseFocus);

        double balancedDeathRate = balancedBuilds.Any() ? (double)balancedBuilds.Sum(m => m.Deaths) / balancedBuilds.Count() : 0;
        double strDeathRate = strBuilds.Any() ? (double)strBuilds.Sum(m => m.Deaths) / strBuilds.Count() : 0;
        double defDeathRate = defBuilds.Any() ? (double)defBuilds.Sum(m => m.Deaths) / defBuilds.Count() : 0;

        Console.WriteLine($"\n📊 BUILD VIABILITY:");
        Console.WriteLine($"   Balanced Build:  {balancedDeathRate:F1} avg deaths - {(balancedDeathRate < 5 ? "✅ Viable" : "❌ Too Hard")}");
        Console.WriteLine($"   Strength Build:  {strDeathRate:F1} avg deaths - {(strDeathRate < 5 ? "✅ Viable" : "❌ Glass Cannon")}");
        Console.WriteLine($"   Defense Build:   {defDeathRate:F1} avg deaths - {(defDeathRate < 5 ? "✅ Viable" : "❌ Lacks Damage")}");

        if (Math.Abs(balancedDeathRate - strDeathRate) > 3 || Math.Abs(balancedDeathRate - defDeathRate) > 3)
        {
            Console.WriteLine("\n⚠️  BUILD IMBALANCE DETECTED:");
            Console.WriteLine("   → Consider adjusting enemy stats to make all builds viable");
        }

        Console.WriteLine("\n✅ Progression tuning complete! Use these values in your config.");
    }
}
