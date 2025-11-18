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
    private static List<MetricResult>? _latestMetricResults = null;
    private static int _lastConsoleWidth = 0;
    private static int _lastConsoleHeight = 0;

    // Champion/Rubric System - tracks best runs across resets
    private static ProgressionFrameworkData? _champion = null;
    private static double _championFitness = 0;
    private static int _championGeneration = 0;
    private static int _resetCount = 0;
    private const double AUTO_RESET_THRESHOLD = 85.0; // Reset if stuck near optimal
    private const int AUTO_RESET_STUCK_GENS = 150; // Must be stuck this long

    // Anti-flicker settings
    private const int UI_UPDATE_INTERVAL = 5; // Update UI every N generations
    private const int CYCLE_DELAY_MS = 200; // Delay between generations (was 50ms)

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

        // Load champion if exists
        LoadChampion();
        if (_champion != null)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"🏆 Champion loaded: Gen {_championGeneration}, Fitness {_championFitness:F2}");
            Console.WriteLine("   This is your rubric - new runs will be compared against it!");
            Console.ResetColor();
            Console.WriteLine();
        }

        Console.WriteLine("Press ESC to stop, any other key to start...");
        if (Console.ReadKey().Key == ConsoleKey.Escape) return;

        // === ANTI-FLICKER SETUP ===
        Console.CursorVisible = false; // Hide cursor to prevent blinking
        try
        {
            // Set buffer size to prevent auto-scroll
            Console.SetBufferSize(Console.WindowWidth, Math.Max(50, Console.WindowHeight));
        }
        catch
        {
            // Some terminals don't support this
        }

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
                if (_generation % UI_UPDATE_INTERVAL == 0) UpdateStatusLine();
                framework = CreateBaselineFramework(); // Start fresh
                _generationsSinceImprovement = 0;
                Thread.Sleep(500); // Let user see restart
            }
            else
            {
                _currentPhase = "🔬 Mutating parameters...";
                framework = MutateFramework(_bestFramework ?? CreateBaselineFramework());
            }
            _totalSimulations += 50;

            // PHASE 2: Evaluate fitness
            _currentPhase = "📊 Evaluating...";

            double fitness = EvaluateFramework(framework);
            _totalSimulations += 15;

            // PHASE 3: Update if better
            bool improved = false;
            if (fitness > _bestFitness || _bestFramework == null)
            {
                _currentPhase = "🌟 NEW BEST! Saving & generating code...";

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
                Thread.Sleep(300); // Brief pause to see new values
            }
            else
            {
                _generationsSinceImprovement++;
            }

            // PHASE 4: Auto-save every 5 generations
            if (_generation % AUTO_SAVE_INTERVAL_GENERATIONS == 0)
            {
                _currentPhase = "💾 Auto-save...";
                if (_bestFramework != null)
                {
                    SaveFrameworkToJSON(_bestFramework);
                }
                _lastSaveTime = DateTime.Now;
            }

            // PHASE 5: Update status (only every N generations to reduce flicker)
            _currentPhase = improved ? "✅ Improved!" : "🔄 Searching...";
            if (_generation % UI_UPDATE_INTERVAL == 0)
            {
                UpdateStatusLine();
            }

            // Log progress to file
            LogProgress(framework, fitness, improved);

            // Check for ESC or R (reset)
            if (Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    _currentPhase = "💾 Saving...";
                    UpdateStatusLine();
                    if (_bestFramework != null)
                    {
                        SaveFrameworkToJSON(_bestFramework);
                    }
                    break;
                }
                else if (key == ConsoleKey.R)
                {
                    // Manual reset - promote current best to champion
                    PerformReset(manual: true);
                }
            }

            // AUTOMATED RESET: If stuck near optimal, reset and explore new space
            if (_bestFitness >= AUTO_RESET_THRESHOLD && _generationsSinceImprovement >= AUTO_RESET_STUCK_GENS)
            {
                PerformReset(manual: false);
            }

            // Controlled cycle - prevent flicker while maintaining responsiveness
            Thread.Sleep(CYCLE_DELAY_MS);
        }

        // === CLEANUP ON EXIT ===
        Console.CursorVisible = true; // Restore cursor
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

    private static void LoadChampion()
    {
        string championPath = SafeFileWriter.GetFullPath("progression_champion.json");
        if (File.Exists(championPath))
        {
            try
            {
                string json = File.ReadAllText(championPath);
                _champion = JsonSerializer.Deserialize<ProgressionFrameworkData>(json);
                if (_champion != null)
                {
                    _championFitness = _champion.Metadata.OverallFitness;
                    _championGeneration = _champion.Metadata.Generation;
                }
            }
            catch
            {
                // If champion load fails, no problem
            }
        }
    }

    private static void SaveChampion()
    {
        if (_bestFramework == null) return;

        _champion = _bestFramework;
        _championFitness = _bestFitness;
        _championGeneration = _generation;

        var options = new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        };

        string json = JsonSerializer.Serialize(_champion, options);
        SafeFileWriter.SafeWriteAllText("progression_champion.json", json);

        // Also save a timestamped backup
        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        SafeFileWriter.SafeWriteAllText($"progression_champion_{timestamp}.json", json);
    }

    private static void PerformReset(bool manual)
    {
        _currentPhase = manual ? "🔄 MANUAL RESET - Saving champion..." : "🔄 AUTO RESET - Near optimal, exploring new space...";
        Console.SetCursorPosition(0, 6);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write(_currentPhase.PadRight(80));
        Console.ResetColor();
        Thread.Sleep(1000); // Let user see the message

        // Save current best as champion
        if (_bestFramework != null && _bestFitness > _championFitness)
        {
            SaveChampion();
            _currentPhase = $"🏆 NEW CHAMPION! Fitness: {_bestFitness:F2} (was {_championFitness:F2})";
            Console.SetCursorPosition(0, 6);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write(_currentPhase.PadRight(80));
            Console.ResetColor();
            Thread.Sleep(2000);
        }

        // Log the reset
        var log = new System.Text.StringBuilder();
        log.AppendLine($"\n{'═',64}");
        log.AppendLine($"[{DateTime.Now:HH:mm:ss}] RESET #{++_resetCount} - {(manual ? "MANUAL" : "AUTO")}");
        log.AppendLine($"  Previous run: Gen {_generation}, Fitness {_bestFitness:F2}");
        if (_champion != null)
        {
            log.AppendLine($"  Champion: Gen {_championGeneration}, Fitness {_championFitness:F2}");
        }
        log.AppendLine($"  Starting fresh exploration...");
        log.AppendLine($"{'═',64}");
        SafeFileWriter.SafeAppendAllText("progression_research.log", log.ToString());

        // Reset to baseline
        _bestFramework = CreateBaselineFramework();
        _bestFitness = EvaluateFramework(_bestFramework);
        _generation = 0;
        _generationsSinceImprovement = 0;

        // Full redraw
        Console.SetCursorPosition(0, 0);
        RenderResearchDashboard();
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

        // ADAPTIVE MUTATION: Increase exploration if stuck, fine-tune if improving
        double mutationRate = 0.3; // Base rate
        double mutationStrength = 1.0;

        if (_generationsSinceImprovement > 100)
        {
            // Stuck! Explore more aggressively
            mutationRate = 0.7;
            mutationStrength = 2.0;
        }
        else if (_generationsSinceImprovement > 50)
        {
            // Getting stuck, increase exploration
            mutationRate = 0.5;
            mutationStrength = 1.5;
        }
        else if (_generationsSinceImprovement < 10)
        {
            // Improving! Fine-tune
            mutationRate = 0.2;
            mutationStrength = 0.5;
        }

        // Mutate player progression
        mutated.PlayerProgression.BaseHP = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.PlayerProgression.BaseHP + (int)((random.Next(-3, 4) * mutationStrength)), 15, 40)
            : parent.PlayerProgression.BaseHP;

        mutated.PlayerProgression.HPPerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.PlayerProgression.HPPerLevel + random.Next(-1, 2), 1, 5)
            : parent.PlayerProgression.HPPerLevel;

        mutated.PlayerProgression.BaseSTR = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.PlayerProgression.BaseSTR + random.Next(-1, 2), 2, 5)
            : parent.PlayerProgression.BaseSTR;

        mutated.PlayerProgression.BaseDEF = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.PlayerProgression.BaseDEF + random.Next(-1, 2), 0, 3)
            : parent.PlayerProgression.BaseDEF;

        mutated.PlayerProgression.StatPointsPerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.PlayerProgression.StatPointsPerLevel + random.Next(-1, 2), 1, 3)
            : parent.PlayerProgression.StatPointsPerLevel;

        // Mutate enemy progression
        mutated.EnemyProgression.BaseHP = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.EnemyProgression.BaseHP + (int)((random.Next(-2, 3) * mutationStrength)), 3, 12)
            : parent.EnemyProgression.BaseHP;

        mutated.EnemyProgression.HPScalingCoefficient = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.EnemyProgression.HPScalingCoefficient + (random.NextDouble() - 0.5) * 0.5 * mutationStrength, 0.5, 3.0)
            : parent.EnemyProgression.HPScalingCoefficient;

        mutated.EnemyProgression.BaseDamage = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.EnemyProgression.BaseDamage + (int)((random.Next(-1, 2) * mutationStrength)), 1, 5)
            : parent.EnemyProgression.BaseDamage;

        mutated.EnemyProgression.DamageScalingCoefficient = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.EnemyProgression.DamageScalingCoefficient + (random.NextDouble() - 0.5) * 0.3 * mutationStrength, 0.1, 1.0)
            : parent.EnemyProgression.DamageScalingCoefficient;

        // CRITICAL: Mutate ECONOMY parameters (higher rate for critical system)
        var economy = new EconomicProgression();
        economy.BaseGoldPerCombat = random.NextDouble() < (mutationRate * 1.5) // 1.5x higher chance
            ? Math.Clamp(parent.Economy.BaseGoldPerCombat + (int)((random.Next(-3, 4) * mutationStrength)), 8, 20)
            : parent.Economy.BaseGoldPerCombat;

        economy.GoldScalingCoefficient = random.NextDouble() < (mutationRate * 1.5)
            ? Math.Clamp(parent.Economy.GoldScalingCoefficient + (random.NextDouble() - 0.5) * 2.0 * mutationStrength, 2.0, 6.0)
            : parent.Economy.GoldScalingCoefficient;

        mutated.Economy = economy;

        // Mutate LOOT parameters
        var loot = new LootProgression();
        loot.BaseTreasureGold = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Loot.BaseTreasureGold + (int)((random.Next(-10, 11) * mutationStrength)), 10, 50)
            : parent.Loot.BaseTreasureGold;

        loot.TreasurePerDungeonDepth = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Loot.TreasurePerDungeonDepth + (int)((random.Next(-10, 11) * mutationStrength)), 15, 60)
            : parent.Loot.TreasurePerDungeonDepth;

        loot.EquipmentDropRate = random.NextDouble() < mutationRate
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
                PowerIncrease = 0 // Calculate after all tiers are added
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
                PowerIncrease = 0 // Calculate after all tiers are added
            });
        }

        // Now calculate PowerIncrease based on actual BonusValues
        for (int i = 1; i < progression.WeaponTiers.Count; i++)
        {
            var prev = progression.WeaponTiers[i - 1];
            var curr = progression.WeaponTiers[i];

            // Handle tier 0→1 specially (0 bonus to non-zero)
            if (prev.BonusValue == 0 && curr.BonusValue > 0)
            {
                curr.PowerIncrease = 100; // First weapon = 100% increase from nothing
            }
            else if (prev.BonusValue > 0)
            {
                curr.PowerIncrease = ((curr.BonusValue - prev.BonusValue) / (double)prev.BonusValue) * 100;
            }
        }

        for (int i = 1; i < progression.ArmorTiers.Count; i++)
        {
            var prev = progression.ArmorTiers[i - 1];
            var curr = progression.ArmorTiers[i];

            if (prev.BonusValue == 0 && curr.BonusValue > 0)
            {
                curr.PowerIncrease = 100; // First armor = 100% increase from nothing
            }
            else if (prev.BonusValue > 0)
            {
                curr.PowerIncrease = ((curr.BonusValue - prev.BonusValue) / (double)prev.BonusValue) * 100;
            }
        }

        progression.RecommendedFormula = "Flat linear: Bonus = Tier";

        return progression;
    }

    private static BuildViability TestBuildViabilityQuick(ProgressionFrameworkData framework)
    {
        var viability = new BuildViability();

        // Test 3 archetypes at level 5 (mid-game)
        int testLevel = 5;
        int baseHP = framework.PlayerProgression.BaseHP + (testLevel * (int)framework.PlayerProgression.HPPerLevel);
        int enemyHP = framework.EnemyProgression.BaseHP + (int)(testLevel * framework.EnemyProgression.HPScalingCoefficient);
        int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(testLevel * framework.EnemyProgression.DamageScalingCoefficient);

        // Test 3 archetypes with different stat allocations
        var builds = new[]
        {
            ("GlassCannon", 10, 2),  // High offense, low defense
            ("Balanced", 6, 6),      // Even split
            ("Tank", 2, 10)          // Low offense, high defense (was 3, now 2 for more variance)
        };

        foreach (var (name, str, def) in builds)
        {
            // Test this build against actual framework enemy stats
            double viabilityScore = QuickCombatTest(baseHP, str, def, testLevel, enemyHP, enemyDMG);

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
        // Use the new comprehensive metric system
        var (totalFitness, metricResults) = FitnessEvaluator.EvaluateComprehensive(framework);

        // Store latest metric results for dashboard
        _latestMetricResults = metricResults;

        // Log detailed breakdown every 50 generations for analysis
        if (_generation % 50 == 0 && totalFitness > _bestFitness)
        {
            var report = FitnessEvaluator.GetDetailedReport(metricResults);
            SafeFileWriter.SafeAppendAllText("progression_research.log", report);
        }

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

        // Clear and redraw header only
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      🧬 CONTINUOUS PROGRESSION FRAMEWORK RESEARCH 🧬           ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine();

        // Use UpdateStatusLine for all the dynamic content (anti-flicker)
        UpdateStatusLine();
    }

    private static string GenerateBar(double value, int width)
    {
        int filled = (int)Math.Round((value / 100.0) * width);
        filled = Math.Clamp(filled, 0, width);
        return new string('█', filled) + new string('░', width - filled);
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

        // Detect window size and adapt
        int consoleWidth = 80;
        int consoleHeight = 30;
        try
        {
            consoleWidth = Console.WindowWidth;
            consoleHeight = Console.WindowHeight;
        }
        catch
        {
            // If console size detection fails, use defaults
        }

        // Detect window resize and trigger full redraw
        if (_lastConsoleWidth != consoleWidth || _lastConsoleHeight != consoleHeight)
        {
            _lastConsoleWidth = consoleWidth;
            _lastConsoleHeight = consoleHeight;

            // Clear screen on resize
            try
            {
                Console.Clear();
                Console.SetCursorPosition(0, 0);
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
                Console.WriteLine("║      🧬 CONTINUOUS PROGRESSION FRAMEWORK RESEARCH 🧬           ║");
                Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
                Console.WriteLine();
                Console.ResetColor();
            }
            catch
            {
                // Ignore errors during resize
            }
        }

        // Minimum width to display properly
        if (consoleWidth < 80)
        {
            Console.Clear();
            Console.WriteLine("⚠️  Window too narrow! Please resize to at least 80 characters wide.");
            Console.WriteLine("   Current width: {0}, Minimum required: 80", consoleWidth);
            return;
        }

        // Helper to safely write a line
        void SafeWriteLine(int row, string content, ConsoleColor color = ConsoleColor.White)
        {
            try
            {
                if (row >= consoleHeight) return;
                Console.SetCursorPosition(0, row);
                Console.ForegroundColor = color;
                // Truncate or pad to fit window width
                if (content.Length > consoleWidth - 1)
                    content = content.Substring(0, consoleWidth - 1);
                else
                    content = content.PadRight(consoleWidth - 1);
                Console.Write(content);
                Console.ResetColor();
            }
            catch
            {
                // Ignore cursor positioning errors during resize
            }
        }

        // Update just the status lines without clearing screen
        string fitnessDisplay = _champion != null
            ? $"Fitness: {_bestFitness,6:F2} | 🏆 Champion: {_championFitness:F2} (Gen {_championGeneration})"
            : $"Fitness: {_bestFitness,6:F2}";
        SafeWriteLine(4, $"⏱️  RUNTIME: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}  |  Gen: {_generation,4}  |  {fitnessDisplay}", ConsoleColor.Yellow);
        SafeWriteLine(6, _currentPhase, ConsoleColor.Cyan);

        string resetInfo = _resetCount > 0 ? $"  |  Resets: {_resetCount}" : "";
        SafeWriteLine(7, $"Last save: {sinceSave.TotalSeconds:F0}s ago  |  No improvement: {_generationsSinceImprovement} gens{resetInfo}", ConsoleColor.DarkGray);

        // === ENHANCED PARAMETER DISPLAY (NON-SCROLLING) ===
        int row = 9;

        // Player Progression Parameters
        SafeWriteLine(row++, "👤 PLAYER:", ConsoleColor.Green);
        SafeWriteLine(row++, $"   BaseHP={_bestFramework.PlayerProgression.BaseHP,3}  HPPerLvl={_bestFramework.PlayerProgression.HPPerLevel:F1}  BaseSTR={_bestFramework.PlayerProgression.BaseSTR,2}  BaseDEF={_bestFramework.PlayerProgression.BaseDEF,2}  StatPts/Lvl={_bestFramework.PlayerProgression.StatPointsPerLevel}", ConsoleColor.Green);

        // Enemy Progression Parameters
        SafeWriteLine(row++, "👹 ENEMY:", ConsoleColor.Red);
        SafeWriteLine(row++, $"   BaseHP={_bestFramework.EnemyProgression.BaseHP,3}  HPScale={_bestFramework.EnemyProgression.HPScalingCoefficient:F2}  BaseDMG={_bestFramework.EnemyProgression.BaseDamage,2}  DMGScale={_bestFramework.EnemyProgression.DamageScalingCoefficient:F2}", ConsoleColor.Red);

        // Economy Parameters
        string economyStatus = _bestFramework.Economy.CanAffordProgression ? "✅" : "❌";
        SafeWriteLine(row++, $"💰 ECONOMY {economyStatus}:", ConsoleColor.Yellow);
        SafeWriteLine(row++, $"   BaseGold={_bestFramework.Economy.BaseGoldPerCombat,3}g  GoldScale={_bestFramework.Economy.GoldScalingCoefficient:F2}  [Formula: {_bestFramework.Economy.BaseGoldPerCombat}+Lvl×{_bestFramework.Economy.GoldScalingCoefficient:F2}]", ConsoleColor.Yellow);

        // Loot Parameters
        SafeWriteLine(row++, "🎁 LOOT:", ConsoleColor.Magenta);
        SafeWriteLine(row++, $"   BaseTreasure={_bestFramework.Loot.BaseTreasureGold,3}g  DepthScale={_bestFramework.Loot.TreasurePerDungeonDepth,3}g  DropRate={_bestFramework.Loot.EquipmentDropRate:F0}%", ConsoleColor.Magenta);

        // Equipment Tiers (show first 3 tiers)
        SafeWriteLine(row++, "⚔️  EQUIPMENT:", ConsoleColor.Cyan);
        var wepTiers = _bestFramework.Equipment.WeaponTiers.Take(3).ToList();
        SafeWriteLine(row++, $"   Weapons T0-2: [{string.Join(", ", wepTiers.Select(t => $"+{t.BonusValue}={t.RecommendedCost}g"))}]", ConsoleColor.Cyan);
        var armTiers = _bestFramework.Equipment.ArmorTiers.Take(3).ToList();
        SafeWriteLine(row++, $"   Armor   T0-2: [{string.Join(", ", armTiers.Select(t => $"+{t.BonusValue}={t.RecommendedCost}g"))}]", ConsoleColor.Cyan);

        // Live Metrics (if available)
        if (_latestMetricResults != null && _latestMetricResults.Count > 0)
        {
            SafeWriteLine(row++, "⚡ METRICS:", ConsoleColor.White);

            foreach (var metric in _latestMetricResults)
            {
                ConsoleColor color = metric.Score >= 80 ? ConsoleColor.Green :
                                    metric.Score >= 60 ? ConsoleColor.Yellow :
                                    ConsoleColor.Red;
                string bar = GenerateBar(metric.Score, 15);
                // Fixed width formatting: name(22) + bar(15) + score(6) + arrow(3) + weighted(6)
                string metricName = metric.MetricName.PadRight(22);
                SafeWriteLine(row++, $"   {metricName} {bar} {metric.Score,6:F1} → {metric.WeightedScore,6:F2}", color);
            }
        }

        // Mutation strategy indicator
        string strategy = _generationsSinceImprovement > 100 ? "🔥 AGGRESSIVE EXPLORATION" :
                         _generationsSinceImprovement > 50 ? "🔍 MODERATE SEARCH" :
                         _generationsSinceImprovement < 10 ? "🎯 FINE-TUNING" :
                         "⚖️  BALANCED";
        SafeWriteLine(row++, $"Strategy: {strategy}", ConsoleColor.DarkGray);

        // Footer with controls
        SafeWriteLine(row++, "", ConsoleColor.Black);
        SafeWriteLine(row++, "────────────────────────────────────────────────────────────────", ConsoleColor.DarkGray);
        string autoResetStatus = _bestFitness >= AUTO_RESET_THRESHOLD
            ? $"Auto-reset in {AUTO_RESET_STUCK_GENS - _generationsSinceImprovement} gens (fitness ≥{AUTO_RESET_THRESHOLD})"
            : $"Auto-reset at fitness ≥{AUTO_RESET_THRESHOLD} + {AUTO_RESET_STUCK_GENS} stuck gens";
        SafeWriteLine(row++, autoResetStatus, ConsoleColor.DarkCyan);
        SafeWriteLine(row++, "[ESC] Stop & Save  |  [R] Manual Reset → New Champion", ConsoleColor.Yellow);

        // Clear any remaining lines from previous longer displays
        for (int i = row; i < consoleHeight - 1; i++)
        {
            SafeWriteLine(i, "", ConsoleColor.Black);
        }
    }
}

