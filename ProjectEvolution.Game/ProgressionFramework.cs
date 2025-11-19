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
    public CombatMechanics Combat { get; set; } = new(); // NEW
    public SkillSystem Skills { get; set; } = new(); // NEW
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

// NEW: Combat mechanics tuning
public class CombatMechanics
{
    public double BaseCritChance { get; set; } // Base % chance (0-20)
    public double CritChancePerLevel { get; set; } // Per level increase
    public double CritDamageMultiplier { get; set; } // 1.5 = 150% damage
    public double BaseDodgeChance { get; set; } // Base % (0-15)
    public double DodgePerDEF { get; set; } // % per DEF point
    public double BaseBlockChance { get; set; } // Base % with shield (0-25)
    public double BlockPerDEF { get; set; } // % per DEF point
    public double BlockDamageReduction { get; set; } // % damage reduced on block
}

// NEW: Skill system tuning
public class SkillSystem
{
    public int SkillPointsPerLevel { get; set; } // How many skill points per level
    public double SkillDamageBase { get; set; } // Base skill damage multiplier
    public double SkillDamagePerLevel { get; set; } // Skill scaling per level
    public int SkillManaCost { get; set; } // Base mana cost
    public int BaseMana { get; set; } // Starting mana pool
    public double ManaPerLevel { get; set; } // Mana scaling
    public double SkillCooldown { get; set; } // Turns between skill uses
}

public class ResearchMetadata
{
    public DateTime Timestamp { get; set; }
    public int SimulationsRun { get; set; }
    public int Generation { get; set; }
    public double OverallFitness { get; set; }
    public string Version { get; set; } = "Generation 36 - Combat + Skills";
}

public class ProgressionFrameworkResearcher
{
    // VERSION TRACKING - Increment on each bug fix!
    private const string TUNER_VERSION = "v2.0.0-deterministic-adaptive";
    private static readonly DateTime BUILD_TIME = DateTime.Now;

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

    // Throughput smoothing for stable gen/s display
    private static Queue<double> _recentGenerationTimes = new Queue<double>();
    private static DateTime _lastGenerationTime = DateTime.Now;

    // Fitness trend tracking - see if we're improving or plateauing
    private static Queue<(int generation, double fitness)> _fitnessHistory = new Queue<(int, double)>();
    private const int FITNESS_HISTORY_SIZE = 100; // Track last 100 improvements

    // Champion/Rubric System - tracks best runs across resets
    private static ProgressionFrameworkData? _champion = null;
    private static double _championFitness = 0;
    private static int _championGeneration = 0;
    private static int _resetCount = 0;

    // Track last BEST framework for change highlighting (only update on improvement!)
    private static ProgressionFrameworkData? _previousBest = null;
    private static double _previousBestFitness = 0;
    private const double AUTO_RESET_THRESHOLD = 85.0; // Reset if stuck near optimal
    private const int AUTO_RESET_STUCK_GENS = 150; // Must be stuck this long
    private const double AUTO_RESET_TREND_THRESHOLD = 0.05; // Reset if trend < +0.05/1k gens (plateau)
    private const int AUTO_RESET_MIN_IMPROVEMENTS = 20; // Need this many data points to trust trend

    // ADAPTIVE POPULATION-BASED EVOLUTION (Self-tuning!)
    private static int _populationSize = 10; // Start small, grow as needed
    private const int MIN_POPULATION = 10;
    private const int MAX_POPULATION = 100;
    private static List<(ProgressionFrameworkData framework, double fitness)> _population = new();

    // Track current candidate being evaluated (for live display)
    private static ProgressionFrameworkData? _currentCandidate = null;
    private static double _currentCandidateFitness = 0;

    // Anti-flicker settings
    private const int UI_UPDATE_INTERVAL = 25; // Update UI every N generations (reduce flash)
    private const int CYCLE_DELAY_MS = 0; // NO DELAY - max speed!

    // PARALLEL EVOLUTION: Run multiple candidates simultaneously
    private static int POPULATION_SIZE = 16; // Configurable based on performance mode
    private static readonly object _bestLock = new object(); // Thread-safe best tracking

    // Ultima 4-style AAA UI - Rock-solid, zero flicker, full-screen
    private static UltimaStyleDashboard? _ultimaDashboard = null;

    // TUNING THE TUNER: Progressive difficulty based on champion performance
    public static double GetDifficultyMultiplier()
    {
        // As champion improves, make metrics more demanding
        if (_championFitness >= 90) return 1.5;  // HARD MODE: Champion mastered the game
        if (_championFitness >= 85) return 1.3;  // ADVANCED: Tighten tolerances
        if (_championFitness >= 75) return 1.1;  // INTERMEDIATE: Slightly harder
        return 1.0; // BEGINNER: Standard difficulty
    }

    public static double GetChampionFitness() => _championFitness;

    private static string GetFitnessQualityBand(double fitness)
    {
        // Translate fitness score to gameplay quality
        if (fitness >= 90) return "🏆 OPTIMAL - Near-perfect balance! Ready for production!";
        if (fitness >= 80) return "⭐ EXCELLENT - Very good balance, minor tweaks only";
        if (fitness >= 70) return "✅ GOOD - Playable and fun, some rough edges";
        if (fitness >= 60) return "📊 FAIR - Functional but needs work";
        if (fitness >= 50) return "⚠️  POOR - Broken economy or unfun combat";
        return "❌ BROKEN - Unplayable, critical failures";
    }

    private static string GetTimeToTargetEstimate(double currentFitness, double trend, int stuckGens)
    {
        // REALITY CHECK: If stuck for a long time, ETA is wrong!
        if (stuckGens > 200)
            return "STUCK! Auto-reset soon...";

        if (stuckGens > 100)
            return $"Plateauing ({stuckGens} stuck)";

        // Estimate how long to reach next quality tier
        if (trend <= 0.01) return "Trend too low (<0.01/1k)";
        if (trend < 0.05) return $"Slow ({trend:F2}/1k - may plateau)";

        double[] targets = { 70, 75, 80, 85, 90 };
        double nextTarget = targets.FirstOrDefault(t => t > currentFitness);

        if (nextTarget == 0) return "At maximum!";

        double fitnessNeeded = nextTarget - currentFitness;
        double gensNeeded = fitnessNeeded / (trend / 1000.0);

        if (gensNeeded < 0 || gensNeeded > 1000000) return "Unknown (bad trend)";

        double minutesNeeded = gensNeeded / 600.0; // Assume 600 gen/s average

        // Sanity check: If ETA > 2 hours, probably plateauing
        if (minutesNeeded > 120)
            return $">{minutesNeeded / 60:F0}hrs (likely plateau)";

        if (minutesNeeded < 5) return $"~{minutesNeeded:F0}min to {nextTarget:F0}";
        if (minutesNeeded < 60) return $"~{minutesNeeded:F0}min to {nextTarget:F0}";
        return $"~{minutesNeeded / 60:F1}hrs to {nextTarget:F0}";
    }

    private static double CalculateFitnessTrend()
    {
        // Calculate slope of fitness improvements over last N generations
        if (_fitnessHistory.Count < 10) return 10.0; // Not enough data, assume improving

        var recent = _fitnessHistory.ToArray();
        var oldest = recent[0];
        var newest = recent[recent.Length - 1];

        double genDelta = newest.generation - oldest.generation;
        double fitnessDelta = newest.fitness - oldest.fitness;

        return genDelta > 0 ? fitnessDelta / genDelta * 1000 : 0; // fitness gain per 1000 gens
    }

    public static void RunContinuousResearchHeadless()
    {
        // Simple wrapper that sets Console.IsInputRedirected simulation
        RunContinuousResearch();
    }

