using System.Text.Json;
using System.Text.Json.Serialization;

namespace ProjectEvolution.Game;

// Machine-readable progression framework
public class ProgressionFrameworkData
{
    public PlayerProgressionFormulas PlayerProgression { get; set; } = new();
    public EnemyProgressionFormulas EnemyProgression { get; set; } = new();
    public EquipmentProgression Equipment { get; set; } = new();
    public EconomicProgression Economy { get; set; } = new();
    public LootProgression Loot { get; set; } = new();
    public BuildViability Builds { get; set; } = new();
    public ResearchMetadata Metadata { get; set; } = new();
}

public class PlayerProgressionFormulas
{
    public int BaseHP { get; set; }
    public double HPPerLevel { get; set; }
    public int BaseSTR { get; set; }
    public int BaseDEF { get; set; }
    public int StatPointsPerLevel { get; set; }

    // Derived formulas as code strings
    public string MaxHPFormula => $"BaseHP + (Level * {HPPerLevel})";
    public string EffectiveSTRFormula => $"BaseSTR + AllocatedSTR + EquipmentBonus";
}

public class EnemyProgressionFormulas
{
    public int BaseHP { get; set; }
    public double HPScalingCoefficient { get; set; }
    public int BaseDamage { get; set; }
    public double DamageScalingCoefficient { get; set; }

    public string HPFormula => $"BaseHP + (PlayerLevel * {HPScalingCoefficient:F2})";
    public string DamageFormula => $"BaseDamage + (PlayerLevel * {DamageScalingCoefficient:F2})";
}

public class EquipmentProgression
{
    public List<EquipmentTier> WeaponTiers { get; set; } = new();
    public List<EquipmentTier> ArmorTiers { get; set; } = new();
    public double PowerGrowthModel { get; set; } // 0=flat, 1=linear, 2=exponential
    public string RecommendedFormula { get; set; } = "";
}

public class EquipmentTier
{
    public int Tier { get; set; }
    public int BonusValue { get; set; }
    public int RecommendedCost { get; set; }
    public int RecommendedLevel { get; set; }
    public double PowerIncrease { get; set; } // % over previous tier
}

public class EconomicProgression
{
    public int BaseGoldPerCombat { get; set; }
    public double GoldScalingCoefficient { get; set; }
    public List<LevelEconomicSnapshot> LevelSnapshots { get; set; } = new();
    public string GoldFormula => $"BaseGold + (EnemyLevel * {GoldScalingCoefficient:F2})";
    public bool CanAffordProgression { get; set; }
    public int FirstUnaffordableTier { get; set; } = -1;
}

public class LevelEconomicSnapshot
{
    public int Level { get; set; }
    public int ExpectedGoldEarned { get; set; }
    public int CumulativeGold { get; set; }
    public int AffordableEquipmentTier { get; set; }
    public int RecommendedEquipmentTier { get; set; }
    public bool EconomyHealthy { get; set; }
    public string Notes { get; set; } = "";
}

public class LootProgression
{
    public int BaseTreasureGold { get; set; }
    public int TreasurePerDungeonDepth { get; set; }
    public double EquipmentDropRate { get; set; }
    public List<LootTable> DungeonLoot { get; set; } = new();
    public string TreasureFormula => $"BaseTreasure + (Depth * {TreasurePerDungeonDepth})";
}

public class LootTable
{
    public int DungeonDepth { get; set; }
    public int MinGold { get; set; }
    public int MaxGold { get; set; }
    public double EquipmentDropChance { get; set; }
    public int RecommendedEquipmentTier { get; set; }
}

public class BuildViability
{
    public Dictionary<string, BuildRequirements> ViableBuilds { get; set; } = new();
    public string MostBalanced { get; set; } = "";
    public List<string> Warnings { get; set; } = new();
}

public class BuildRequirements
{
    public int MinBaseHP { get; set; }
    public int MaxBaseHP { get; set; }
    public (int str, int def) RecommendedRatio { get; set; }
    public double ViabilityScore { get; set; }
}

public class ResearchMetadata
{
    public DateTime Timestamp { get; set; }
    public int SimulationsRun { get; set; }
    public int Generation { get; set; }
    public double OverallFitness { get; set; }
    public string Version { get; set; } = "Generation 35";
}

public class ProgressionFrameworkResearcher
{
    private static int _generation = 0;
    private static ProgressionFrameworkData? _bestFramework = null;
    private static double _bestFitness = 0;
    private static DateTime _startTime = DateTime.Now;
    private static string _currentPhase = "Initializing...";
    private static int _totalSimulations = 0;
    private static int _generationsSinceImprovement = 0;
    private static DateTime _lastSaveTime = DateTime.Now;
    private const int AUTO_SAVE_INTERVAL_GENERATIONS = 5;