// ═══════════════════════════════════════════════════════════════════════════════
// EXTENSIBLE FITNESS METRIC SYSTEM
// Add new metrics as features are added - they automatically integrate
//
// DOCUMENTATION: See ../FITNESS_SYSTEM.md for comprehensive guide on:
//   - Metric architecture and how it works
//   - How to add new metrics (2 simple steps)
//   - Expected improvements and testing guidance
//   - Future metric ideas (DungeonBalance, SkillTree, etc.)
// ═══════════════════════════════════════════════════════════════════════════════

public interface IFitnessMetric
{
    string Name { get; }
    double Weight { get; } // Contribution to total fitness (0.0 - 1.0)
    MetricResult Evaluate(ProgressionFrameworkData framework);
}

public class MetricResult
{
    public string MetricName { get; set; } = "";
    public double Score { get; set; } // 0-100
    public double WeightedScore { get; set; } // Score * Weight
    public List<string> Details { get; set; } = new();
    public List<string> Warnings { get; set; } = new();
    public bool Critical { get; set; } // If true and score < 50, entire fitness = 0
}

public class CombatBalanceMetric : IFitnessMetric
{
    public string Name => "Combat Balance";
    public double Weight => 0.25;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };
        var scores = new List<double>();

        // Test ALL levels 1-10, not just 1,3,5
        for (int level = 1; level <= 10; level++)
        {
            int playerHP = framework.PlayerProgression.BaseHP + (int)(level * framework.PlayerProgression.HPPerLevel);
            int playerSTR = framework.PlayerProgression.BaseSTR + level;
            int playerDEF = framework.PlayerProgression.BaseDEF + level;

            int enemyHP = framework.EnemyProgression.BaseHP + (int)(level * framework.EnemyProgression.HPScalingCoefficient);
            int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(level * framework.EnemyProgression.DamageScalingCoefficient);

            var combat = SimulateCombatDetailed(playerHP, playerSTR, playerDEF, enemyHP, enemyDMG, level);

            // Granular scoring based on combat quality
            double levelScore = 0;

            // 1. Win rate (0-40 points)
            levelScore += combat.WinRate * 40;

            // 2. Time to kill - target 3-7 turns (0-40 points)
            double ttkScore = 0;
            if (combat.AvgTurns >= 3 && combat.AvgTurns <= 7)
                ttkScore = 40; // Perfect range
            else if (combat.AvgTurns < 3)
                ttkScore = Math.Max(0, 40 - (3 - combat.AvgTurns) * 20); // Too fast = boring
            else if (combat.AvgTurns > 7)
                ttkScore = Math.Max(0, 40 - (combat.AvgTurns - 7) * 5); // Too slow = grindy
            levelScore += ttkScore;

            // 3. Consistency (0-20 points) - low variance is good
            double varianceScore = Math.Max(0, 20 - combat.TurnVariance * 2);
            levelScore += varianceScore;

            scores.Add(levelScore);

            if (level % 3 == 1) // Sample details
            {
                result.Details.Add($"L{level}: {combat.WinRate:P0} wins, {combat.AvgTurns:F1} turns±{combat.TurnVariance:F1} ({levelScore:F0}/100)");
            }

            // Check for critical failures
            if (combat.WinRate < 0.3)
                result.Warnings.Add($"Level {level}: Only {combat.WinRate:P0} win rate - too hard!");
            if (combat.AvgTurns < 2)
                result.Warnings.Add($"Level {level}: {combat.AvgTurns:F1} turns - combat too fast/boring!");
            if (combat.AvgTurns > 15)
                result.Warnings.Add($"Level {level}: {combat.AvgTurns:F1} turns - combat too grindy!");
        }

        result.Score = scores.Average();
        result.WeightedScore = result.Score * Weight;
        return result;
    }

    private CombatStats SimulateCombatDetailed(int playerHP, int playerSTR, int playerDEF, int enemyHP, int enemyDMG, int level)
    {
        const int SIMULATIONS = 20; // More sims for better statistics
        int wins = 0;
        var turnCounts = new List<int>();

        for (int sim = 0; sim < SIMULATIONS; sim++)
        {
            int hp = playerHP;
            int ehp = enemyHP;
            int turns = 0;

            while (ehp > 0 && hp > 0 && turns < 50) // Prevent infinite loops
            {
                turns++;
                ehp -= playerSTR;
                if (ehp > 0)
                    hp -= Math.Max(1, enemyDMG - playerDEF);
            }

            if (hp > 0)
            {
                wins++;
                turnCounts.Add(turns);
            }
        }

        double avgTurns = turnCounts.Count > 0 ? turnCounts.Average() : 50;
        double variance = turnCounts.Count > 1 ? Math.Sqrt(turnCounts.Select(t => Math.Pow(t - avgTurns, 2)).Average()) : 0;

        return new CombatStats
        {
            WinRate = wins / (double)SIMULATIONS,
            AvgTurns = avgTurns,
            TurnVariance = variance
        };
    }

    private class CombatStats
    {
        public double WinRate { get; set; }
        public double AvgTurns { get; set; }
        public double TurnVariance { get; set; }
    }
}