    public static void RunContinuousResearch()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║      🧬 CONTINUOUS PROGRESSION FRAMEWORK RESEARCH 🧬           ║");
        Console.WriteLine("║    Discovers formulas → Generates code → Tests → Refines       ║");
        Console.WriteLine($"║     {TUNER_VERSION,-58} ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        // Configure output path
        // Docker mode: auto-use /data volume
        if (Console.IsInputRedirected || Directory.Exists("/data"))
        {
            string dockerPath = "/data";
            SafeFileWriter.SetOutputPath(dockerPath);
            Console.WriteLine($"🐳 Docker mode: Using volume path: {dockerPath}");
        }
        else
        {
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
        }

        Console.WriteLine("\nTesting write access...");
        if (!SafeFileWriter.TestOutputPath())
        {
            Console.WriteLine("❌ Cannot write to output path! Press any key to return...");
            Console.ReadKey();
            return;
        }
        Console.WriteLine("✅ Write access confirmed!\n");

        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("🚀 OPTIMIZED EVOLUTION MODE");
        Console.ResetColor();
        Console.WriteLine("   Demoscene optimizations: Span, stackalloc, zero GC pressure");
        Console.WriteLine("   Serial execution: No parallel overhead");
        Console.WriteLine($"   CPU Cores: {Environment.ProcessorCount} available");
        Console.WriteLine("   Target: 250-400+ gen/s\n");

        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("\n📚 WHAT YOU'RE ABOUT TO SEE:");
        Console.ResetColor();
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━");
        Console.WriteLine("FITNESS SCORE = Overall game balance quality (0-100)");
        Console.WriteLine("");
        Console.WriteLine("  50-60: ⚠️  POOR - Broken economy or unfun combat (1-2 hours)");
        Console.WriteLine("  60-70: 📊 FAIR - Functional but needs work (2-4 hours)");
        Console.WriteLine("  70-80: ✅ GOOD - Playable and fun! (4-8 hours)");
        Console.WriteLine("  80-90: ⭐ EXCELLENT - Production ready! (8-24 hours)");
        Console.WriteLine("  90+:   🏆 OPTIMAL - Near perfect! (days/weeks)");
        Console.WriteLine("");
        Console.WriteLine("TREND = How fast it's improving (+X.XX per 1000 generations)");
        Console.WriteLine("  +2.0/1k = FAST climb (early run, expect hours)");
        Console.WriteLine("  +0.5/1k = STEADY progress (mid run, overnight)");
        Console.WriteLine("  +0.1/1k = SLOW gains (near optimal, days)");
        Console.WriteLine("  +0.05/1k = PLATEAU (auto-resets to explore new space)");
        Console.WriteLine("");
        Console.WriteLine("SPARKLINE = Visual trend of last 20 improvements");
        Console.WriteLine("  ▁▂▃▄▅▆▇█ = Climbing (keep going!)");
        Console.WriteLine("  ████████ = Flat (plateau, will auto-reset)");
        Console.WriteLine("");
        Console.WriteLine("AUTO-RESET = Saves best as Champion, starts fresh exploration");
        Console.WriteLine("  Triggers when stuck at plateau to escape local optimum");
        Console.WriteLine("  Your Champion is PRESERVED as the best ever found!");
        Console.WriteLine("━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━\n");

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

        // VERSION INFO - Always visible
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"\n📋 TUNER VERSION: {TUNER_VERSION}");
        Console.WriteLine($"   Build time: {BUILD_TIME:yyyy-MM-dd HH:mm:ss}");
        Console.ResetColor();
        var versionLog = $"\n[{DateTime.Now:HH:mm:ss}] ═══ TUNER START ═══\n  Version: {TUNER_VERSION}\n  Build: {BUILD_TIME:yyyy-MM-dd HH:mm:ss}\n";
        SafeFileWriter.SafeAppendAllText("progression_research.log", versionLog);

        if (_champion != null)
        {
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.WriteLine($"\n🏆 Champion loaded: Gen {_championGeneration}, Fitness {_championFitness:F2}");
            Console.WriteLine("   This is your rubric - new runs will be compared against it!");
            Console.ResetColor();
        }

        // Docker mode: auto-start
        if (!Console.IsInputRedirected)
        {
            Console.WriteLine("\nPress ESC to stop, any other key to start...");
            if (Console.ReadKey().Key == ConsoleKey.Escape) return;
        }
        else
        {
            Console.WriteLine("\n🐳 Docker mode: Auto-starting research...");
        }

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

        // Initialize Ultima-style dashboard
        _ultimaDashboard = new UltimaStyleDashboard();
        _ultimaDashboard.Initialize();

        // Initial evaluation to populate metrics before first render
        if (_bestFramework != null)
        {
            _bestFitness = EvaluateFramework(_bestFramework);
        }

        // Initial render with new UI
        if (_bestFramework != null && _ultimaDashboard != null)
        {
            RenderUltimaDashboard();
        }

        while (true)
        {
            _generation++;

            // Track generation throughput (gen/s)
            var now = DateTime.Now;
            var elapsed = (now - _lastGenerationTime).TotalSeconds;
            if (elapsed > 0)
            {
                double currentGenPerSec = 1.0 / elapsed;
                _recentGenerationTimes.Enqueue(currentGenPerSec);
                if (_recentGenerationTimes.Count > 30) // Average last 30 generations
                    _recentGenerationTimes.Dequeue();
            }
            _lastGenerationTime = now;

            // POPULATION-BASED EVOLUTION (better for multi-objective optimization)
            ProgressionFrameworkData framework;
            double fitness;

            // Initialize population if empty
            if (_population.Count == 0)
            {
                _currentPhase = "🌱 Seeding population...";

                // CRITICAL: Start from CHAMPION if available (the 70.9 solution!)
                if (_champion != null && _championFitness > _bestFitness)
                {
                    var msg = $"📍 Seeding from CHAMPION (fitness {_championFitness:F1})";
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                    _population.Add((_champion, _championFitness));
                    _bestFramework = _champion;
                    _bestFitness = _championFitness;
                }
                else if (_bestFramework != null)
                {
                    var msg = $"📍 Seeding from CURRENT BEST (fitness {_bestFitness:F1})";
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                    _population.Add((_bestFramework, _bestFitness));
                }

                // Fill rest with variations of the best (champion or current)
                ProgressionFrameworkData seed = _champion ?? _bestFramework ?? CreateBaselineFramework();

                // PARALLEL EVALUATION: Utilize all CPU cores
                int needed = _populationSize - _population.Count;
                var newCandidates = new System.Collections.Concurrent.ConcurrentBag<(ProgressionFrameworkData, double)>();
                Parallel.For(0, needed, i =>
                {
                    var variant = MutateFramework(seed);
                    var variantFitness = EvaluateFramework(variant);
                    newCandidates.Add((variant, variantFitness));
                    Interlocked.Add(ref _totalSimulations, 65);
                });

                foreach (var candidate in newCandidates)
                {
                    _population.Add(candidate);
                }

                // Sort by fitness
                _population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            }

            // ADAPTIVE POPULATION SIZING - Let it self-tune!
            if (_generation % 500 == 0 && _population.Count > 2)
            {
                // Measure diversity: fitness range in population
                var fitnesses = _population.Select(p => p.fitness).ToArray();
                double diversityRange = fitnesses.Max() - fitnesses.Min();

                // Measure throughput
                double avgGenPerSec = _recentGenerationTimes.Count > 0 ? _recentGenerationTimes.Average() : 1000;

                // GROW population aggressively if monoculture detected
                if (diversityRange < 1.0 && _populationSize < MAX_POPULATION && avgGenPerSec > 1000)
                {
                    int oldSize = _populationSize;
                    // Grow faster based on throughput - if fast, grow bigger jumps!
                    int growthStep = avgGenPerSec > 15000 ? 50 : // 20k gen/s? Jump by 50!
                                    avgGenPerSec > 10000 ? 30 : // 10k gen/s? Jump by 30
                                    avgGenPerSec > 5000 ? 20 :  // 5k gen/s? Jump by 20
                                    10;                         // Otherwise jump by 10

                    _populationSize = Math.Min(MAX_POPULATION, _populationSize + growthStep);
                    var msg = $"🔬 MONOCULTURE! Growing pop {oldSize}→{_populationSize} (diversity={diversityRange:F2}, speed={avgGenPerSec:F0}gen/s)";
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                }
                // SHRINK population if throughput drops below 3000 gen/s
                else if (_populationSize > MIN_POPULATION && avgGenPerSec < 3000)
                {
                    int oldSize = _populationSize;
                    _populationSize = Math.Max(MIN_POPULATION, _populationSize - 20);
                    // Trim population to new size
                    while (_population.Count > _populationSize)
                        _population.RemoveAt(_population.Count - 1);
                    var msg = $"⚡ Throughput dropped ({avgGenPerSec:F0}gen/s). Shrinking pop {oldSize}→{_populationSize}";
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                }
                // GROW slightly if diversity is low-medium and we have speed budget
                else if (diversityRange < 2.0 && _populationSize < MAX_POPULATION && avgGenPerSec > 10000)
                {
                    int oldSize = _populationSize;
                    _populationSize = Math.Min(MAX_POPULATION, _populationSize + 10);
                    var msg = $"📈 Low diversity ({diversityRange:F2}), growing pop {oldSize}→{_populationSize}";
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"[{DateTime.Now:HH:mm:ss}] {msg}\n");
                }
            }