    public static void RunContinuousResearch()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      🧬 CONTINUOUS PROGRESSION FRAMEWORK RESEARCH 🧬           ║");
        Console.WriteLine("║    Discovers formulas → Generates code → Tests → Refines       ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        // Configure output path
        Console.WriteLine("📁 Output Configuration:");
        Console.WriteLine("   [1] Local directory (current folder)");
        Console.WriteLine("   [2] Unraid share (network mount)");
        Console.WriteLine("   [3] Custom path\n");
        Console.Write("Select output location [1-3]: ");

        var pathChoice = Console.ReadKey(intercept: true);
        Console.WriteLine();

        if (pathChoice.Key == ConsoleKey.D2 || pathChoice.KeyChar == '2')
        {
            Console.Write("\nEnter Unraid share path (e.g., /mnt/user/GameResearch): ");
            string? unraidPath = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(unraidPath))
            {
                SafeFileWriter.SetOutputPath(unraidPath);
                Console.WriteLine($"✅ Output path set to: {unraidPath}");
            }
        }
        else if (pathChoice.Key == ConsoleKey.D3 || pathChoice.KeyChar == '3')
        {
            Console.Write("\nEnter custom path: ");
            string? customPath = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(customPath))
            {
                SafeFileWriter.SetOutputPath(customPath);
                Console.WriteLine($"✅ Output path set to: {customPath}");
            }
        }
        else
        {
            SafeFileWriter.SetOutputPath(".");
            Console.WriteLine("✅ Using local directory");
        }

        Console.WriteLine("\nTesting write access...");
        if (!SafeFileWriter.TestOutputPath())
        {
            Console.WriteLine("❌ Cannot write to output path! Press any key to return...");
            Console.ReadKey();
            return;
        }
        Console.WriteLine("✅ Write access confirmed!\n");

        Console.WriteLine("This system will:");
        Console.WriteLine("  1️⃣  Discover optimal progression formulas");
        Console.WriteLine("  2️⃣  Simulate economy over 10 levels");
        Console.WriteLine("  3️⃣  Balance gold income vs equipment costs");
        Console.WriteLine("  4️⃣  Tune loot drop rates");
        Console.WriteLine("  5️⃣  Output machine-readable JSON");
        Console.WriteLine("  6️⃣  Auto-generate balanced code");
        Console.WriteLine("  7️⃣  Test & refine continuously!");
        Console.WriteLine("  8️⃣  Safe file writes with lock protection!\n");

        // Load previous research if exists
        string frameworkPath = SafeFileWriter.GetFullPath("progression_framework.json");
        if (File.Exists(frameworkPath))
        {
            try
            {
                string json = File.ReadAllText(frameworkPath);
                _bestFramework = JsonSerializer.Deserialize<ProgressionFrameworkData>(json);
                if (_bestFramework != null)
                {
                    _generation = _bestFramework.Metadata.Generation;
                    _bestFitness = _bestFramework.Metadata.OverallFitness;
                    Console.ForegroundColor = ConsoleColor.Green;
                    Console.WriteLine($"✅ Loaded previous research: Generation {_generation}, Fitness {_bestFitness:F2}");
                    Console.ResetColor();
                    Console.WriteLine("   Continuing from where you left off...\n");
                }
            }
            catch
            {
                Console.WriteLine("⚠️  Could not load previous research, starting fresh...\n");
            }
        }

        Console.WriteLine("Press ESC to stop, any other key to start...");
        if (Console.ReadKey().Key == ConsoleKey.Escape) return;

        _startTime = DateTime.Now;
        _lastSaveTime = DateTime.Now;
        _totalSimulations = 0;

        // Initialize log file with header
        if (_generation == 0)
        {
            var logHeader = new System.Text.StringBuilder();
            logHeader.AppendLine("═══════════════════════════════════════════════════════════════");
            logHeader.AppendLine("  PROGRESSION FRAMEWORK RESEARCH LOG");
            logHeader.AppendLine($"  Output Path: {SafeFileWriter.GetOutputPath()}");
            logHeader.AppendLine($"  Started: {DateTime.Now}");
            logHeader.AppendLine("═══════════════════════════════════════════════════════════════");
            SafeFileWriter.SafeWriteAllText("progression_research.log", logHeader.ToString());

            var summaryHeader = "═══ PROGRESSION RESEARCH SUMMARY ═══\n";
            SafeFileWriter.SafeWriteAllText("progression_summary.log", summaryHeader);
        }

        // Initial render once
        Console.Clear();
        if (_bestFramework != null)
        {
            RenderResearchDashboard();
        }

        while (true)
        {
            _generation++;

            // PHASE 1: Mutate current best framework
            // If stuck too long, do random restart
            ProgressionFrameworkData framework;
            if (_generationsSinceImprovement > 100)
            {
                _currentPhase = "🔄 RANDOM RESTART - Exploring new space...";
                UpdateStatusLine();
                framework = CreateBaselineFramework(); // Start fresh
                _generationsSinceImprovement = 0;
                Thread.Sleep(500); // Let user see restart
            }
            else
            {
                _currentPhase = "🔬 Mutating parameters...";
                UpdateStatusLine();
                framework = MutateFramework(_bestFramework ?? CreateBaselineFramework());
            }
            _totalSimulations += 50;

            // PHASE 2: Evaluate fitness
            _currentPhase = "📊 Evaluating...";
            UpdateStatusLine();

            double fitness = EvaluateFramework(framework);
            _totalSimulations += 15;

            // PHASE 3: Update if better
            bool improved = false;
            if (fitness > _bestFitness || _bestFramework == null)
            {
                _currentPhase = "🌟 NEW BEST! Saving & generating code...";
                UpdateStatusLine();

                _bestFitness = fitness;
                _bestFramework = framework;
                _generationsSinceImprovement = 0;
                improved = true;

                SaveFrameworkToJSON(framework);
                GenerateGameCode(framework);
                _lastSaveTime = DateTime.Now;

                // Full redraw when improved to show new formulas
                Console.SetCursorPosition(0, 0);
                RenderResearchDashboard();
                Thread.Sleep(500); // Brief pause to see new values
            }
            else
            {
                _generationsSinceImprovement++;
            }

            // PHASE 4: Auto-save every 5 generations
            if (_generation % AUTO_SAVE_INTERVAL_GENERATIONS == 0)
            {
                _currentPhase = "💾 Auto-save...";
                UpdateStatusLine();
                if (_bestFramework != null)
                {
                    SaveFrameworkToJSON(_bestFramework);
                }
                _lastSaveTime = DateTime.Now;
            }

            // PHASE 5: Update status
            _currentPhase = improved ? "✅ Improved!" : "🔄 Searching...";
            UpdateStatusLine();

            // Log progress to file
            LogProgress(framework, fitness, improved);

            // Check for ESC
            if (Console.KeyAvailable && Console.ReadKey(true).Key == ConsoleKey.Escape)
            {
                _currentPhase = "💾 Saving...";
                UpdateStatusLine();
                if (_bestFramework != null)
                {
                    SaveFrameworkToJSON(_bestFramework);
                }
                break;
            }

            // Fast cycle - no pause!
            Thread.Sleep(50);
        }

        Console.Clear();

        TimeSpan totalElapsed = DateTime.Now - _startTime;

        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║              🎉 RESEARCH SESSION COMPLETE! 🎉                  ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"⏱️  Total Runtime: {totalElapsed.Hours}h {totalElapsed.Minutes}m {totalElapsed.Seconds}s");
        Console.WriteLine($"🧬 Generations: {_generation}");
        Console.WriteLine($"🔬 Simulations: {_totalSimulations}");
        Console.WriteLine($"🏆 Best Fitness: {_bestFitness:F2}");
        Console.ResetColor();
        Console.WriteLine();

        Console.WriteLine("📁 GENERATED FILES:");
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"  📂 Output Location: {SafeFileWriter.GetOutputPath()}");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  ✅ {SafeFileWriter.GetFullPath("progression_framework.json")}");
        Console.WriteLine("     └─ Machine-readable progression data");
        Console.WriteLine();
        Console.WriteLine($"  ✅ {SafeFileWriter.GetFullPath("GeneratedCode/ProgressionFormulas_Generated.cs")}");
        Console.WriteLine("     └─ Ready-to-use scaling formulas");
        Console.WriteLine();
        Console.WriteLine($"  ✅ {SafeFileWriter.GetFullPath("GeneratedCode/Equipment_Generated.cs")}");
        Console.WriteLine("     └─ Balanced weapons & armor");
        Console.WriteLine();
        Console.WriteLine($"  ✅ {SafeFileWriter.GetFullPath("PROGRESSION_FRAMEWORK.md")}");
        Console.WriteLine("     └─ Human-readable design document");
        Console.WriteLine();
        Console.WriteLine($"  ✅ {SafeFileWriter.GetFullPath("progression_research.log")}");
        Console.WriteLine("     └─ Detailed generation log");
        Console.ResetColor();
        Console.WriteLine();

        if (_bestFramework != null)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("📊 DISCOVERED INSIGHTS:");
            Console.WriteLine($"{"─",64}");
            Console.ResetColor();
            Console.WriteLine($"  • HP scales at +{_bestFramework.PlayerProgression.HPPerLevel:F1} per level");
            Console.WriteLine($"  • Enemy HP grows {_bestFramework.EnemyProgression.HPScalingCoefficient:F2}× player level");
            Console.WriteLine($"  • Gold income: {_bestFramework.Economy.BaseGoldPerCombat}g + ({_bestFramework.Economy.GoldScalingCoefficient:F2}× enemy level)");
            Console.WriteLine($"  • Economy status: {(_bestFramework.Economy.CanAffordProgression ? "✅ HEALTHY - Players can afford progression" : "❌ BROKEN - Need more gold")}");
            Console.WriteLine($"  • Viable builds: {_bestFramework.Builds.ViableBuilds.Count(b => b.Value.ViabilityScore > 60)}/3");
            Console.WriteLine();
        }

        Console.WriteLine("\nPress any key to return to menu...");
        Console.ReadKey();
    }

    private static ProgressionFrameworkData CreateBaselineFramework()
    {
        var framework = new ProgressionFrameworkData();

        // Player progression baseline
        framework.PlayerProgression.BaseHP = 20;
        framework.PlayerProgression.HPPerLevel = 3;
        framework.PlayerProgression.BaseSTR = 3;
        framework.PlayerProgression.BaseDEF = 1;
        framework.PlayerProgression.StatPointsPerLevel = 2;

        // Enemy progression baseline
        framework.EnemyProgression.BaseHP = 5;
        framework.EnemyProgression.HPScalingCoefficient = 1.5;
        framework.EnemyProgression.BaseDamage = 2;
        framework.EnemyProgression.DamageScalingCoefficient = 0.4;

        // Economy baseline - ENSURE VALID VALUES!
        var economy = new EconomicProgression();
        economy.BaseGoldPerCombat = 10;
        economy.GoldScalingCoefficient = 3.5; // Start with healthy value
        economy.CanAffordProgression = true;
        framework.Economy = economy;

        // Loot baseline
        var loot = new LootProgression();
        loot.BaseTreasureGold = 20;
        loot.TreasurePerDungeonDepth = 30;
        loot.EquipmentDropRate = 20;
        framework.Loot = loot;

        // Generate equipment and test economy
        framework.Equipment = GenerateEquipmentTiers(framework);
        framework.Economy = SimulateEconomicProgression(framework);
        framework.Builds = TestBuildViabilityQuick(framework);

        // Metadata
        framework.Metadata.Timestamp = DateTime.Now;
        framework.Metadata.Generation = 0;
        framework.Metadata.Version = "Generation 35";

        return framework;
    }

    private static ProgressionFrameworkData MutateFramework(ProgressionFrameworkData parent)
    {
        var random = new Random(_generation * 7); // Different seed each generation
        var mutated = new ProgressionFrameworkData();

        // Mutate player progression (20% chance each)
        mutated.PlayerProgression.BaseHP = random.Next(100) < 20
            ? Math.Clamp(parent.PlayerProgression.BaseHP + random.Next(-3, 4), 15, 40)
            : parent.PlayerProgression.BaseHP;

        mutated.PlayerProgression.HPPerLevel = random.Next(100) < 20
            ? Math.Clamp(parent.PlayerProgression.HPPerLevel + random.Next(-1, 2), 1, 5)
            : parent.PlayerProgression.HPPerLevel;

        mutated.PlayerProgression.BaseSTR = parent.PlayerProgression.BaseSTR;
        mutated.PlayerProgression.BaseDEF = parent.PlayerProgression.BaseDEF;
        mutated.PlayerProgression.StatPointsPerLevel = parent.PlayerProgression.StatPointsPerLevel;

        // Mutate enemy progression (30% chance each)
        mutated.EnemyProgression.BaseHP = random.Next(100) < 30
            ? Math.Clamp(parent.EnemyProgression.BaseHP + random.Next(-2, 3), 3, 12)
            : parent.EnemyProgression.BaseHP;

        mutated.EnemyProgression.HPScalingCoefficient = random.Next(100) < 30
            ? Math.Clamp(parent.EnemyProgression.HPScalingCoefficient + (random.NextDouble() - 0.5) * 0.5, 0.5, 3.0)
            : parent.EnemyProgression.HPScalingCoefficient;

        mutated.EnemyProgression.BaseDamage = random.Next(100) < 30
            ? Math.Clamp(parent.EnemyProgression.BaseDamage + random.Next(-1, 2), 1, 5)
            : parent.EnemyProgression.BaseDamage;

        mutated.EnemyProgression.DamageScalingCoefficient = random.Next(100) < 30
            ? Math.Clamp(parent.EnemyProgression.DamageScalingCoefficient + (random.NextDouble() - 0.5) * 0.3, 0.1, 1.0)
            : parent.EnemyProgression.DamageScalingCoefficient;

        // CRITICAL: Mutate ECONOMY parameters (60% chance each - VERY high priority!)
        var economy = new EconomicProgression();
        economy.BaseGoldPerCombat = random.Next(100) < 60
            ? Math.Clamp(parent.Economy.BaseGoldPerCombat + random.Next(-3, 4), 8, 20) // Never below 8!
            : parent.Economy.BaseGoldPerCombat;

        economy.GoldScalingCoefficient = random.Next(100) < 60
            ? Math.Clamp(parent.Economy.GoldScalingCoefficient + (random.NextDouble() - 0.5) * 2.0, 2.0, 6.0) // Never below 2.0!
            : parent.Economy.GoldScalingCoefficient;

        mutated.Economy = economy;

        // Mutate LOOT parameters (40% chance each)
        var loot = new LootProgression();
        loot.BaseTreasureGold = random.Next(100) < 40
            ? Math.Clamp(parent.Loot.BaseTreasureGold + random.Next(-10, 11), 10, 50)
            : parent.Loot.BaseTreasureGold;

        loot.TreasurePerDungeonDepth = random.Next(100) < 40
            ? Math.Clamp(parent.Loot.TreasurePerDungeonDepth + random.Next(-10, 11), 15, 60)
            : parent.Loot.TreasurePerDungeonDepth;

        loot.EquipmentDropRate = random.Next(100) < 40
            ? Math.Clamp(parent.Loot.EquipmentDropRate + random.Next(-5, 6), 10, 40)
            : parent.Loot.EquipmentDropRate;

        mutated.Loot = loot;

        // Re-generate equipment tiers and simulate economy with mutated values
        mutated.Equipment = GenerateEquipmentTiers(mutated);
        mutated.Economy = SimulateEconomicProgression(mutated);
        mutated.Builds = TestBuildViabilityQuick(mutated);

        // Metadata
        mutated.Metadata.Timestamp = DateTime.Now;
        mutated.Metadata.Generation = _generation;
        mutated.Metadata.Version = "Generation 35";
        mutated.Metadata.SimulationsRun = _totalSimulations;

        return mutated;
    }

    private static ProgressionFrameworkData DiscoverProgressionFormulas()
    {
        var framework = new ProgressionFrameworkData();

        // Test HP scaling (silent - all happens in one frame)
        var hpResults = TestHPScalingQuick();
        framework.PlayerProgression.BaseHP = 20;
        framework.PlayerProgression.HPPerLevel = hpResults.optimal;
        framework.PlayerProgression.BaseSTR = 3;
        framework.PlayerProgression.BaseDEF = 1;
        framework.PlayerProgression.StatPointsPerLevel = 2;

        // Test enemy scaling
        var enemyResults = TestEnemyScalingQuick();
        framework.EnemyProgression.BaseHP = 5;
        framework.EnemyProgression.HPScalingCoefficient = enemyResults.hpCoef;
        framework.EnemyProgression.BaseDamage = 2;
        framework.EnemyProgression.DamageScalingCoefficient = enemyResults.dmgCoef;

        // Simulate economy progression
        framework.Economy = SimulateEconomicProgression(framework);
        framework.Loot = GenerateLootTables(framework);

        // Generate equipment tiers with economically-balanced costs
        framework.Equipment = GenerateEquipmentTiers(framework);

        // Test build viability
        framework.Builds = TestBuildViabilityQuick(framework);

        // Metadata
        framework.Metadata.Timestamp = DateTime.Now;
        framework.Metadata.Generation = _generation;
        framework.Metadata.Version = "Generation 35";
        framework.Metadata.SimulationsRun = _totalSimulations;

        return framework;
    }

    private static EconomicProgression SimulateEconomicProgression(ProgressionFrameworkData framework)
    {
        var economy = new EconomicProgression();
        var random = new Random(_generation * 3);

        // Test different gold scaling values
        economy.BaseGoldPerCombat = 10;
        economy.GoldScalingCoefficient = 2.0 + random.NextDouble() * 3.0; // 2.0 to 5.0 per enemy level

        // Simulate progression from level 1 to 10
        int cumulativeGold = 50; // Starting gold
        bool economyFailed = false;

        for (int level = 1; level <= 10; level++)
        {
            // Estimate combats per level (about 5-10)
            int combatsThisLevel = 5 + random.Next(6);
            int avgEnemyLevel = level;

            // Gold earned this level
            int goldPerCombat = economy.BaseGoldPerCombat + (int)(avgEnemyLevel * economy.GoldScalingCoefficient);
            int goldEarnedThisLevel = combatsThisLevel * goldPerCombat;

            // Add dungeon treasure (if they do 1 dungeon per 3 levels)
            if (level % 3 == 0)
            {
                int dungeonDepth = level / 3;
                int treasureGold = 20 + (dungeonDepth * 30); // Base treasure scaling
                goldEarnedThisLevel += treasureGold;
            }

            cumulativeGold += goldEarnedThisLevel;

            // What tier equipment should they have?
            int recommendedTier = level / 2; // Tier every 2 levels
            int equipmentCostAtTier = recommendedTier * recommendedTier * 25 + 5;

            // Can they afford it?
            bool canAfford = cumulativeGold >= equipmentCostAtTier;
            int affordableTier = 0;
            for (int t = 0; t <= 5; t++)
            {
                int cost = t * t * 25 + 5;
                if (cumulativeGold >= cost) affordableTier = t;
            }

            // Account for spending (assume they buy equipment when they can)
            if (canAfford && recommendedTier > 0)
            {
                cumulativeGold -= equipmentCostAtTier;
            }

            bool healthy = affordableTier >= recommendedTier || recommendedTier == 0;

            var snapshot = new LevelEconomicSnapshot
            {
                Level = level,
                ExpectedGoldEarned = goldEarnedThisLevel,
                CumulativeGold = cumulativeGold,
                AffordableEquipmentTier = affordableTier,
                RecommendedEquipmentTier = recommendedTier,
                EconomyHealthy = healthy,
                Notes = healthy ? "✅ Can afford progression" : $"⚠️  Can't afford Tier {recommendedTier}"
            };

            economy.LevelSnapshots.Add(snapshot);

            if (!healthy && !economyFailed)
            {
                economyFailed = true;
                economy.FirstUnaffordableTier = recommendedTier;
            }
        }

        economy.CanAffordProgression = !economyFailed;

        // If economy failed, increase gold scaling
        if (economyFailed)
        {
            economy.GoldScalingCoefficient *= 1.2; // 20% increase
        }

        return economy;
    }

    private static LootProgression GenerateLootTables(ProgressionFrameworkData framework)
    {
        var loot = new LootProgression();
        var random = new Random(_generation * 4);

        loot.BaseTreasureGold = 15 + random.Next(10);
        loot.TreasurePerDungeonDepth = 25 + random.Next(15);
        loot.EquipmentDropRate = 15 + random.Next(20); // 15-35% chance

        // Generate loot tables for dungeon depths 1-5
        for (int depth = 1; depth <= 5; depth++)
        {
            int baseGold = loot.BaseTreasureGold;
            int bonus = depth * loot.TreasurePerDungeonDepth;

            loot.DungeonLoot.Add(new LootTable
            {
                DungeonDepth = depth,
                MinGold = baseGold + bonus - 10,
                MaxGold = baseGold + bonus + 10,
                EquipmentDropChance = loot.EquipmentDropRate + (depth * 5), // Higher drop rate in deeper dungeons
                RecommendedEquipmentTier = Math.Min(5, depth)
            });
        }

        return loot;
    }

    private static (int optimal, double fitness) TestHPScalingQuick()
    {
        var random = new Random(_generation); // Different seed each gen!
        int best = 2;
        double bestFitness = 0;

        foreach (int hpPerLevel in new[] { 1, 2, 3, 4, 5 })
        {
            int hp = 20 + (5 * hpPerLevel); // Test at level 5
            double viability = QuickCombatTest(hp, 8, 5, 5, 7, 2);

            if (viability > bestFitness)
            {
                bestFitness = viability;
                best = hpPerLevel;
            }
        }

        return (best, bestFitness);
    }

    private static (double hpCoef, double dmgCoef) TestEnemyScalingQuick()
    {
        var random = new Random(_generation * 2);

        // Randomize search each generation
        double hpCoef = 1.0 + (random.NextDouble() * 2.0); // 1.0 to 3.0
        double dmgCoef = 0.2 + (random.NextDouble() * 0.8); // 0.2 to 1.0

        return (hpCoef, dmgCoef);
    }

    private static EquipmentProgression GenerateEquipmentTiers(ProgressionFrameworkData framework)
    {
        var progression = new EquipmentProgression();
        progression.PowerGrowthModel = 1.0; // Linear

        // Generate 6 weapon tiers (0-5)
        for (int tier = 0; tier <= 5; tier++)
        {
            progression.WeaponTiers.Add(new EquipmentTier
            {
                Tier = tier,
                BonusValue = tier, // Flat progression: 0, 1, 2, 3, 4, 5
                RecommendedCost = tier * tier * 25 + 5, // Quadratic cost
                RecommendedLevel = tier * 2, // Available every 2 levels
                PowerIncrease = tier > 0 ? (tier / (double)(tier - 1 + 0.001) - 1) * 100 : 0
            });
        }

        // Generate 6 armor tiers
        for (int tier = 0; tier <= 5; tier++)
        {
            progression.ArmorTiers.Add(new EquipmentTier
            {
                Tier = tier,
                BonusValue = tier,
                RecommendedCost = tier * tier * 25 + 5,
                RecommendedLevel = tier * 2,
                PowerIncrease = tier > 0 ? (tier / (double)(tier - 1 + 0.001) - 1) * 100 : 0
            });
        }

        progression.RecommendedFormula = "Flat linear: Bonus = Tier";

        return progression;
    }

    private static BuildViability TestBuildViabilityQuick(ProgressionFrameworkData framework)
    {
        var viability = new BuildViability();

        // Test 3 archetypes quickly
        var builds = new[]
        {
            ("GlassCannon", 10, 2),
            ("Balanced", 6, 6),
            ("Tank", 3, 10)
        };

        foreach (var (name, str, def) in builds)
        {
            int hp = framework.PlayerProgression.BaseHP + (5 * (int)framework.PlayerProgression.HPPerLevel);
            double viabilityScore = QuickCombatTest(hp, str, def, 5, 7, 2);

            viability.ViableBuilds[name] = new BuildRequirements
            {
                MinBaseHP = framework.PlayerProgression.BaseHP,
                MaxBaseHP = framework.PlayerProgression.BaseHP + 10,
                RecommendedRatio = (str * 10, def * 10),
                ViabilityScore = viabilityScore
            };
        }

        viability.MostBalanced = "Balanced";

        return viability;
    }

    private static double QuickCombatTest(int playerHP, int playerSTR, int playerDEF, int level, int enemyHP, int enemyDMG)
    {
        int combatsWon = 0;

        for (int i = 0; i < 3; i++)
        {
            int hp = playerHP;
            int ehp = enemyHP;

            while (ehp > 0 && hp > 0)
            {
                ehp -= playerSTR;
                hp -= Math.Max(1, enemyDMG - playerDEF);
            }

            if (hp > 0) combatsWon++;
        }

        return (combatsWon / 3.0) * 100;
    }

    private static double EvaluateFramework(ProgressionFrameworkData framework)
    {
        // CRITICAL: Broken economy = INSTANT FAILURE
        if (framework.Economy.GoldScalingCoefficient <= 0.1 || framework.Economy.BaseGoldPerCombat <= 0)
        {
            return 0; // Invalid configuration
        }

        if (!framework.Economy.CanAffordProgression)
        {
            return 10; // Broken economy is terrible
        }

        // 1. Economic health - MOST IMPORTANT (50%)
        int healthyLevels = framework.Economy.LevelSnapshots.Count(s => s.EconomyHealthy);
        int totalLevels = framework.Economy.LevelSnapshots.Count;
        double economicScore = totalLevels > 0 ? (healthyLevels / (double)totalLevels) * 100 : 0;

        // Bonus for healthy economy throughout
        if (healthyLevels == totalLevels)
        {
            economicScore = 100; // Perfect economy
        }

        // 2. Combat balance across levels (30%)
        double combatScore = 0;
        int combatTests = 0;
        foreach (int level in new[] { 1, 3, 5 })
        {
            int playerHP = framework.PlayerProgression.BaseHP + (int)(level * framework.PlayerProgression.HPPerLevel);
            int playerSTR = framework.PlayerProgression.BaseSTR + level;
            int playerDEF = framework.PlayerProgression.BaseDEF + level;

            int enemyHP = framework.EnemyProgression.BaseHP + (int)(level * framework.EnemyProgression.HPScalingCoefficient);
            int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(level * framework.EnemyProgression.DamageScalingCoefficient);

            double score = QuickCombatTest(playerHP, playerSTR, playerDEF, level, enemyHP, enemyDMG);
            combatScore += score;
            combatTests++;
        }
        combatScore = combatTests > 0 ? combatScore / combatTests : 0;

        // 3. Build diversity (20%)
        int viableBuilds = framework.Builds.ViableBuilds.Count(b => b.Value.ViabilityScore > 60);
        double buildScore = (viableBuilds / 3.0) * 100;

        // Combined fitness: 50% ECONOMY, 30% combat, 20% build diversity
        double totalFitness = (economicScore * 0.5) + (combatScore * 0.3) + (buildScore * 0.2);

        return totalFitness;
    }

    private static void SaveFrameworkToJSON(ProgressionFrameworkData framework)
    {
        framework.Metadata.SimulationsRun = _generation * 10;
        framework.Metadata.Generation = _generation;
        framework.Metadata.OverallFitness = _bestFitness;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        string json = JsonSerializer.Serialize(framework, options);

        // Safe write with retry logic
        bool success = SafeFileWriter.SafeWriteAllText("progression_framework.json", json);

        if (!success)
        {
            // Fallback to local if network share fails
            try
            {
                File.WriteAllText("progression_framework_backup.json", json);
            }
            catch { /* Silent failure for backup */ }
        }
    }

    private static void GenerateGameCode(ProgressionFrameworkData framework)
    {
        // Generate Equipment.cs
        GenerateEquipmentCode(framework);

        // Generate ProgressionFormulas.cs helper class
        GenerateProgressionFormulasCode(framework);

        // Generate README with discovered patterns
        GenerateProgressionReadme(framework);
    }

    private static void GenerateEquipmentCode(ProgressionFrameworkData framework)
    {
        var code = new System.Text.StringBuilder();

        code.AppendLine("namespace ProjectEvolution.Game;");
        code.AppendLine();
        code.AppendLine("// AUTO-GENERATED from Progression Research");
        code.AppendLine($"// Generation: {_generation}  |  Fitness: {_bestFitness:F2}");
        code.AppendLine($"// Generated: {DateTime.Now}");
        code.AppendLine();

        // Generate Weapon class
        code.AppendLine("public class Weapon");
        code.AppendLine("{");
        code.AppendLine("    public string Name { get; set; }");
        code.AppendLine("    public int BonusStrength { get; set; }");
        code.AppendLine("    public int Value { get; set; }");
        code.AppendLine();
        code.AppendLine("    public Weapon(string name, int bonusStr, int value)");
        code.AppendLine("    {");
        code.AppendLine("        Name = name;");
        code.AppendLine("        BonusStrength = bonusStr;");
        code.AppendLine("        Value = value;");
        code.AppendLine("    }");
        code.AppendLine();
        code.AppendLine("    // AUTO-GENERATED: Balanced weapon progression");
        code.AppendLine("    public static Weapon[] AllWeapons = new[]");
        code.AppendLine("    {");

        string[] weaponNames = { "Rusty Dagger", "Iron Sword", "Steel Blade", "Silver Sword", "Mythril Blade", "Enchanted Blade" };
        for (int i = 0; i < framework.Equipment.WeaponTiers.Count && i < weaponNames.Length; i++)
        {
            var tier = framework.Equipment.WeaponTiers[i];
            string comma = i < framework.Equipment.WeaponTiers.Count - 1 ? "," : "";
            code.AppendLine($"        new Weapon(\"{weaponNames[i]}\", {tier.BonusValue}, {tier.RecommendedCost}){comma}  // Tier {tier.Tier}: +{tier.PowerIncrease:F0}% power");
        }

        code.AppendLine("    };");
        code.AppendLine("}");
        code.AppendLine();

        // Generate Armor class
        code.AppendLine("public class Armor");
        code.AppendLine("{");
        code.AppendLine("    public string Name { get; set; }");
        code.AppendLine("    public int BonusDefense { get; set; }");
        code.AppendLine("    public int Value { get; set; }");
        code.AppendLine();
        code.AppendLine("    public Armor(string name, int bonusDef, int value)");
        code.AppendLine("    {");
        code.AppendLine("        Name = name;");
        code.AppendLine("        BonusDefense = bonusDef;");
        code.AppendLine("        Value = value;");
        code.AppendLine("    }");
        code.AppendLine();
        code.AppendLine("    // AUTO-GENERATED: Balanced armor progression");
        code.AppendLine("    public static Armor[] AllArmor = new[]");
        code.AppendLine("    {");

        string[] armorNames = { "Cloth Rags", "Leather Armor", "Chain Mail", "Plate Armor", "Mythril Plate", "Dragon Scale" };
        for (int i = 0; i < framework.Equipment.ArmorTiers.Count && i < armorNames.Length; i++)
        {
            var tier = framework.Equipment.ArmorTiers[i];
            string comma = i < framework.Equipment.ArmorTiers.Count - 1 ? "," : "";
            code.AppendLine($"        new Armor(\"{armorNames[i]}\", {tier.BonusValue}, {tier.RecommendedCost}){comma}  // Tier {tier.Tier}: +{tier.PowerIncrease:F0}% power");
        }

        code.AppendLine("    };");
        code.AppendLine("}");
        code.AppendLine();

        // Inventory class (unchanged)
        code.AppendLine("public class Inventory");
        code.AppendLine("{");
        code.AppendLine("    public Weapon EquippedWeapon { get; set; }");
        code.AppendLine("    public Armor EquippedArmor { get; set; }");
        code.AppendLine("    public List<Weapon> Weapons { get; set; } = new List<Weapon>();");
        code.AppendLine("    public List<Armor> Armors { get; set; } = new List<Armor>();");
        code.AppendLine();
        code.AppendLine("    public Inventory()");
        code.AppendLine("    {");
        code.AppendLine("        EquippedWeapon = Weapon.AllWeapons[0];");
        code.AppendLine("        EquippedArmor = Armor.AllArmor[0];");
        code.AppendLine("    }");
        code.AppendLine();
        code.AppendLine("    public int GetTotalStrength(int baseStr)");
        code.AppendLine("    {");
        code.AppendLine("        return baseStr + EquippedWeapon.BonusStrength;");
        code.AppendLine("    }");
        code.AppendLine();
        code.AppendLine("    public int GetTotalDefense(int baseDef)");
        code.AppendLine("    {");
        code.AppendLine("        return baseDef + EquippedArmor.BonusDefense;");
        code.AppendLine("    }");
        code.AppendLine("}");

        // Save to GeneratedCode subdirectory
        string genCodePath = Path.Combine(SafeFileWriter.GetOutputPath(), "GeneratedCode");
        try
        {
            Directory.CreateDirectory(genCodePath);
        }
        catch { /* Directory might already exist */ }

        SafeFileWriter.SafeWriteAllText("GeneratedCode/Equipment_Generated.cs", code.ToString(), silent: true);
    }

    private static void GenerateProgressionFormulasCode(ProgressionFrameworkData framework)
    {
        var code = new System.Text.StringBuilder();

        code.AppendLine("namespace ProjectEvolution.Game;");
        code.AppendLine();
        code.AppendLine("// AUTO-GENERATED from Progression Research");
        code.AppendLine($"// Generation: {_generation}  |  Fitness: {_bestFitness:F2}");
        code.AppendLine($"// Generated: {DateTime.Now}");
        code.AppendLine($"// Economy Status: {(framework.Economy.CanAffordProgression ? "✅ HEALTHY" : "❌ BROKEN")}");
        code.AppendLine();
        code.AppendLine("public static class ProgressionFormulas");
        code.AppendLine("{");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // PLAYER PROGRESSION");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine($"    public const int BASE_HP = {framework.PlayerProgression.BaseHP};");
        code.AppendLine($"    public const int HP_PER_LEVEL = {(int)framework.PlayerProgression.HPPerLevel};");
        code.AppendLine();
        code.AppendLine("    public static int CalculateMaxHP(int level)");
        code.AppendLine($"        => BASE_HP + (level * HP_PER_LEVEL);");
        code.AppendLine();
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // ENEMY SCALING");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine($"    public const int BASE_ENEMY_HP = {framework.EnemyProgression.BaseHP};");
        code.AppendLine($"    public const double ENEMY_HP_SCALING = {framework.EnemyProgression.HPScalingCoefficient:F2};");
        code.AppendLine($"    public const int BASE_ENEMY_DAMAGE = {framework.EnemyProgression.BaseDamage};");
        code.AppendLine($"    public const double ENEMY_DAMAGE_SCALING = {framework.EnemyProgression.DamageScalingCoefficient:F2};");
        code.AppendLine();
        code.AppendLine("    public static int CalculateEnemyHP(int baseHP, int playerLevel)");
        code.AppendLine($"        => baseHP + (int)(playerLevel * ENEMY_HP_SCALING);");
        code.AppendLine();
        code.AppendLine("    public static int CalculateEnemyDamage(int baseDMG, int playerLevel)");
        code.AppendLine($"        => baseDMG + (int)(playerLevel * ENEMY_DAMAGE_SCALING);");
        code.AppendLine();
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // ECONOMIC FORMULAS");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine($"    public const int BASE_GOLD_PER_COMBAT = {framework.Economy.BaseGoldPerCombat};");
        code.AppendLine($"    public const double GOLD_SCALING = {framework.Economy.GoldScalingCoefficient:F2};");
        code.AppendLine();
        code.AppendLine("    public static int CalculateGoldReward(int enemyLevel)");
        code.AppendLine($"        => BASE_GOLD_PER_COMBAT + (int)(enemyLevel * GOLD_SCALING);");
        code.AppendLine();
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // LOOT & TREASURE");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine($"    public const int BASE_TREASURE = {framework.Loot.BaseTreasureGold};");
        code.AppendLine($"    public const int TREASURE_PER_DEPTH = {framework.Loot.TreasurePerDungeonDepth};");
        code.AppendLine($"    public const double EQUIPMENT_DROP_RATE = {framework.Loot.EquipmentDropRate:F1};");
        code.AppendLine();
        code.AppendLine("    public static int CalculateTreasureGold(int dungeonDepth)");
        code.AppendLine($"        => BASE_TREASURE + (dungeonDepth * TREASURE_PER_DEPTH);");
        code.AppendLine();
        code.AppendLine("    public static bool RollForEquipmentDrop(int dungeonDepth, Random random)");
        code.AppendLine($"        => random.Next(100) < EQUIPMENT_DROP_RATE + (dungeonDepth * 5);");
        code.AppendLine();
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // EQUIPMENT & ECONOMY");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    public static int GetRecommendedEquipmentTier(int playerLevel)");
        code.AppendLine("        => Math.Min(5, playerLevel / 2);");
        code.AppendLine();
        code.AppendLine("    public static int GetEquipmentCost(int tier)");
        code.AppendLine("        => tier * tier * 25 + 5; // Quadratic cost scaling");
        code.AppendLine();
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    // BUILD VALIDATION");
        code.AppendLine("    // ═══════════════════════════════════════════════");
        code.AppendLine("    public static bool IsBuildViable(int str, int def, int totalPoints)");
        code.AppendLine("    {");
        code.AppendLine("        double strRatio = (double)str / totalPoints;");
        code.AppendLine("        return strRatio >= 0.2 && strRatio <= 0.8; // 20-80% STR is viable");
        code.AppendLine("    }");
        code.AppendLine("}");

        SafeFileWriter.SafeWriteAllText("GeneratedCode/ProgressionFormulas_Generated.cs", code.ToString(), silent: true);
    }

    private static void GenerateProgressionReadme(ProgressionFrameworkData framework)
    {
        var readme = new System.Text.StringBuilder();

        readme.AppendLine("# PROGRESSION FRAMEWORK - AUTO-GENERATED");
        readme.AppendLine($"**Generated:** {DateTime.Now}");
        readme.AppendLine($"**Generation:** {_generation}");
        readme.AppendLine($"**Fitness Score:** {_bestFitness:F2}");
        readme.AppendLine($"**Economy Status:** {(framework.Economy.CanAffordProgression ? "✅ HEALTHY" : "❌ BROKEN")}");
        readme.AppendLine();
        readme.AppendLine("## Player Progression");
        readme.AppendLine($"- **Base HP:** {framework.PlayerProgression.BaseHP}");
        readme.AppendLine($"- **HP Per Level:** {framework.PlayerProgression.HPPerLevel:F1}");
        readme.AppendLine($"- **Formula:** `{framework.PlayerProgression.MaxHPFormula}`");
        readme.AppendLine();
        readme.AppendLine("## Enemy Progression");
        readme.AppendLine($"- **Base HP:** {framework.EnemyProgression.BaseHP}");
        readme.AppendLine($"- **HP Scaling:** ×{framework.EnemyProgression.HPScalingCoefficient:F2} per player level");
        readme.AppendLine($"- **Base Damage:** {framework.EnemyProgression.BaseDamage}");
        readme.AppendLine($"- **Damage Scaling:** ×{framework.EnemyProgression.DamageScalingCoefficient:F2} per player level");
        readme.AppendLine($"- **HP Formula:** `{framework.EnemyProgression.HPFormula}`");
        readme.AppendLine($"- **Damage Formula:** `{framework.EnemyProgression.DamageFormula}`");
        readme.AppendLine();
        readme.AppendLine("## Economy & Gold");
        readme.AppendLine($"- **Base Gold per Combat:** {framework.Economy.BaseGoldPerCombat}g");
        readme.AppendLine($"- **Gold Scaling:** +{framework.Economy.GoldScalingCoefficient:F2}g per enemy level");
        readme.AppendLine($"- **Gold Formula:** `{framework.Economy.GoldFormula}`");
        readme.AppendLine();
        readme.AppendLine("### Economic Progression Simulation:");
        readme.AppendLine("| Level | Gold Earned | Cumulative | Can Afford | Status |");
        readme.AppendLine("|-------|-------------|------------|------------|--------|");
        foreach (var snapshot in framework.Economy.LevelSnapshots)
        {
            string status = snapshot.EconomyHealthy ? "✅" : "❌";
            readme.AppendLine($"| {snapshot.Level,2} | {snapshot.ExpectedGoldEarned,6}g | {snapshot.CumulativeGold,7}g | Tier {snapshot.AffordableEquipmentTier} | {status} |");
        }
        readme.AppendLine();
        readme.AppendLine("## Loot & Treasure");
        readme.AppendLine($"- **Base Treasure:** {framework.Loot.BaseTreasureGold}g");
        readme.AppendLine($"- **Treasure per Depth:** +{framework.Loot.TreasurePerDungeonDepth}g");
        readme.AppendLine($"- **Equipment Drop Rate:** {framework.Loot.EquipmentDropRate:F0}%");
        readme.AppendLine($"- **Treasure Formula:** `{framework.Loot.TreasureFormula}`");
        readme.AppendLine();
        readme.AppendLine("### Dungeon Loot Tables:");
        readme.AppendLine("| Depth | Gold Range | Equip Drop % | Tier |");
        readme.AppendLine("|-------|------------|--------------|------|");
        foreach (var loot in framework.Loot.DungeonLoot)
        {
            readme.AppendLine($"| {loot.DungeonDepth} | {loot.MinGold}-{loot.MaxGold}g | {loot.EquipmentDropChance:F0}% | {loot.RecommendedEquipmentTier} |");
        }
        readme.AppendLine();
        readme.AppendLine("## Equipment Tiers");
        readme.AppendLine("### Weapons:");
        foreach (var tier in framework.Equipment.WeaponTiers)
        {
            readme.AppendLine($"- **Tier {tier.Tier}:** +{tier.BonusValue} STR | Cost: {tier.RecommendedCost}g | Recommended Level: {tier.RecommendedLevel}+");
        }
        readme.AppendLine();
        readme.AppendLine("### Armor:");
        foreach (var tier in framework.Equipment.ArmorTiers)
        {
            readme.AppendLine($"- **Tier {tier.Tier}:** +{tier.BonusValue} DEF | Cost: {tier.RecommendedCost}g | Recommended Level: {tier.RecommendedLevel}+");
        }
        readme.AppendLine();
        readme.AppendLine("## Build Viability");
        foreach (var (name, reqs) in framework.Builds.ViableBuilds)
        {
            readme.AppendLine($"### {name}");
            readme.AppendLine($"- **Min HP Required:** {reqs.MinBaseHP}");
            readme.AppendLine($"- **Recommended Ratio:** {reqs.RecommendedRatio.str}:{reqs.RecommendedRatio.def}");
            readme.AppendLine($"- **Viability Score:** {reqs.ViabilityScore:F1}%");
            readme.AppendLine();
        }

        SafeFileWriter.SafeWriteAllText("PROGRESSION_FRAMEWORK.md", readme.ToString(), silent: true);
    }

    private static void RenderResearchDashboard()
    {
        if (_bestFramework == null) return;

        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeSpan sinceSave = DateTime.Now - _lastSaveTime;

        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      🧬 CONTINUOUS PROGRESSION FRAMEWORK RESEARCH 🧬           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Status bar
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"⏱️  RUNTIME: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}  |  Gen: {_generation}  |  Sims: {_totalSimulations}  |  Fitness: {_bestFitness:F2}");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"STATUS: {_currentPhase}");
        Console.ResetColor();
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"Last save: {sinceSave.TotalSeconds:F0}s ago  |  Gens since improvement: {_generationsSinceImprovement}");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("📊 CURRENT BEST FORMULAS:");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"  Player: {_bestFramework.PlayerProgression.BaseHP}HP + (Lvl×{_bestFramework.PlayerProgression.HPPerLevel:F1})");
        Console.WriteLine($"  Enemy:  {_bestFramework.EnemyProgression.BaseHP}HP + (Lvl×{_bestFramework.EnemyProgression.HPScalingCoefficient:F2})  |  {_bestFramework.EnemyProgression.BaseDamage}DMG + (Lvl×{_bestFramework.EnemyProgression.DamageScalingCoefficient:F2})");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine($"  Gold:   {_bestFramework.Economy.BaseGoldPerCombat}g + (Lvl×{_bestFramework.Economy.GoldScalingCoefficient:F2})");
        Console.WriteLine($"  Loot:   {_bestFramework.Loot.BaseTreasureGold}g + (Depth×{_bestFramework.Loot.TreasurePerDungeonDepth})  |  Drop: {_bestFramework.Loot.EquipmentDropRate:F0}%");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("💰 ECONOMIC SIMULATION (Levels 1-10):");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.WriteLine("  Lvl | Gold Earned | Total Gold | Can Afford | Rec. Tier");

        foreach (var snapshot in _bestFramework.Economy.LevelSnapshots.Take(5)) // Show first 5 levels
        {
            string status = snapshot.EconomyHealthy ? "✅" : "❌";
            Console.WriteLine($"  {status} {snapshot.Level,2} | {snapshot.ExpectedGoldEarned,6}g      | {snapshot.CumulativeGold,6}g    | Tier {snapshot.AffordableEquipmentTier}     | Tier {snapshot.RecommendedEquipmentTier}");
        }

        if (_bestFramework.Economy.LevelSnapshots.Count > 5)
        {
            var lastSnapshot = _bestFramework.Economy.LevelSnapshots.Last();
            Console.WriteLine($"  ... | ...         | ...        | ...        | ...");
            string status = lastSnapshot.EconomyHealthy ? "✅" : "❌";
            Console.WriteLine($"  {status}{lastSnapshot.Level,2} | {lastSnapshot.ExpectedGoldEarned,6}g      | {lastSnapshot.CumulativeGold,6}g    | Tier {lastSnapshot.AffordableEquipmentTier}     | Tier {lastSnapshot.RecommendedEquipmentTier}");
        }

        Console.ForegroundColor = _bestFramework.Economy.CanAffordProgression ? ConsoleColor.Green : ConsoleColor.Red;
        Console.WriteLine($"\n  Economy Status: {(_bestFramework.Economy.CanAffordProgression ? "✅ HEALTHY" : "❌ BROKEN")}");
        Console.ResetColor();
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.WriteLine("🎁 LOOT TABLES:");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.WriteLine($"  Treasure: {_bestFramework.Loot.BaseTreasureGold}g + (Depth × {_bestFramework.Loot.TreasurePerDungeonDepth}g)");
        Console.WriteLine($"  Equipment Drop Rate: {_bestFramework.Loot.EquipmentDropRate:F0}% (increases with depth)");
        Console.WriteLine();

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("✅ BUILD VIABILITY:");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        foreach (var (name, reqs) in _bestFramework.Builds.ViableBuilds)
        {
            string status = reqs.ViabilityScore > 60 ? "✅" : "❌";
            Console.WriteLine($"  {status} {name,-15} Viability: {reqs.ViabilityScore:F0}%");
        }
        Console.WriteLine();

        // Footer with controls and auto-save status
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine($"💾 Auto-saves every {AUTO_SAVE_INTERVAL_GENERATIONS} gens  |  Output: {SafeFileWriter.GetOutputPath()}");
        Console.WriteLine($"📁 Files: progression_*.json/.log/.md  |  GeneratedCode/*.cs");
        Console.WriteLine($"🔒 Lock-safe writes with retry (network share compatible)");
        Console.ResetColor();
        Console.WriteLine($"{"─",64}");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press [ESC] to stop and save  |  Files update automatically");
        Console.ResetColor();
    }

    private static void LogProgress(ProgressionFrameworkData framework, double fitness, bool improved)
    {
        TimeSpan elapsed = DateTime.Now - _startTime;

        var log = new System.Text.StringBuilder();
        log.AppendLine($"\n[{DateTime.Now:HH:mm:ss}] Generation {_generation} - Runtime: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}");
        log.AppendLine($"  Fitness: {fitness:F2} (Best: {_bestFitness:F2}) {(improved ? "🌟 IMPROVED!" : "")}");
        log.AppendLine($"  HP/Level: {framework.PlayerProgression.HPPerLevel:F1}  |  Enemy HP×{framework.EnemyProgression.HPScalingCoefficient:F2}  |  Gold×{framework.Economy.GoldScalingCoefficient:F2}");
        log.AppendLine($"  Economy: {(framework.Economy.CanAffordProgression ? "✅ Healthy" : "❌ Broken")}  |  Viable Builds: {framework.Builds.ViableBuilds.Count(b => b.Value.ViabilityScore > 60)}/3");

        if (improved)
        {
            log.AppendLine("  📝 Code regenerated!");
            log.AppendLine($"  📊 Economic snapshots:");
            foreach (var snapshot in framework.Economy.LevelSnapshots.Where((s, i) => i % 2 == 0)) // Every other level
            {
                log.AppendLine($"     Lvl {snapshot.Level}: {snapshot.CumulativeGold,5}g → {(snapshot.EconomyHealthy ? "✅" : "❌")} Tier {snapshot.AffordableEquipmentTier}");
            }
        }

        log.AppendLine($"  Gens since improvement: {_generationsSinceImprovement}");

        // Append to log file with lock protection
        SafeFileWriter.SafeAppendAllText("progression_research.log", log.ToString());

        // Keep a rolling summary (last 100 generations)
        if (_generation % 10 == 0)
        {
            var summary = $"[Gen {_generation}] Fitness: {_bestFitness:F2} | Elapsed: {elapsed.TotalMinutes:F1}m | Economy: {(_bestFramework?.Economy.CanAffordProgression == true ? "✅" : "❌")}\n";
            SafeFileWriter.SafeAppendAllText("progression_summary.log", summary);
        }
    }

    private static void UpdateStatusLine()
    {
        if (_bestFramework == null) return;

        TimeSpan elapsed = DateTime.Now - _startTime;
        TimeSpan sinceSave = DateTime.Now - _lastSaveTime;

        // Update just the status lines without clearing screen
        Console.SetCursorPosition(0, 4);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"⏱️  RUNTIME: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}  |  Gen: {_generation,4}  |  Sims: {_totalSimulations,6}  |  Fitness: {_bestFitness,6:F2}");
        Console.ResetColor();

        Console.SetCursorPosition(0, 6);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(_currentPhase.PadRight(70));
        Console.ResetColor();

        Console.SetCursorPosition(0, 7);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.Write($"Last save: {sinceSave.TotalSeconds:F0}s ago  |  No improvement for: {_generationsSinceImprovement} gens".PadRight(70));
        Console.ResetColor();

        // Show current test values on line 8
        Console.SetCursorPosition(0, 8);
        Console.ForegroundColor = ConsoleColor.DarkCyan;
        string economyStatus = _bestFramework.Economy.CanAffordProgression ? "✅" : "❌";
        Console.Write($"Economy: {economyStatus}  Gold: {_bestFramework.Economy.BaseGoldPerCombat}g+(×{_bestFramework.Economy.GoldScalingCoefficient:F2})  Treasure: {_bestFramework.Loot.BaseTreasureGold}g+(×{_bestFramework.Loot.TreasurePerDungeonDepth})".PadRight(70));
        Console.ResetColor();
    }
}