public class EconomicHealthMetric : IFitnessMetric
{
    public string Name => "Economic Health";
    public double Weight => 0.20;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name, Critical = true };

        // Hard failures
        if (framework.Economy.GoldScalingCoefficient <= 0.1 || framework.Economy.BaseGoldPerCombat <= 0)
        {
            result.Score = 0;
            result.Warnings.Add("CRITICAL: Invalid economy parameters");
            result.WeightedScore = 0;
            return result;
        }

        var scores = new List<double>();
        int affordableCount = 0;
        double totalSurplus = 0;

        foreach (var snapshot in framework.Economy.LevelSnapshots)
        {
            double levelScore = 0;

            // 1. Can afford recommended tier? (0-50 points)
            if (snapshot.EconomyHealthy && snapshot.AffordableEquipmentTier >= snapshot.RecommendedEquipmentTier)
            {
                levelScore += 50;
                affordableCount++;
            }
            else
            {
                result.Warnings.Add($"Level {snapshot.Level}: Cannot afford tier {snapshot.RecommendedEquipmentTier}");
            }

            // 2. Economic surplus - how much extra gold? (0-50 points)
            var recommendedTier = framework.Equipment.WeaponTiers.FirstOrDefault(t => t.Tier == snapshot.RecommendedEquipmentTier);
            if (recommendedTier != null)
            {
                double surplus = snapshot.CumulativeGold - recommendedTier.RecommendedCost;
                double surplusRatio = surplus / Math.Max(1, recommendedTier.RecommendedCost);

                // Ideal: 20-50% surplus (not too much, not too little)
                if (surplusRatio >= 0.2 && surplusRatio <= 0.5)
                    levelScore += 50;
                else if (surplusRatio > 0.5)
                    levelScore += Math.Max(0, 50 - (surplusRatio - 0.5) * 50); // Too much = inflation
                else if (surplusRatio >= 0)
                    levelScore += surplusRatio / 0.2 * 50; // Gradient from 0-20%

                totalSurplus += surplusRatio;
            }

            scores.Add(levelScore);
        }

        result.Score = scores.Count > 0 ? scores.Average() : 0;
        result.WeightedScore = result.Score * Weight;

        double affordableRate = affordableCount / (double)Math.Max(1, framework.Economy.LevelSnapshots.Count);
        result.Details.Add($"{affordableRate:P0} of levels can afford progression");
        result.Details.Add($"Avg economic surplus: {(totalSurplus / scores.Count):P0}");

        return result;
    }
}