            // CHAMPION-GUIDED EXPLORATION when severely stuck
            if (_generationsSinceImprovement > 3000)
            {
                // ULTRA STUCK: Every 500 gens, fully reseed population from champion
                if (_generation % 500 == 0)
                {
                    _currentPhase = "🏆 ULTRA - Reseeding from CHAMPION!";
                    _population.Clear();
                    if (_champion != null)
                    {
                        // Refill ENTIRE population with champion variations
                        for (int i = 0; i < _populationSize; i++)
                        {
                            var variant = MutateFramework(_champion);
                            var varFit = EvaluateFramework(variant);
                            _population.Add((variant, varFit));
                            _totalSimulations += 65;
                        }
                        _population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
                    }
                }

                // Explore NEAR champion with big mutations
                if (_champion != null)
                {
                    _currentPhase = "🏆 Champion-guided (2x)";
                    framework = MutateFramework(_champion); // 2x strength
                }
                else
                {
                    framework = MutateFramework(_bestFramework ?? CreateBaselineFramework());
                }

                _currentCandidate = framework;
                fitness = EvaluateFramework(framework);
                _currentCandidateFitness = fitness;
                _totalSimulations += 65;
            }
            else if (_generationsSinceImprovement > 1000)
            {
                // SEVERELY STUCK: 50% champion-guided, 50% crossover
                var random = new Random(_generation * 19);
                if (_champion != null && random.NextDouble() < 0.5)
                {
                    _currentPhase = "🏆 Champion-guided (1.5x mutations)";
                    framework = MutateFramework(_champion); // 1.5x strength
                }
                else
                {
                    // Crossover best candidates + strong mutation
                    _currentPhase = "🧬 Aggressive crossover...";
                    var parent1 = TournamentSelect(_population);
                    var parent2 = TournamentSelect(_population);
                    framework = CrossoverFrameworks(parent1, parent2);
                    framework = MutateFramework(framework); // 1.5x strength
                }
                _currentCandidate = framework;
                fitness = EvaluateFramework(framework);
                _currentCandidateFitness = fitness;
                _totalSimulations += 65;
            }
            // Every 10 gens when stuck >500: inject diversity
            else if (_generationsSinceImprovement > 500 && _generation % 10 == 0)
            {
                _currentPhase = "💥 DIVERSITY INJECTION - Big jumps!";
                framework = CreateRandomFramework();
                _currentCandidate = framework; // Track for live display
                fitness = EvaluateFramework(framework);
                _currentCandidateFitness = fitness;
                _totalSimulations += 65;
            }
            // Every 20 gens when stuck >200: try random jumps
            else if (_generationsSinceImprovement > 200 && _generation % 20 == 0)
            {
                _currentPhase = "🎲 Random exploration...";
                framework = CreateRandomFramework();
                _currentCandidate = framework; // Track for live display
                fitness = EvaluateFramework(framework);
                _currentCandidateFitness = fitness;
                _totalSimulations += 65;
            }
            // Normal evolution: crossover + mutation
            else
            {
                // Every 100 gens: inject champion to prevent population drift
                if (_champion != null && _generation % 100 == 0)
                {
                    _currentPhase = "🏆 Champion injection - maintaining diversity";
                    framework = MutateFramework(_champion); // Small variations on champion
                }
                else
                {
                    _currentPhase = "🧬 Crossover + Mutation...";

                    // Tournament selection: pick 2 random, use better one
                    var parent1 = TournamentSelect(_population);
                    var parent2 = TournamentSelect(_population);

                    // Crossover: combine traits from both parents
                    framework = CrossoverFrameworks(parent1, parent2);

                    // Mutate offspring
                    framework = MutateFramework(framework);
                }

                // Evaluate
                _currentCandidate = framework; // Track for live display
                fitness = EvaluateFramework(framework);
                _currentCandidateFitness = fitness;
                _totalSimulations += 65;
            }

            // PHASE 3: Update population and track best
            bool improved = false;

            // Add to population, remove worst if full
            _population.Add((framework, fitness));
            _population.Sort((a, b) => b.fitness.CompareTo(a.fitness));
            if (_population.Count > _populationSize)
            {
                _population.RemoveAt(_population.Count - 1); // Remove worst
            }

            // Update global best (with SIMULATED ANNEALING for plateau escape)
            bool acceptSolution = false;

            if (fitness > _bestFitness || _bestFramework == null)
            {
                // Always accept improvements
                acceptSolution = true;
                improved = true;
            }
            else if (_generationsSinceImprovement > 1000)
            {
                // SIMULATED ANNEALING: Accept worse solutions when stuck to escape plateau
                // Temperature based on how stuck we are
                double temperature = 1.0 + (_generationsSinceImprovement / 10000.0); // Increases with stuckness
                double fitnessDelta = fitness - _bestFitness; // Negative value
                double acceptanceProbability = Math.Exp(fitnessDelta / temperature);

                var random = new Random(_generation * 31);
                if (random.NextDouble() < acceptanceProbability)
                {
                    acceptSolution = true;
                    SafeFileWriter.SafeAppendAllText("progression_research.log",
                        $"[{DateTime.Now:HH:mm:ss}] 🌡️ ANNEALING: Accepted {fitness:F2} (was {_bestFitness:F2}, prob={acceptanceProbability:F4}, stuck={_generationsSinceImprovement})\n");
                }
            }

            if (acceptSolution)
            {
                _currentPhase = fitness > _bestFitness ? "🌟 NEW BEST! Saving..." : "🌡️ Annealing accepted";

                // Save previous best for delta display
                _previousBest = _bestFramework;
                _previousBestFitness = _bestFitness;

                if (fitness > _bestFitness)
                {
                    _bestFitness = fitness;
                    _generationsSinceImprovement = 0;

                    // Track fitness improvement history
                    _fitnessHistory.Enqueue((_generation, fitness));
                    while (_fitnessHistory.Count > FITNESS_HISTORY_SIZE)
                        _fitnessHistory.Dequeue();
                }

                _bestFramework = framework;

                SaveFrameworkToJSON(framework);
                GenerateGameCode(framework);
                _lastSaveTime = DateTime.Now;

                // Auto-update champion if we beat it!
                if (_bestFitness > _championFitness)
                {
                    double oldChampFitness = _championFitness;
                    SaveChampion(); // This updates _championFitness inside
                    _currentPhase = $"🏆 NEW CHAMPION! {_bestFitness:F1} (beat {oldChampFitness:F1})";
                }

                // Render will happen below at normal interval
            }
            else
            {
                _generationsSinceImprovement++;
            }

            // PHASE 4: Auto-save every 50 generations (was 5 - reduced I/O for speed!)
            if (_generation % 50 == 0)
            {
                _currentPhase = "💾 Auto-save...";
                if (_bestFramework != null)
                {
                    SaveFrameworkToJSON(_bestFramework);
                }
                _lastSaveTime = DateTime.Now;
            }

            // PHASE 5: Update UI (uses double-buffered Ultima dashboard)
            _currentPhase = improved ? "✅ Improved!" : "🔄 Searching...";

            // Update web state (for API integration)
            var webElapsed = DateTime.Now - _startTime;
            double webGenPerSec = _recentGenerationTimes.Count > 0 ? _recentGenerationTimes.Average() : 0;
            TunerWebState.Update(new TunerStateData
            {
                Generation = _generation,
                BestFitness = _bestFitness,
                AvgFitness = _population.Count > 0 ? _population.Average(p => p.fitness) : _bestFitness,
                GenPerSec = webGenPerSec,
                StuckGens = _generationsSinceImprovement,
                Phase = _currentPhase,
                PopulationSize = _population.Count,
                ChampionFitness = _championFitness,
                ChampionGen = _championGeneration,
                Resets = _resetCount,
                Device = "CPU",
                Elapsed = webElapsed
            });

            // Update UI at controlled intervals only
            bool shouldRenderNow = (_generation % UI_UPDATE_INTERVAL == 0) || improved;