public class EquipmentCurveMetric : IFitnessMetric
{
    public string Name => "Equipment Progression";
    public double Weight => 0.15;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };
        var scores = new List<double>();

        // Validate weapon tiers
        scores.Add(EvaluateEquipmentTiers(framework.Equipment.WeaponTiers, "Weapon", result));

        // Validate armor tiers
        scores.Add(EvaluateEquipmentTiers(framework.Equipment.ArmorTiers, "Armor", result));

        result.Score = scores.Average();
        result.WeightedScore = result.Score * Weight;
        return result;
    }

    private double EvaluateEquipmentTiers(List<EquipmentTier> tiers, string type, MetricResult result)
    {
        if (tiers.Count < 3)
        {
            result.Warnings.Add($"{type}: Too few tiers ({tiers.Count})");
            return 0;
        }

        var tierScores = new List<double>();

        for (int i = 1; i < tiers.Count; i++)
        {
            var prev = tiers[i - 1];
            var curr = tiers[i];

            double tierScore = 100;

            // 1. Power increase validation (should be 15-50% per tier for meaningful upgrades)
            double powerIncrease = (curr.BonusValue - prev.BonusValue) / (double)Math.Max(1, prev.BonusValue);

            if (powerIncrease >= 0.15 && powerIncrease <= 0.5)
                tierScore = 100; // Perfect
            else if (powerIncrease < 0.15)
            {
                tierScore = Math.Max(0, 100 - (0.15 - powerIncrease) * 200); // Too small = boring
                if (i == 1) result.Details.Add($"{type} T{i}: Only {powerIncrease:P0} upgrade - feels weak");
            }
            else if (powerIncrease > 0.5)
            {
                tierScore = Math.Max(0, 100 - (powerIncrease - 0.5) * 100); // Too large = P2W feel
                result.Details.Add($"{type} T{i}: {powerIncrease:P0} upgrade - too powerful!");
            }

            // 2. Cost scaling (should grow but not exponentially)
            if (i > 0)
            {
                double costRatio = curr.RecommendedCost / (double)Math.Max(1, prev.RecommendedCost);
                if (costRatio < 1.5 || costRatio > 5.0)
                {
                    tierScore *= 0.8; // Penalty for bad cost scaling
                    result.Warnings.Add($"{type} T{i}: Cost ratio {costRatio:F1}x is extreme");
                }
            }

            tierScores.Add(tierScore);
        }

        return tierScores.Average();
    }
}