            if (shouldRenderNow)
            {
                RenderUltimaDashboard();
            }

            // Log progress (only on improvement or every 50 gens to reduce I/O!)
            if (improved || _generation % 50 == 0)
            {
                LogProgress(framework, fitness, improved);
            }

            // Diagnostic: Log population diversity every 500 gens
            if (_generation % 500 == 0 && _population.Count > 0)
            {
                var popFitnesses = _population.Select(p => p.fitness).ToArray();
                double avgFit = popFitnesses.Average();
                double minFit = popFitnesses.Min();
                double maxFit = popFitnesses.Max();
                var diagnostic = $"[{DateTime.Now:HH:mm:ss}] Gen {_generation} DIVERSITY: Pop={_population.Count}, Fit range={minFit:F1}-{maxFit:F1} (avg={avgFit:F1}), Stuck={_generationsSinceImprovement}\n";
                SafeFileWriter.SafeAppendAllText("progression_research.log", diagnostic);

                // Check for monoculture (all candidates within 0.5 fitness)
                if (maxFit - minFit < 0.5)
                {
                    SafeFileWriter.SafeAppendAllText("progression_research.log", $"  ⚠️  MONOCULTURE DETECTED! Fitness range only {maxFit - minFit:F2}\n");
                }
            }

            // AUTO-RESET: If stuck too long, perform automatic reset
            // Increased from 10k to 100k to allow proper exploration before resetting
            if (_generationsSinceImprovement > 100000)
            {
                _currentPhase = "🔄 AUTO-RESET - Stuck >100k gens, promoting and restarting";
                PerformReset(manual: false);
                continue; // Skip rest of loop, render will happen next iteration
            }

            // Check for ESC or R (manual reset) - Skip in Docker mode
            if (!Console.IsInputRedirected && Console.KeyAvailable)
            {
                var key = Console.ReadKey(true).Key;
                if (key == ConsoleKey.Escape)
                {
                    _currentPhase = "💾 Saving...";
                    RenderUltimaDashboard();
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

            // AUTOMATED RESET: Multiple triggers to force continuous improvement!
            bool shouldAutoReset = false;
            string resetReason = "";

            // Trigger 1: Stuck at high fitness
            if (_bestFitness >= AUTO_RESET_THRESHOLD && _generationsSinceImprovement >= AUTO_RESET_STUCK_GENS)
            {
                shouldAutoReset = true;
                resetReason = $"Stuck at {_bestFitness:F1} for {_generationsSinceImprovement} gens";
            }

            // Trigger 2: Plateau detected (low improvement slope)
            // Increased to 50k to allow MUCH longer exploration before giving up
            double trend = CalculateFitnessTrend();
            if (_fitnessHistory.Count >= AUTO_RESET_MIN_IMPROVEMENTS &&
                trend < AUTO_RESET_TREND_THRESHOLD &&
                _generationsSinceImprovement >= 50000)
            {
                shouldAutoReset = true;
                resetReason = $"Plateau detected (trend: +{trend:F2}/1k, stuck: {_generationsSinceImprovement})";
            }

            if (shouldAutoReset)
            {
                _currentPhase = $"🔄 AUTO-RESET: {resetReason}";
                RenderUltimaDashboard();
                Thread.Sleep(1000); // Show message
                PerformReset(manual: false);
            }

            // NO DELAY - max speed! UI updates every 10 gens to prevent flicker
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

                    // Validate champion fitness is in reasonable range (0-100)
                    if (_championFitness > 100 || _championFitness < 0)
                    {
                        Console.WriteLine($"⚠️  WARNING: Champion data corrupted (fitness={_championFitness:F1})");
                        Console.WriteLine("   Clearing champion and starting fresh...");
                        _champion = null;
                        _championFitness = 0;
                        _championGeneration = 0;
                        File.Delete(championPath); // Delete corrupted file
                        Thread.Sleep(2000);
                    }
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
        _currentPhase = manual ? "🔄 MANUAL RESET - Saving champion..." : "🔄 AUTO RESET - Plateau detected, exploring new space...";
        RenderUltimaDashboard();
        Thread.Sleep(1000);

        // Save current best as champion
        if (_bestFramework != null && _bestFitness > _championFitness)
        {
            SaveChampion();
            _currentPhase = $"🏆 NEW CHAMPION! Fitness: {_bestFitness:F2} (was {_championFitness:F2})";
            RenderUltimaDashboard();
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

        // Clear population to force reseeding from champion!
        _population.Clear();

        // Reset to champion if available, not baseline!
        if (_champion != null)
        {
            _bestFramework = _champion;
            _bestFitness = _championFitness;
            var msg = $"🏆 Reset: Starting from CHAMPION (fitness {_championFitness:F1}, BaseDEF={_champion.PlayerProgression.BaseDEF})";
            log.AppendLine($"  {msg}");
        }
        else
        {
            _bestFramework = CreateBaselineFramework();
            _bestFitness = EvaluateFramework(_bestFramework);
            var msg = $"📍 Reset: Starting from baseline (fitness {_bestFitness:F1})";
            log.AppendLine($"  {msg}");
        }

        _generation = 0;
        _generationsSinceImprovement = 0;
        _previousBest = null;
        _previousBestFitness = 0;

        // Don't render here - will render on next iteration
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

        // NEW: Combat mechanics baseline
        framework.Combat.BaseCritChance = 5.0; // 5% base crit
        framework.Combat.CritChancePerLevel = 0.5; // +0.5% per level
        framework.Combat.CritDamageMultiplier = 1.5; // 150% damage on crit
        framework.Combat.BaseDodgeChance = 5.0; // 5% base dodge
        framework.Combat.DodgePerDEF = 1.0; // +1% per DEF
        framework.Combat.BaseBlockChance = 10.0; // 10% base block (with shield)
        framework.Combat.BlockPerDEF = 1.5; // +1.5% per DEF
        framework.Combat.BlockDamageReduction = 50.0; // Block reduces 50% damage

        // NEW: Skill system baseline
        framework.Skills.SkillPointsPerLevel = 1; // 1 skill point per level
        framework.Skills.SkillDamageBase = 2.0; // 2x normal attack damage
        framework.Skills.SkillDamagePerLevel = 0.3; // +30% per level
        framework.Skills.SkillManaCost = 10; // Costs 10 mana
        framework.Skills.BaseMana = 20; // Start with 20 mana
        framework.Skills.ManaPerLevel = 5; // +5 mana per level
        framework.Skills.SkillCooldown = 3; // Use every 3 turns

        // Generate equipment and test economy
        framework.Equipment = GenerateEquipmentTiers(framework);
        framework.Economy = SimulateEconomicProgression(framework);
        framework.Builds = TestBuildViabilityQuick(framework);

        // Metadata
        framework.Metadata.Timestamp = DateTime.Now;
        framework.Metadata.Generation = 0;
        framework.Metadata.Version = "Generation 36 - Combat + Skills";

        return framework;
    }

    private static ProgressionFrameworkData MutateFramework(ProgressionFrameworkData parent)
    {
        var random = new Random(_generation * 7); // Different seed each generation
        var mutated = new ProgressionFrameworkData();

        // ADAPTIVE MUTATION: Increase exploration if stuck, fine-tune if improving
        double mutationRate = 0.3; // Base rate
        double mutationStrength = 1.0;

        if (_generationsSinceImprovement > 2000)
        {
            // ULTRA STUCK! But use SMART exploration, not random chaos
            mutationRate = 1.0;
            mutationStrength = 2.0; // Medium jumps, not huge (±6 instead of ±15)
        }
        else if (_generationsSinceImprovement > 1000)
        {
            // SEVERELY STUCK! Mutate everything, but controlled
            mutationRate = 1.0; // 100% - mutate everything!
            mutationStrength = 1.5; // Moderate jumps (±4.5)
        }
        else if (_generationsSinceImprovement > 500)
        {
            // Stuck! Increase exploration
            mutationRate = 0.8;
            mutationStrength = 1.2;
        }
        else if (_generationsSinceImprovement > 100)
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

        // Force mutation when severely stuck (mutation rate > 1.0 = always mutate)
        bool forceMutate = mutationRate >= 1.0;

        if (forceMutate || random.NextDouble() < (mutationRate * 1.5))
        {
            int goldDelta = (int)((random.Next(-3, 4) * mutationStrength));
            economy.BaseGoldPerCombat = Math.Clamp(parent.Economy.BaseGoldPerCombat + goldDelta, 8, 20);
        }
        else
        {
            economy.BaseGoldPerCombat = parent.Economy.BaseGoldPerCombat;
        }

        if (forceMutate || random.NextDouble() < (mutationRate * 1.5))
        {
            double scaleDelta = (random.NextDouble() - 0.5) * 2.0 * mutationStrength;
            economy.GoldScalingCoefficient = Math.Clamp(parent.Economy.GoldScalingCoefficient + scaleDelta, 2.0, 6.0);
        }
        else
        {
            economy.GoldScalingCoefficient = parent.Economy.GoldScalingCoefficient;
        }

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

        // NEW: Mutate COMBAT MECHANICS
        mutated.Combat.BaseCritChance = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.BaseCritChance + (random.NextDouble() - 0.5) * 5 * mutationStrength, 0, 20)
            : parent.Combat.BaseCritChance;

        mutated.Combat.CritChancePerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.CritChancePerLevel + (random.NextDouble() - 0.5) * 0.5 * mutationStrength, 0, 2)
            : parent.Combat.CritChancePerLevel;

        mutated.Combat.CritDamageMultiplier = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.CritDamageMultiplier + (random.NextDouble() - 0.5) * 0.5 * mutationStrength, 1.2, 3.0)
            : parent.Combat.CritDamageMultiplier;

        mutated.Combat.BaseDodgeChance = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.BaseDodgeChance + (random.NextDouble() - 0.5) * 5 * mutationStrength, 0, 20)
            : parent.Combat.BaseDodgeChance;

        mutated.Combat.DodgePerDEF = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.DodgePerDEF + (random.NextDouble() - 0.5) * 1 * mutationStrength, 0, 3)
            : parent.Combat.DodgePerDEF;

        mutated.Combat.BaseBlockChance = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.BaseBlockChance + (random.NextDouble() - 0.5) * 10 * mutationStrength, 0, 40)
            : parent.Combat.BaseBlockChance;

        mutated.Combat.BlockPerDEF = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.BlockPerDEF + (random.NextDouble() - 0.5) * 1 * mutationStrength, 0, 3)
            : parent.Combat.BlockPerDEF;

        mutated.Combat.BlockDamageReduction = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Combat.BlockDamageReduction + (random.NextDouble() - 0.5) * 20 * mutationStrength, 25, 75)
            : parent.Combat.BlockDamageReduction;

        // NEW: Mutate SKILL SYSTEM
        mutated.Skills.SkillPointsPerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.SkillPointsPerLevel + random.Next(-1, 2), 1, 3)
            : parent.Skills.SkillPointsPerLevel;

        mutated.Skills.SkillDamageBase = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.SkillDamageBase + (random.NextDouble() - 0.5) * 1 * mutationStrength, 1.0, 4.0)
            : parent.Skills.SkillDamageBase;

        mutated.Skills.SkillDamagePerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.SkillDamagePerLevel + (random.NextDouble() - 0.5) * 0.3 * mutationStrength, 0.1, 1.0)
            : parent.Skills.SkillDamagePerLevel;

        mutated.Skills.SkillManaCost = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.SkillManaCost + random.Next(-5, 6), 5, 30)
            : parent.Skills.SkillManaCost;

        mutated.Skills.BaseMana = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.BaseMana + random.Next(-10, 11), 10, 50)
            : parent.Skills.BaseMana;

        mutated.Skills.ManaPerLevel = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.ManaPerLevel + (random.NextDouble() - 0.5) * 5 * mutationStrength, 2, 15)
            : parent.Skills.ManaPerLevel;

        mutated.Skills.SkillCooldown = random.NextDouble() < mutationRate
            ? Math.Clamp(parent.Skills.SkillCooldown + (random.NextDouble() - 0.5) * 2 * mutationStrength, 1, 10)
            : parent.Skills.SkillCooldown;

        // Re-generate equipment tiers and simulate economy with mutated values
        mutated.Equipment = GenerateEquipmentTiers(mutated);
        mutated.Economy = SimulateEconomicProgression(mutated);
        mutated.Builds = TestBuildViabilityQuick(mutated);

        // Metadata
        mutated.Metadata.Timestamp = DateTime.Now;
        mutated.Metadata.Generation = _generation;
        mutated.Metadata.Version = "Generation 36 - Combat + Skills";
        mutated.Metadata.SimulationsRun = _totalSimulations;

        return mutated;
    }

    // GENETIC ALGORITHM HELPERS

    private static ProgressionFrameworkData TournamentSelect(List<(ProgressionFrameworkData framework, double fitness)> population)
    {
        if (population.Count == 0) return CreateBaselineFramework();

        var random = new Random(_generation * 13);
        int idx1 = random.Next(population.Count);
        int idx2 = random.Next(population.Count);

        // Return the better of the two
        return population[idx1].fitness > population[idx2].fitness
            ? population[idx1].framework
            : population[idx2].framework;
    }

    private static ProgressionFrameworkData CrossoverFrameworks(ProgressionFrameworkData parent1, ProgressionFrameworkData parent2)
    {
        var random = new Random(_generation * 17);
        var child = new ProgressionFrameworkData();

        // CROSSOVER: Mix parameters from both parents
        // Each parameter has 50% chance from either parent

        // Player progression
        child.PlayerProgression.BaseHP = random.NextDouble() < 0.5 ? parent1.PlayerProgression.BaseHP : parent2.PlayerProgression.BaseHP;
        child.PlayerProgression.HPPerLevel = random.NextDouble() < 0.5 ? parent1.PlayerProgression.HPPerLevel : parent2.PlayerProgression.HPPerLevel;
        child.PlayerProgression.BaseSTR = random.NextDouble() < 0.5 ? parent1.PlayerProgression.BaseSTR : parent2.PlayerProgression.BaseSTR;
        child.PlayerProgression.BaseDEF = random.NextDouble() < 0.5 ? parent1.PlayerProgression.BaseDEF : parent2.PlayerProgression.BaseDEF;
        child.PlayerProgression.StatPointsPerLevel = random.NextDouble() < 0.5 ? parent1.PlayerProgression.StatPointsPerLevel : parent2.PlayerProgression.StatPointsPerLevel;

        // Enemy progression
        child.EnemyProgression.BaseHP = random.NextDouble() < 0.5 ? parent1.EnemyProgression.BaseHP : parent2.EnemyProgression.BaseHP;
        child.EnemyProgression.HPScalingCoefficient = random.NextDouble() < 0.5 ? parent1.EnemyProgression.HPScalingCoefficient : parent2.EnemyProgression.HPScalingCoefficient;
        child.EnemyProgression.BaseDamage = random.NextDouble() < 0.5 ? parent1.EnemyProgression.BaseDamage : parent2.EnemyProgression.BaseDamage;
        child.EnemyProgression.DamageScalingCoefficient = random.NextDouble() < 0.5 ? parent1.EnemyProgression.DamageScalingCoefficient : parent2.EnemyProgression.DamageScalingCoefficient;

        // Economy (take as a unit - more likely to be coherent)
        if (random.NextDouble() < 0.5)
        {
            child.Economy.BaseGoldPerCombat = parent1.Economy.BaseGoldPerCombat;
            child.Economy.GoldScalingCoefficient = parent1.Economy.GoldScalingCoefficient;
        }
        else
        {
            child.Economy.BaseGoldPerCombat = parent2.Economy.BaseGoldPerCombat;
            child.Economy.GoldScalingCoefficient = parent2.Economy.GoldScalingCoefficient;
        }

        // Loot (also take as a unit)
        if (random.NextDouble() < 0.5)
        {
            child.Loot.BaseTreasureGold = parent1.Loot.BaseTreasureGold;
            child.Loot.TreasurePerDungeonDepth = parent1.Loot.TreasurePerDungeonDepth;
            child.Loot.EquipmentDropRate = parent1.Loot.EquipmentDropRate;
        }
        else
        {
            child.Loot.BaseTreasureGold = parent2.Loot.BaseTreasureGold;
            child.Loot.TreasurePerDungeonDepth = parent2.Loot.TreasurePerDungeonDepth;
            child.Loot.EquipmentDropRate = parent2.Loot.EquipmentDropRate;
        }

        // Regenerate derived data
        child.Equipment = GenerateEquipmentTiers(child);
        child.Economy = SimulateEconomicProgression(child);
        child.Builds = TestBuildViabilityQuick(child);

        return child;
    }

    private static ProgressionFrameworkData CreateRandomFramework()
    {
        // For diversity injection: random values across entire valid range
        var random = new Random(_generation * 23);
        var framework = new ProgressionFrameworkData();

        // Player - full random in valid ranges
        framework.PlayerProgression.BaseHP = random.Next(15, 41);
        framework.PlayerProgression.HPPerLevel = random.Next(1, 6);
        framework.PlayerProgression.BaseSTR = random.Next(2, 6);
        framework.PlayerProgression.BaseDEF = random.Next(0, 4);
        framework.PlayerProgression.StatPointsPerLevel = random.Next(1, 4);

        // Enemy
        framework.EnemyProgression.BaseHP = random.Next(3, 13);
        framework.EnemyProgression.HPScalingCoefficient = 0.5 + random.NextDouble() * 2.5;
        framework.EnemyProgression.BaseDamage = random.Next(1, 6);
        framework.EnemyProgression.DamageScalingCoefficient = 0.1 + random.NextDouble() * 0.9;

        // Economy
        framework.Economy.BaseGoldPerCombat = random.Next(8, 21);
        framework.Economy.GoldScalingCoefficient = 2.0 + random.NextDouble() * 4.0;

        // Loot
        framework.Loot.BaseTreasureGold = random.Next(10, 51);
        framework.Loot.TreasurePerDungeonDepth = random.Next(15, 61);
        framework.Loot.EquipmentDropRate = random.Next(10, 41);

        // Generate derived data
        framework.Equipment = GenerateEquipmentTiers(framework);
        framework.Economy = SimulateEconomicProgression(framework);
        framework.Builds = TestBuildViabilityQuick(framework);

        return framework;
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
        // USE the economy values that were already set (from mutation/crossover)
        // DON'T overwrite them - just validate and set CanAffordProgression flag
        var economy = framework.Economy;

        // DETERMINISTIC: Use framework hash for consistent randomness
        int seed = HashCode.Combine(
            framework.PlayerProgression.BaseHP,
            framework.PlayerProgression.HPPerLevel,
            framework.Economy.BaseGoldPerCombat,
            framework.Economy.GoldScalingCoefficient
        );
        var random = new Random(seed);

        // If economy values aren't set yet, use defaults
        if (economy.BaseGoldPerCombat <= 0)
        {
            economy.BaseGoldPerCombat = 10;
        }
        if (economy.GoldScalingCoefficient <= 0)
        {
            economy.GoldScalingCoefficient = 2.0 + random.NextDouble() * 3.0;
        }

        // Simulate progression from level 1 to 10
        int cumulativeGold = 50; // Starting gold
        bool economyFailed = false;

        for (int level = 1; level <= 10; level++)
        {
            // DETERMINISTIC: Fixed average combats per level
            int combatsThisLevel = 7; // Was random 5-10, now fixed
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

        // Calculate realistic stat points at level 5
        int availableStatPoints = (testLevel - 1) * framework.PlayerProgression.StatPointsPerLevel; // e.g., 4×2 = 8 points
        int baseSTR = framework.PlayerProgression.BaseSTR;
        int baseDEF = framework.PlayerProgression.BaseDEF;
        int equipmentBonus = testLevel / 3; // Average equipment at level 5 = tier 1

        // Test 3 archetypes with REALISTIC stat distributions
        var builds = new[]
        {
            ("GlassCannon", 1.0, 0.0),  // 100% points into STR
            ("Balanced", 0.6, 0.4),     // 60% STR, 40% DEF
            ("Tank", 0.2, 0.8)          // 20% STR, 80% DEF
        };

        foreach (var (name, strRatio, defRatio) in builds)
        {
            int strPoints = (int)(availableStatPoints * strRatio);
            int defPoints = availableStatPoints - strPoints;

            int totalSTR = baseSTR + strPoints + equipmentBonus;
            int totalDEF = baseDEF + defPoints + equipmentBonus;

            // FAST: Test against single enemy level (not 3 levels - that 3× the cost!)
            double viabilityScore = QuickCombatTest(baseHP, totalSTR, totalDEF, testLevel, enemyHP, enemyDMG);

            viability.ViableBuilds[name] = new BuildRequirements
            {
                MinBaseHP = framework.PlayerProgression.BaseHP,
                MaxBaseHP = framework.PlayerProgression.BaseHP + 10,
                RecommendedRatio = (strPoints * 10, defPoints * 10),
                ViabilityScore = viabilityScore
            };
        }

        viability.MostBalanced = "Balanced";

        return viability;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private static double QuickCombatTest(int playerHP, int playerSTR, int playerDEF, int level, int enemyHP, int enemyDMG)
    {
        int combatsWon = 0;

        // DEMOSCENE: Unroll loop for speed (3 iterations)
        for (int i = 0; i < 3; i++)
        {
            int hp = playerHP;
            int ehp = enemyHP;

            while (ehp > 0 && hp > 0)
            {
                ehp -= playerSTR;
                if (ehp > 0)
                {
                    // Inline Math.Max
                    int damage = enemyDMG - playerDEF;
                    hp -= damage > 1 ? damage : 1;
                }
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

    private static void RenderUltimaDashboard()
    {
        if (_bestFramework == null || _ultimaDashboard == null) return;

        TimeSpan elapsed = DateTime.Now - _startTime;
        double genPerSec = 0;

        if (_recentGenerationTimes.Count > 0)
            genPerSec = _recentGenerationTimes.Average();

        _ultimaDashboard.RenderResearchDashboard(
            generation: _generation,
            fitness: _bestFitness,
            championFitness: _championFitness,
            championGen: _championGeneration,
            genPerSec: genPerSec,
            stuckGens: _generationsSinceImprovement,
            resets: _resetCount,
            phase: _currentPhase,
            elapsed: elapsed,
            currentCandidate: _currentCandidate,      // What we're trying NOW
            currentCandidateFitness: _currentCandidateFitness,
            bestFramework: _bestFramework,             // Current best
            previousBest: _previousBest,               // Previous best
            previousFitness: _previousBestFitness,
            metrics: _latestMetricResults,
            fitnessHistory: _fitnessHistory,
            populationSize: _population.Count
        );

        // Update web state for dashboard
        var avgFitness = _population.Count > 0 ? _population.Average(p => p.fitness) : _bestFitness;
        TunerWebState.Update(new TunerStateData
        {
            Generation = _generation,
            BestFitness = _bestFitness,
            AvgFitness = avgFitness,
            GenPerSec = genPerSec,
            StuckGens = _generationsSinceImprovement,
            Phase = _currentPhase,
            PopulationSize = _population.Count,
            ChampionFitness = _championFitness,
            ChampionGen = _championGeneration,
            Resets = _resetCount,
            Device = "CPU",
            Elapsed = elapsed
        });
    }

    private static void RenderResearchDashboard()
    {
        // Legacy method - redirects to Ultima dashboard
        RenderUltimaDashboard();
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

        // Get difficulty multiplier based on champion performance
        double difficultyMult = ProgressionFrameworkResearcher.GetDifficultyMultiplier();
        double championFitness = ProgressionFrameworkResearcher.GetChampionFitness();
        if (difficultyMult > 1.0)
        {
            result.Details.Add($"Difficulty: {difficultyMult:F1}× (Champion: {championFitness:F1})");
        }

        // Test ALL levels 1-10, not just 1,3,5
        for (int level = 1; level <= 10; level++)
        {
            int playerHP = framework.PlayerProgression.BaseHP + (int)(level * framework.PlayerProgression.HPPerLevel);

            // REALISTIC stat allocation: StatPointsPerLevel split between STR/DEF
            // Use a balanced 60/40 split (slightly favor offense)
            int statPoints = (level - 1) * framework.PlayerProgression.StatPointsPerLevel;
            int strPoints = (int)(statPoints * 0.6);
            int defPoints = statPoints - strPoints;

            int playerSTR = framework.PlayerProgression.BaseSTR + strPoints;
            int playerDEF = framework.PlayerProgression.BaseDEF + defPoints;

            // Add AVERAGE equipment bonus (not best tier - realistic mid-game gear)
            int equipmentTier = Math.Min(5, level / 3); // Slower gear progression than ideal
            playerSTR += equipmentTier;
            playerDEF += equipmentTier;

            int enemyHP = framework.EnemyProgression.BaseHP + (int)(level * framework.EnemyProgression.HPScalingCoefficient);
            int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(level * framework.EnemyProgression.DamageScalingCoefficient);

            var combat = SimulateCombatDetailed(playerHP, playerSTR, playerDEF, enemyHP, enemyDMG, level);

            // Granular scoring based on combat quality
            double levelScore = 0;

            // 1. Win rate (0-40 points) - PROGRESSIVE DIFFICULTY
            // Window narrows as champion improves (tuning the tuner!)
            double idealWinRate = 0.85; // Target center
            double windowSize = 0.15 / difficultyMult; // Narrows at higher difficulty
            double minWinRate = idealWinRate - windowSize;
            double maxWinRate = idealWinRate + windowSize;

            if (combat.WinRate >= minWinRate && combat.WinRate <= maxWinRate)
                levelScore += 40; // Perfect - within acceptable window
            else if (combat.WinRate < minWinRate)
                levelScore += (combat.WinRate / minWinRate) * 40; // Too hard
            else if (combat.WinRate > maxWinRate)
                levelScore += Math.Max(0, 40 - (combat.WinRate - maxWinRate) * 200 * difficultyMult); // Too easy (penalize more at high difficulty)

            // 2. Time to kill (0-40 points) - PROGRESSIVE DIFFICULTY
            // Ideal: 5 turns, window narrows as champion improves
            double idealTTK = 5.0;
            double ttkWindow = 1.5 / difficultyMult; // 1.5 turns initially, narrows to 1 turn at hard mode
            double ttkScore = 0;

            double ttkDelta = Math.Abs(combat.AvgTurns - idealTTK);
            if (ttkDelta <= ttkWindow)
                ttkScore = 40 * (1.0 - (ttkDelta / ttkWindow) * 0.25); // Perfect to good range
            else if (ttkDelta <= ttkWindow * 2)
                ttkScore = 30 * (1.0 - (ttkDelta - ttkWindow) / ttkWindow); // Acceptable range
            else
                ttkScore = Math.Max(0, 20 - (ttkDelta - ttkWindow * 2) * 5 * difficultyMult); // Poor

            levelScore += ttkScore;

            // 3. Consistency (0-20 points) - PROGRESSIVE DIFFICULTY
            // Higher difficulty = less tolerance for variance
            double maxVariance = 6.0 / difficultyMult; // 6 turns variance at beginner, 4 at hard
            double varianceScore = Math.Max(0, 20 * (1.0 - combat.TurnVariance / maxVariance));
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

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private CombatStats SimulateCombatDetailed(int playerHP, int playerSTR, int playerDEF, int enemyHP, int enemyDMG, int level)
    {
        const int SIMULATIONS = 10;
        int wins = 0;

        // DEMOSCENE OPTIMIZATION: stackalloc instead of List (zero GC pressure!)
        Span<int> turnCounts = stackalloc int[SIMULATIONS];
        int turnIndex = 0;

        for (int sim = 0; sim < SIMULATIONS; sim++)
        {
            int hp = playerHP;
            int ehp = enemyHP;
            int turns = 0;

            while (ehp > 0 && hp > 0 && turns < 50)
            {
                turns++;
                ehp -= playerSTR;
                if (ehp > 0)
                {
                    // Inline Math.Max
                    int damage = enemyDMG - playerDEF;
                    hp -= damage > 1 ? damage : 1;
                }
            }

            if (hp > 0)
            {
                wins++;
                turnCounts[turnIndex++] = turns;
            }
        }

        // Manual average/variance (NO LINQ allocations!)
        double avgTurns = 50;
        double variance = 0;

        if (turnIndex > 0)
        {
            int sum = 0;
            for (int i = 0; i < turnIndex; i++)
                sum += turnCounts[i];
            avgTurns = sum / (double)turnIndex;

            if (turnIndex > 1)
            {
                double sumSq = 0;
                for (int i = 0; i < turnIndex; i++)
                {
                    double diff = turnCounts[i] - avgTurns;
                    sumSq += diff * diff;
                }
                variance = Math.Sqrt(sumSq / turnIndex);
            }
        }

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
    public double Weight => 0.25;

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
    public double Weight => 0.10;

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

        // Test skill usage at 3 levels only (was 5, now 3 for speed: 1, 5, 10)
        var testLevels = new[] { 1, 5, 10 };
        foreach (int level in testLevels)
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
        // DETERMINISTIC: Simplified stun benefit calculation
        // Stun lets you skip ~25% of enemy turns (diminishing with resistance)
        // Approximate benefit as reducing enemy effective damage by 15%
        int normalTurns = (enemyHP / str) + 1;
        int normalDamageTaken = normalTurns * Math.Max(1, enemyDMG - def);

        // With stuns: roughly 15% less damage taken (stuns land ~25% of time, save 1 hit each)
        int stunDamageTaken = (int)(normalDamageTaken * 0.85);

        // Return benefit as percentage of HP saved
        return (normalDamageTaken - stunDamageTaken) / (double)hp;
    }

    private double TestBerserkerRage(int hp, int str, int def, int enemyHP, int enemyDMG)
    {
        // DETERMINISTIC: Calculate berserker rage outcome mathematically
        // With rage: 2x damage dealt, 1.5x damage taken for 3 turns
        int rageTurns = Math.Min(3, (enemyHP / (str * 2)) + 1);
        int normalTurnsToKill = (enemyHP / str) + (enemyHP % str > 0 ? 1 : 0);

        // Damage taken with rage active (first 3 turns deal 2x damage, take 1.5x)
        int rageDamageTaken = rageTurns * (int)(Math.Max(1, enemyDMG - def) * 1.5);
        // Remaining turns (if any) are normal
        int remainingTurns = Math.Max(0, normalTurnsToKill - rageTurns);
        int normalDamageTaken = normalTurnsToKill * Math.Max(1, enemyDMG - def);

        int totalDamageWithRage = rageDamageTaken + (remainingTurns * Math.Max(1, enemyDMG - def));

        // Return benefit: negative if rage makes you take more damage
        return (normalDamageTaken - totalDamageWithRage) / (double)hp;
    }

    private double TestStaminaEconomy(int str, int enemyHP)
    {
        // Can player afford to use skills? (Power Strike costs 5 stamina)
        int turnsToKill = (int)Math.Ceiling(enemyHP / (double)(str * 1.5)); // With Power Strike
        int staminaNeeded = turnsToKill * 5;
        int staminaAvailable = 12; // Starting stamina

        return Math.Min(1.0, staminaAvailable / (double)staminaNeeded);
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private double SimulateCombat(int hp, int str, int def, int enemyHP, int enemyDMG, bool useSkills)
    {
        // DETERMINISTIC: Single combat simulation
        int strDamage = useSkills ? (int)(str * 1.5) : str;

        int playerHP = hp;
        int eHP = enemyHP;

        while (playerHP > 0 && eHP > 0)
        {
            eHP -= strDamage;
            if (eHP > 0)
            {
                int damage = enemyDMG - def;
                playerHP -= damage > 1 ? damage : 1;
            }
        }

        return playerHP > 0 ? 1.0 : 0.0;
    }
}

public class BuildDiversityMetric : IFitnessMetric
{
    public string Name => "Build Diversity";
    public double Weight => 0.15;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };

        // Count viable builds + extract scores (DEMOSCENE: manual loop, no LINQ!)
        int viableCount = 0;
        double sumScores = 0;
        int buildCount = 0;

        foreach (var build in framework.Builds.ViableBuilds)
        {
            if (build.Value.ViabilityScore > 60)
                viableCount++;

            sumScores += build.Value.ViabilityScore;
            buildCount++;
        }

        // 1. Viability count (0-50 points)
        double viabilityScore = (viableCount / 3.0) * 50;

        // 2. Differentiation (0-50 points) - DEMOSCENE: manual variance, no LINQ!
        double variance = 0;
        if (buildCount > 1)
        {
            double mean = sumScores / buildCount;
            double sumSq = 0;

            foreach (var build in framework.Builds.ViableBuilds)
            {
                double diff = build.Value.ViabilityScore - mean;
                sumSq += diff * diff;
            }

            variance = Math.Sqrt(sumSq / buildCount);
        }

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

        result.Details.Add($"{viableCount}/3 builds viable (>60% score)");
        result.Details.Add($"Build scores: {string.Join(", ", framework.Builds.ViableBuilds.Select(b => $"{b.Key}={b.Value.ViabilityScore:F0}"))}");
        result.Details.Add($"Variance: {variance:F1} (target: 10-20)");

        foreach (var build in framework.Builds.ViableBuilds)
        {
            if (build.Value.ViabilityScore < 60)
                result.Warnings.Add($"{build.Key}: Only {build.Value.ViabilityScore:F0}% viable");
        }

        return result;
    }
}

public class ProgressionStrataMetric : IFitnessMetric
{
    public string Name => "Progression Strata";
    public double Weight => 0.15;

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };

        // Test combat at level 5 with 3 equipment tiers to validate progression feel
        int testLevel = 5;
        int baseHP = framework.PlayerProgression.BaseHP + (testLevel * (int)framework.PlayerProgression.HPPerLevel);

        // Realistic stat allocation (60/40 STR/DEF split)
        int statPoints = (testLevel - 1) * framework.PlayerProgression.StatPointsPerLevel;
        int baseSTR = framework.PlayerProgression.BaseSTR + (int)(statPoints * 0.6);
        int baseDEF = framework.PlayerProgression.BaseDEF + (statPoints - (int)(statPoints * 0.6));

        int enemyHP = framework.EnemyProgression.BaseHP + (int)(testLevel * framework.EnemyProgression.HPScalingCoefficient);
        int enemyDMG = framework.EnemyProgression.BaseDamage + (int)(testLevel * framework.EnemyProgression.DamageScalingCoefficient);

        double totalScore = 0;

        // TIER 0: DEFAULT/NAKED (no equipment) - Should be HARD but WINNABLE
        int t0STR = baseSTR;
        int t0DEF = baseDEF;
        double t0WinRate = TestWinRate(baseHP, t0STR, t0DEF, enemyHP, enemyDMG);

        // Target: 60-70% win rate (challenging, requires skill)
        double t0Score = 0;
        if (t0WinRate >= 0.60 && t0WinRate <= 0.70)
            t0Score = 100; // Perfect - hard but winnable!
        else if (t0WinRate < 0.60)
            t0Score = Math.Max(0, (t0WinRate / 0.60) * 100); // Too hard
        else if (t0WinRate > 0.70)
            t0Score = Math.Max(0, 100 - (t0WinRate - 0.70) * 300); // Too easy (big penalty!)

        totalScore += t0Score * 0.33; // Tier 0 = 33% of this metric
        result.Details.Add($"Tier 0 (Default): {t0WinRate:P0} wins (target: 60-70%) → {t0Score:F0}/100");

        if (t0WinRate < 0.50)
            result.Warnings.Add($"Default gear TOO HARD! Only {t0WinRate:P0} win rate - game unwinnable!");
        else if (t0WinRate > 0.80)
            result.Warnings.Add($"Default gear TOO EASY! {t0WinRate:P0} win rate - no progression needed!");

        // TIER 2: MID-GAME EQUIPMENT - Should be EASIER not EASY
        int t2STR = baseSTR + 2; // +2 weapon
        int t2DEF = baseDEF + 2; // +2 armor
        double t2WinRate = TestWinRate(baseHP, t2STR, t2DEF, enemyHP, enemyDMG);

        // Target: 75-85% win rate (comfortable, rewarding)
        double t2Score = 0;
        if (t2WinRate >= 0.75 && t2WinRate <= 0.85)
            t2Score = 100; // Perfect - upgrade feels good!
        else if (t2WinRate < 0.75)
            t2Score = Math.Max(0, (t2WinRate / 0.75) * 100); // Still too hard
        else if (t2WinRate > 0.85)
            t2Score = Math.Max(0, 100 - (t2WinRate - 0.85) * 200); // Getting too easy

        totalScore += t2Score * 0.33; // Tier 2 = 33% of this metric
        result.Details.Add($"Tier 2 (Mid-gear): {t2WinRate:P0} wins (target: 75-85%) → {t2Score:F0}/100");

        // TIER 5: BEST EQUIPMENT - Should FEEL OP but NOT BE TRIVIAL
        int t5STR = baseSTR + 5; // +5 enchanted blade
        int t5DEF = baseDEF + 5; // +5 dragon scale
        double t5WinRate = TestWinRate(baseHP, t5STR, t5DEF, enemyHP, enemyDMG);

        // Target: 90-95% win rate (feels OP, but can still lose!)
        double t5Score = 0;
        if (t5WinRate >= 0.90 && t5WinRate <= 0.95)
            t5Score = 100; // Perfect - power fantasy fulfilled, still fair!
        else if (t5WinRate < 0.90)
            t5Score = Math.Max(0, (t5WinRate / 0.90) * 100); // Best gear should dominate more
        else if (t5WinRate >= 0.98)
            t5Score = 0; // Trivial = BROKEN, 100% win rate kills challenge!
        else if (t5WinRate > 0.95)
            t5Score = Math.Max(0, 100 - (t5WinRate - 0.95) * 500); // Steep penalty approaching trivial

        totalScore += t5Score * 0.34; // Tier 5 = 34% of this metric
        result.Details.Add($"Tier 5 (Best gear): {t5WinRate:P0} wins (target: 90-95%) → {t5Score:F0}/100");

        if (t5WinRate >= 0.98)
            result.Warnings.Add($"Best gear TRIVIALIZES game! {t5WinRate:P0} win rate - no challenge left!");
        else if (t5WinRate < 0.85)
            result.Warnings.Add($"Best gear too weak! Only {t5WinRate:P0} - doesn't feel rewarding!");

        // Validate progression curve (each tier should be better than last)
        if (t2WinRate <= t0WinRate)
        {
            result.Warnings.Add($"Tier 2 not better than Tier 0! No progression reward!");
            totalScore *= 0.5; // Big penalty
        }

        if (t5WinRate <= t2WinRate + 0.05)
        {
            result.Warnings.Add($"Tier 5 barely better than Tier 2! Best gear doesn't feel special!");
            totalScore *= 0.8;
        }

        result.Score = totalScore;
        result.WeightedScore = result.Score * Weight;

        return result;
    }

    [System.Runtime.CompilerServices.MethodImpl(System.Runtime.CompilerServices.MethodImplOptions.AggressiveInlining)]
    private double TestWinRate(int hp, int str, int def, int enemyHP, int enemyDMG)
    {
        // DETERMINISTIC: Just calculate the combat outcome directly
        // No randomness needed - it's turn-based math
        int playerHP = hp;
        int eHP = enemyHP;

        while (playerHP > 0 && eHP > 0)
        {
            // Player attacks
            eHP -= str;
            if (eHP > 0)
            {
                // Enemy attacks back
                int damage = enemyDMG - def;
                playerHP -= damage > 1 ? damage : 1;
            }
        }

        // Return 1.0 if player wins, 0.0 if enemy wins
        return playerHP > 0 ? 1.0 : 0.0;
    }
}

public static class FitnessEvaluator
{
    private static readonly List<IFitnessMetric> _metrics = new()
    {
        new CombatBalanceMetric(),      // 25% (core gameplay)
        new EconomicHealthMetric(),     // 25% (CRITICAL - can't be broken)
        new ProgressionStrataMetric(),  // 15% (NEW - validates user's progression philosophy!)
        new SkillBalanceMetric(),       // 15% (skills balanced)
        new EquipmentCurveMetric(),     // 10% (power curve)
        new DifficultyPacingMetric()    // 10% (smoothness)
        // BuildDiversityMetric DISABLED - stuck at 50.0, too expensive
        // Total: 100% - tuner evolves with game features!
    };

    public static (double totalFitness, List<MetricResult> results) EvaluateComprehensive(ProgressionFrameworkData framework)
    {
        // SIMPLE: Serial metric evaluation (parallel overhead was killing us with only 5 metrics!)
        var results = new List<MetricResult>(_metrics.Count);

        foreach (var metric in _metrics)
        {
            results.Add(metric.Evaluate(framework));
        }

        // Check for critical failures first
        bool hasCriticalFailure = results.Any(r => r.Critical && r.Score < 50);
        if (hasCriticalFailure)
        {
            return (0, results); // Instant failure
        }

        // Sum weighted scores
        double totalWeightedScore = results.Sum(r => r.WeightedScore);

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