public class DifficultyPacingMetric : IFitnessMetric
{
    public string Name => "Difficulty Pacing";
    public double Weight => 0.10;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };

        // Measure difficulty curve smoothness across levels
        var difficulties = new List<double>();

        for (int level = 1; level <= 10; level++)
        {
            int playerHP = framework.PlayerProgression.BaseHP + (int)(level * framework.PlayerProgression.HPPerLevel);
            int playerPower = framework.PlayerProgression.BaseSTR + level;

            int enemyHP = framework.EnemyProgression.BaseHP + (int)(level * framework.EnemyProgression.HPScalingCoefficient);
            int enemyPower = framework.EnemyProgression.BaseDamage + (int)(level * framework.EnemyProgression.DamageScalingCoefficient);

            // Difficulty = enemy threat / player survivability
            double difficulty = (enemyHP * enemyPower) / (double)(playerHP * playerPower);
            difficulties.Add(difficulty);
        }

        // 1. Smoothness - check for spikes (0-60 points)
        double maxSpike = 0;
        for (int i = 1; i < difficulties.Count; i++)
        {
            double change = Math.Abs(difficulties[i] - difficulties[i - 1]) / difficulties[i - 1];
            maxSpike = Math.Max(maxSpike, change);
        }

        double smoothnessScore = Math.Max(0, 60 - maxSpike * 200); // Penalize spikes > 30%

        if (maxSpike > 0.3)
            result.Warnings.Add($"Difficulty spike detected: {maxSpike:P0} jump between levels");

        // 2. Trend - should gradually increase (0-40 points)
        double avgIncrease = 0;
        int increases = 0;
        for (int i = 1; i < difficulties.Count; i++)
        {
            if (difficulties[i] > difficulties[i - 1])
                increases++;
            avgIncrease += difficulties[i] - difficulties[i - 1];
        }

        double trendScore = (increases / (double)(difficulties.Count - 1)) * 40;

        result.Score = smoothnessScore + trendScore;
        result.WeightedScore = result.Score * Weight;
        result.Details.Add($"Difficulty curve: {increases}/{difficulties.Count - 1} levels increase");
        result.Details.Add($"Max spike: {maxSpike:P0}");

        return result;
    }
}

public class SkillBalanceMetric : IFitnessMetric
{
    public string Name => "Skill Balance";
    public double Weight => 0.15;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };
        var scores = new List<double>();

        // Test skill usage across levels 1-10
        for (int level = 1; level <= 10; level++)
        {
            int playerHP = framework.PlayerProgression.BaseHP + (int)(level * framework.PlayerProgression.HPPerLevel);
            int playerSTR = framework.PlayerProgression.BaseSTR + level;
            int playerDEF = framework.PlayerProgression.BaseDEF + level;

            int enemyHP = framework.EnemyProgression.BaseHP + (int)(level * framework.EnemyProgression.HPScalingCoefficient);
            int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(level * framework.EnemyProgression.DamageScalingCoefficient);

            double levelScore = 0;

            // Test 1: Power Strike viability (should help but not trivialize)
            double powerStrikeBenefit = TestSkillBenefit(playerHP, playerSTR, playerDEF, enemyHP, enemyDMG, "PowerStrike");
            if (powerStrikeBenefit > 0.1 && powerStrikeBenefit < 0.4) // 10-40% improvement is good
                levelScore += 25;
            else if (powerStrikeBenefit > 0.4)
            {
                result.Warnings.Add($"L{level}: Power Strike too powerful ({powerStrikeBenefit:P0} advantage)");
                levelScore += 10;
            }
            else
                levelScore += 5;

            // Test 2: Shield Bash stun effectiveness (with resistance)
            double stunBenefit = TestStunResistance(playerHP, playerSTR, playerDEF, enemyHP, enemyDMG);
            if (stunBenefit > 0.05 && stunBenefit < 0.30) // 5-30% improvement from stuns
                levelScore += 25;
            else if (stunBenefit > 0.30)
            {
                result.Warnings.Add($"L{level}: Stun-lock possible ({stunBenefit:P0} advantage)");
                levelScore += 10;
            }
            else
                levelScore += 15;

            // Test 3: Berserker Rage risk/reward (should be risky but rewarding)
            double rageBenefit = TestBerserkerRage(playerHP, playerSTR, playerDEF, enemyHP, enemyDMG);
            if (rageBenefit > -0.1 && rageBenefit < 0.3) // Slightly positive to moderate gain
                levelScore += 25;
            else if (rageBenefit < -0.1)
            {
                result.Details.Add($"L{level}: Berserker Rage too risky ({rageBenefit:P0})");
                levelScore += 5;
            }
            else
                levelScore += 15;

            // Test 4: Skills don't break stamina economy
            double skillStaminaBalance = TestStaminaEconomy(playerSTR, enemyHP);
            if (skillStaminaBalance > 0.7) // Player has enough stamina for skills
                levelScore += 25;
            else
            {
                result.Warnings.Add($"L{level}: Skill stamina too limited");
                levelScore += 10;
            }

            scores.Add(levelScore);
        }

        result.Score = scores.Average();
        result.WeightedScore = result.Score * Weight;
        result.Details.Add($"Average skill balance: {result.Score:F0}/100");

        return result;
    }

    private double TestSkillBenefit(int hp, int str, int def, int enemyHP, int enemyDMG, string skill)
    {
        // Simulate combat with and without Power Strike
        double normalWinRate = SimulateCombat(hp, str, def, enemyHP, enemyDMG, false);
        double skillWinRate = SimulateCombat(hp, str, def, enemyHP, enemyDMG, true);
        return skillWinRate - normalWinRate;
    }

    private double TestStunResistance(int hp, int str, int def, int enemyHP, int enemyDMG)
    {
        // Simulate repeated stun attempts (resistance builds up)
        int wins = 0;
        const int sims = 20;

        for (int i = 0; i < sims; i++)
        {
            int playerHP = hp;
            int eHP = enemyHP;
            int stunResistance = 0;
            int stamina = 12;

            while (playerHP > 0 && eHP > 0)
            {
                // Try Shield Bash if we have stamina
                if (stamina >= 4 && new Random().Next(100) < (100 - stunResistance * 25))
                {
                    stamina -= 4;
                    stunResistance++;
                    eHP -= (int)(str * 0.7); // Bash damage
                    // Enemy stunned, skips turn
                }
                else
                {
                    // Normal attack
                    stamina = Math.Max(0, stamina - 3);
                    eHP -= str;
                    playerHP -= Math.Max(1, enemyDMG - def);
                }

                if (stamina < 3) stamina = 12; // Regenerate if depleted
            }

            if (playerHP > 0) wins++;
        }

        double stunWinRate = wins / (double)sims;
        double normalWinRate = SimulateCombat(hp, str, def, enemyHP, enemyDMG, false);
        return stunWinRate - normalWinRate;
    }

    private double TestBerserkerRage(int hp, int str, int def, int enemyHP, int enemyDMG)
    {
        // Simulate combat with Berserker Rage (2x damage, 1.5x damage taken)
        int wins = 0;
        const int sims = 20;

        for (int i = 0; i < sims; i++)
        {
            int playerHP = hp;
            int eHP = enemyHP;
            int rageTurns = 3; // Rage lasts 3 turns

            while (playerHP > 0 && eHP > 0)
            {
                if (rageTurns > 0)
                {
                    // Rage active
                    eHP -= str * 2;
                    playerHP -= (int)(Math.Max(1, enemyDMG - def) * 1.5);
                    rageTurns--;
                }
                else
                {
                    // Normal combat
                    eHP -= str;
                    playerHP -= Math.Max(1, enemyDMG - def);
                }
            }

            if (playerHP > 0) wins++;
        }

        double rageWinRate = wins / (double)sims;
        double normalWinRate = SimulateCombat(hp, str, def, enemyHP, enemyDMG, false);
        return rageWinRate - normalWinRate;
    }

    private double TestStaminaEconomy(int str, int enemyHP)
    {
        // Can player afford to use skills? (Power Strike costs 5 stamina)
        int turnsToKill = (int)Math.Ceiling(enemyHP / (double)(str * 1.5)); // With Power Strike
        int staminaNeeded = turnsToKill * 5;
        int staminaAvailable = 12; // Starting stamina

        return Math.Min(1.0, staminaAvailable / (double)staminaNeeded);
    }

    private double SimulateCombat(int hp, int str, int def, int enemyHP, int enemyDMG, bool useSkills)
    {
        int wins = 0;
        const int sims = 10;

        for (int i = 0; i < sims; i++)
        {
            int playerHP = hp;
            int eHP = enemyHP;

            while (playerHP > 0 && eHP > 0)
            {
                int damage = useSkills ? (int)(str * 1.5) : str; // Power Strike
                eHP -= damage;
                if (eHP > 0)
                    playerHP -= Math.Max(1, enemyDMG - def);
            }

            if (playerHP > 0) wins++;
        }

        return wins / (double)sims;
    }
}

public class BuildDiversityMetric : IFitnessMetric
{
    public string Name => "Build Diversity";
    public double Weight => 0.15;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };

        // Count viable builds (score > 60)
        int viableCount = framework.Builds.ViableBuilds.Count(b => b.Value.ViabilityScore > 60);

        // 1. Viability count (0-50 points)
        double viabilityScore = (viableCount / 3.0) * 50;

        // 2. Differentiation (0-50 points) - builds should have DIFFERENT scores, not all 100
        var scores = framework.Builds.ViableBuilds.Select(b => b.Value.ViabilityScore).ToList();
        double variance = scores.Count > 1 ? Math.Sqrt(scores.Select(s => Math.Pow(s - scores.Average(), 2)).Average()) : 0;

        // Perfect variance: 10-20 points (builds viable but different)
        double differentiationScore = 0;
        if (variance >= 10 && variance <= 20)
            differentiationScore = 50;
        else if (variance < 10)
            differentiationScore = variance / 10.0 * 50; // Too similar
        else
            differentiationScore = Math.Max(0, 50 - (variance - 20) * 2); // Too different = some unviable

        result.Score = viabilityScore + differentiationScore;
        result.WeightedScore = result.Score * Weight;

        result.Details.Add($"{viableCount}/3 builds viable (>{60}% score)");
        result.Details.Add($"Build differentiation: {variance:F1} variance");

        foreach (var build in framework.Builds.ViableBuilds)
        {
            if (build.Value.ViabilityScore < 60)
                result.Warnings.Add($"{build.Key}: Only {build.Value.ViabilityScore:F0}% viable");
        }

        return result;
    }
}

public static class FitnessEvaluator
{
    private static readonly List<IFitnessMetric> _metrics = new()
    {
        new CombatBalanceMetric(),      // 25% (was 30%)
        new EconomicHealthMetric(),     // 20% (was 25%)
        new EquipmentCurveMetric(),     // 15%
        new DifficultyPacingMetric(),   // 10% (was 15%)
        new SkillBalanceMetric(),       // 15% (NEW - Gen 35)
        new BuildDiversityMetric()      // 15%
        // Total: 100% - add new metrics here as features are added!
    };

    public static (double totalFitness, List<MetricResult> results) EvaluateComprehensive(ProgressionFrameworkData framework)
    {
        var results = new List<MetricResult>();
        double totalWeightedScore = 0;

        foreach (var metric in _metrics)
        {
            var result = metric.Evaluate(framework);
            results.Add(result);
            totalWeightedScore += result.WeightedScore;

            // Critical failures
            if (result.Critical && result.Score < 50)
            {
                return (0, results); // Instant failure
            }
        }

        // Sum of weighted scores already represents 0-100 scale
        // (each metric: score 0-100 × weight, all weights sum to 1.0)
        double totalFitness = totalWeightedScore;

        return (totalFitness, results);
    }

    public static string GetDetailedReport(List<MetricResult> results)
    {
        var report = new System.Text.StringBuilder();
        report.AppendLine("\n╔═══════════════════════════════════════════════════════════════╗");
        report.AppendLine("║                  FITNESS METRIC BREAKDOWN                     ║");
        report.AppendLine("╚═══════════════════════════════════════════════════════════════╝\n");

        foreach (var result in results)
        {
            report.AppendLine($"📊 {result.MetricName}: {result.Score:F1}/100 (weighted: {result.WeightedScore:F1})");

            foreach (var detail in result.Details)
                report.AppendLine($"   ✓ {detail}");

            foreach (var warning in result.Warnings)
                report.AppendLine($"   ⚠️  {warning}");

            report.AppendLine();
        }

        return report.ToString();
    }
}


