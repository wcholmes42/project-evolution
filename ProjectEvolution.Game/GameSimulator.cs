namespace ProjectEvolution.Game;

public class SimulationConfig
{
    // World & Mob Spawning
    public int MobDetectionRange { get; set; } = 3;
    public int MaxMobs { get; set; } = 29; // AI-optimized
    public int MinMobs { get; set; } = 5;
    public int EncounterRateMultiplier { get; set; } = 100; // Percentage

    // Player Starting Stats
    public int PlayerStartHP { get; set; } = 20; // NEW: More reasonable starting value
    public int PlayerStrength { get; set; } = 3;
    public int PlayerDefense { get; set; } = 1;

    // NEW: Enemy Scaling Parameters
    public double EnemyHPScaling { get; set; } = 1.5; // HP per player level
    public double EnemyDamageScaling { get; set; } = 0.5; // Damage per player level
    public int BaseEnemyHP { get; set; } = 5; // Starting enemy HP
    public int BaseEnemyDamage { get; set; } = 2; // Starting enemy damage

    // NEW: Equipment Parameters
    public int EquipmentDropRate { get; set; } = 20; // Percent chance per victory
    public double EquipmentBonusScaling { get; set; } = 1.0; // Multiplier for weapon/armor bonuses

    // NEW: Leveling Parameters
    public int XPPerLevel { get; set; } = 100; // Base XP requirement per level
    public int HPPerLevel { get; set; } = 2; // HP gained per level

    // Simulation Controls
    public int SimulationSpeed { get; set; } = 100; // ms delay per turn
    public bool ShowVisuals { get; set; } = true;
}

// Floating point config for gradient descent
public class FloatConfig
{
    public double MobDetectionRange { get; set; } = 3.0;
    public double MaxMobs { get; set; } = 29.0;
    public double MinMobs { get; set; } = 5.0;
    public double PlayerStartHP { get; set; } = 20.0;
    public double PlayerStrength { get; set; } = 3.0;
    public double PlayerDefense { get; set; } = 1.0;
    public double EnemyHPScaling { get; set; } = 1.5;
    public double EnemyDamageScaling { get; set; } = 0.5;
    public double BaseEnemyHP { get; set; } = 5.0;
    public double BaseEnemyDamage { get; set; } = 2.0;

    public SimulationConfig ToSimConfig()
    {
        return new SimulationConfig
        {
            MobDetectionRange = (int)Math.Round(MobDetectionRange),
            MaxMobs = (int)Math.Round(MaxMobs),
            MinMobs = (int)Math.Round(MinMobs),
            PlayerStartHP = (int)Math.Round(PlayerStartHP),
            PlayerStrength = (int)Math.Round(PlayerStrength),
            PlayerDefense = (int)Math.Round(PlayerDefense),
            EnemyHPScaling = EnemyHPScaling,
            EnemyDamageScaling = EnemyDamageScaling,
            BaseEnemyHP = (int)Math.Round(BaseEnemyHP),
            BaseEnemyDamage = (int)Math.Round(BaseEnemyDamage),
            ShowVisuals = false,
            SimulationSpeed = 0
        };
    }
}

public class SimulationStats
{
    public int TotalRuns { get; set; }
    public int Deaths { get; set; }
    public int TotalTurns { get; set; }
    public int TotalCombatsWon { get; set; }
    public int TotalGoldEarned { get; set; }
    public int MaxTurnsSurvived { get; set; }
    public int MaxLevel { get; set; }
    public List<string> DeathReasons { get; set; } = new List<string>();

    // NEW: Progression tracking
    public int DeathsAtLevel1 { get; set; }
    public int DeathsAtLevel2to4 { get; set; }
    public int DeathsAtLevel5Plus { get; set; }
    public double AverageLevel => TotalRuns > 0 ? (double)MaxLevel / TotalRuns : 0;

    public double AverageTurnsPerRun => TotalRuns > 0 ? (double)TotalTurns / TotalRuns : 0;
    public double SurvivalRate => TotalRuns > 0 ? (double)(TotalRuns - Deaths) / TotalRuns * 100 : 0;
    public double AverageCombatsWon => TotalRuns > 0 ? (double)TotalCombatsWon / TotalRuns : 0;

    public void Display()
    {
        Console.WriteLine("\n╔══════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    SIMULATION STATISTICS                     ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║ Total Runs:          {TotalRuns,10}                              ║");
        Console.WriteLine($"║ Deaths:              {Deaths,10}   ({Deaths * 100.0 / Math.Max(1, TotalRuns):F1}%)                  ║");
        Console.WriteLine($"║ Total Turns:         {TotalTurns,10}                              ║");
        Console.WriteLine($"║ Avg Turns/Run:       {AverageTurnsPerRun,10:F1}                              ║");
        Console.WriteLine($"║ Max Turns Survived:  {MaxTurnsSurvived,10}                              ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║ Total Combats Won:   {TotalCombatsWon,10}                              ║");
        Console.WriteLine($"║ Avg Combats/Run:     {AverageCombatsWon,10:F1}                              ║");
        Console.WriteLine($"║ Total Gold Earned:   {TotalGoldEarned,10}                              ║");
        Console.WriteLine($"║ Max Level Reached:   {MaxLevel,10}                              ║");
        Console.WriteLine("╠══════════════════════════════════════════════════════════════╣");
        Console.WriteLine("║ Top Death Reasons:                                           ║");

        var topDeaths = DeathReasons
            .GroupBy(r => r)
            .OrderByDescending(g => g.Count())
            .Take(3);

        foreach (var group in topDeaths)
        {
            int count = group.Count();
            double percent = count * 100.0 / Math.Max(1, Deaths);
            Console.WriteLine($"║   {group.Key,-40} {count,5} ({percent:F1}%) ║");
        }

        Console.WriteLine("╚══════════════════════════════════════════════════════════════╝");
    }
}

public class GameSimulator
{
    private SimulationConfig _config;
    private SimulationStats _stats = new SimulationStats();
    private UIRenderer _ui;

    public GameSimulator(SimulationConfig config)
    {
        _config = config;
        _ui = new UIRenderer();
    }

    private bool _abortAll = false;

    public SimulationStats RunSimulation(int numberOfRuns)
    {
        if (_config.ShowVisuals)
        {
            // Visual mode - run sequentially
            Console.WriteLine($"\n🎮 Starting {numberOfRuns} automated game runs...\n");
            _abortAll = false;

            for (int run = 0; run < numberOfRuns; run++)
            {
                if (_abortAll)
                {
                    Console.WriteLine($"\n⚠️ Aborted after {run} runs.");
                    break;
                }

                bool aborted = RunSingleGame(run + 1, numberOfRuns);
                if (aborted)
                {
                    _abortAll = true;
                }
            }
        }
        else
        {
            // Headless mode - PARALLEL EXECUTION!
            int cores = Environment.ProcessorCount;
            int completed = 0;
            var startTime = DateTime.Now;

            // Thread-safe stats collection
            var threadLocalStats = new object();

            Parallel.For(0, numberOfRuns, new ParallelOptions { MaxDegreeOfParallelism = cores }, (run) =>
            {
                var game = new RPGGame();
                game.SetPlayerStats(_config.PlayerStrength, _config.PlayerDefense);
                game.StartWorldExploration();
                var autoPlayer = new AutoPlayer(game);

                int maxTurns = 500;
                int turns = 0;
                int combatRounds = 0;

                while (autoPlayer.IsAlive && turns < maxTurns)
                {
                    if (game.CombatEnded == false && game.EnemyHP > 0)
                    {
                        combatRounds++;
                        if (combatRounds > 50)
                        {
                            game.EndCombatStalemate();
                            combatRounds = 0;
                            continue;
                        }

                        if (autoPlayer.ShouldUsePotion()) { game.UsePotion(); combatRounds--; }
                        else if (autoPlayer.ShouldFlee())
                        {
                            bool fled = game.AttemptFlee();
                            autoPlayer.CombatsFled++;
                            if (fled || game.PlayerHP <= 0) combatRounds = 0;
                            if (game.PlayerHP <= 0) break;
                        }
                        else
                        {
                            game.ExecuteGameLoopRoundWithRandomHits(autoPlayer.DecideCombatAction(), CombatAction.Attack);
                            if (game.CombatEnded)
                            {
                                game.ProcessGameLoopVictory();
                                if (game.IsWon) autoPlayer.CombatsWon++;
                                combatRounds = 0;
                            }
                        }
                    }
                    else
                    {
                        combatRounds = 0;
                        autoPlayer.PlayTurn();
                        turns++;

                        if (!game.InDungeon)
                        {
                            var mob = game.GetMobAt(game.PlayerX, game.PlayerY);
                            if (mob != null)
                            {
                                game.TriggerMobEncounter(mob);
                                continue;
                            }
                            else if (game.RollForEncounter())
                            {
                                game.TriggerEncounter();
                                continue;
                            }
                        }
                    }

                    if (game.PlayerHP <= 0) break;
                }

                // Collect stats (thread-safe)
                lock (threadLocalStats)
                {
                    _stats.TotalRuns++;
                    completed++;

                    // Update progress display
                    if (completed % 10 == 0 || completed == numberOfRuns)
                    {
                        double elapsed = (DateTime.Now - startTime).TotalSeconds;
                        double rate = completed / elapsed;
                        Console.Write($"\r🚀 {cores} cores: {completed}/{numberOfRuns} ({rate:F0} games/sec) ");
                    }

                    if (game.PlayerHP <= 0)
                    {
                        _stats.Deaths++;
                        _stats.DeathReasons.Add($"Lvl{game.PlayerLevel} Turn{turns} vs {game.EnemyName}");
                    }
                    _stats.TotalTurns += turns;
                    _stats.TotalCombatsWon += autoPlayer.CombatsWon;
                    _stats.TotalGoldEarned += game.PlayerGold;
                    _stats.MaxTurnsSurvived = Math.Max(_stats.MaxTurnsSurvived, turns);
                    _stats.MaxLevel = Math.Max(_stats.MaxLevel, game.PlayerLevel);
                }
            });

            double totalElapsed = (DateTime.Now - startTime).TotalSeconds;
            Console.WriteLine($"✅ Done in {totalElapsed:F1}s!");
        }

        return _stats;
    }

    private bool RunSingleGame(int runNumber, int totalRuns)
    {
        var game = new RPGGame();
        game.SetPlayerStats(_config.PlayerStrength, _config.PlayerDefense);
        game.StartWorldExploration();

        var autoPlayer = new AutoPlayer(game);

        if (_config.ShowVisuals)
        {
            _ui.Initialize();
            _ui.RenderStatusBar(game);
            _ui.RenderMap(game);
            _ui.AddMessage($"🤖 AUTO-PLAY RUN {runNumber}/{totalRuns}");
            _ui.RenderCommandBar(false);
            _ui.RenderDebugPanel(game, autoPlayer);
        }

        int maxTurns = 500; // Safety limit
        int turns = 0;
        bool userAborted = false;
        int combatRounds = 0;
        int maxCombatRounds = 50; // Prevent infinite miss loops!

        while (autoPlayer.IsAlive && turns < maxTurns && !userAborted)
        {
            // Handle combat if in combat
            if (game.CombatEnded == false && game.EnemyHP > 0)
            {
                combatRounds++;

                // SAFETY: Break out of infinite miss loops
                if (combatRounds > maxCombatRounds)
                {
                    game.EndCombatStalemate();
                    if (_config.ShowVisuals)
                    {
                        _ui.AddMessage("⚠️ Combat stalemate - both disengage!");
                        Thread.Sleep(_config.SimulationSpeed * 2);
                    }
                    combatRounds = 0;
                    // DON'T continue here - let it process victory/defeat
                }
                // In combat - execute actions
                else if (autoPlayer.ShouldUsePotion())
                {
                    game.UsePotion();
                    if (_config.ShowVisuals)
                    {
                        _ui.AddMessage("🤖 Used potion!");
                        Thread.Sleep(_config.SimulationSpeed);
                    }
                    // Potion use doesn't count as a combat round
                    combatRounds--;
                }
                else if (autoPlayer.ShouldFlee())
                {
                    bool fled = game.AttemptFlee();
                    autoPlayer.CombatsFled++;
                    if (_config.ShowVisuals)
                    {
                        _ui.AddMessage(fled ? "🤖 Fled successfully!" : "🤖 Failed to flee!");
                        Thread.Sleep(_config.SimulationSpeed);
                    }
                    if (fled || game.PlayerHP <= 0)
                    {
                        combatRounds = 0;
                        if (game.PlayerHP <= 0) break;
                    }
                }
                else
                {
                    var action = autoPlayer.DecideCombatAction();
                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);

                    if (_config.ShowVisuals)
                    {
                        _ui.AddMessage($"🤖 {action}: {game.CombatLog}");
                        _ui.RenderStatusBar(game);
                        _ui.RenderDebugPanel(game, autoPlayer);

                        // Check for ESC in combat too
                        if (Console.KeyAvailable)
                        {
                            var key = Console.ReadKey(intercept: true);
                            if (key.Key == ConsoleKey.Escape)
                            {
                                userAborted = true;
                                break;
                            }
                        }

                        Thread.Sleep(_config.SimulationSpeed);
                    }
                }

                // Check if combat ended (after all actions)
                if (game.CombatEnded)
                {
                    game.ProcessGameLoopVictory();
                    if (game.IsWon)
                    {
                        autoPlayer.CombatsWon++;
                    }
                    combatRounds = 0; // Reset for next combat
                }
            }
            else
            {
                combatRounds = 0; // Not in combat, reset counter
                // Normal exploration
                autoPlayer.PlayTurn();
                turns++;

                // CHECK FOR MOB ENCOUNTERS after movement!
                if (!game.InDungeon)
                {
                    var mob = game.GetMobAt(game.PlayerX, game.PlayerY);

                    if (mob != null)
                    {
                        // Walked into a mob - trigger encounter!
                        if (_config.ShowVisuals)
                        {
                            _ui.AddMessage($"⚔️ AI walked into {mob.Name} [Lvl{mob.Level}]!");
                            Thread.Sleep(_config.SimulationSpeed * 2);
                        }

                        game.TriggerMobEncounter(mob);

                        if (_config.ShowVisuals)
                        {
                            _ui.RenderStatusBar(game);
                            _ui.RenderDebugPanel(game, autoPlayer);
                        }

                        // Combat will be handled on next loop iteration
                        continue;
                    }
                    // Also check for random encounters
                    else if (game.RollForEncounter())
                    {
                        game.TriggerEncounter();

                        if (_config.ShowVisuals)
                        {
                            _ui.AddMessage($"💥 Ambush! {game.EnemyName}!");
                            _ui.RenderStatusBar(game);
                            Thread.Sleep(_config.SimulationSpeed * 2);
                        }

                        continue;
                    }
                }

                if (_config.ShowVisuals)
                {
                    _ui.RenderStatusBar(game);
                    _ui.RenderMap(game);
                    _ui.RenderDebugPanel(game, autoPlayer);

                    // Check for ESC key to abort
                    if (Console.KeyAvailable)
                    {
                        var key = Console.ReadKey(intercept: true);
                        if (key.Key == ConsoleKey.Escape)
                        {
                            _ui.AddMessage("⚠️ Simulation aborted by user!");
                            Thread.Sleep(1000);
                            userAborted = true;
                        }
                    }

                    Thread.Sleep(_config.SimulationSpeed);
                }
            }

            // Check if dead
            if (game.PlayerHP <= 0)
            {
                break;
            }
        }

        // Don't count aborted runs in stats
        if (userAborted)
        {
            if (_config.ShowVisuals)
            {
                _ui.Cleanup();
            }
            return true; // Signal abort
        }

        // Collect stats
        _stats.TotalRuns++;
        if (game.PlayerHP <= 0)
        {
            _stats.Deaths++;
            _stats.DeathReasons.Add($"Lvl{game.PlayerLevel} Turn{turns} vs {game.EnemyName}");
        }
        _stats.TotalTurns += turns;
        _stats.TotalCombatsWon += autoPlayer.CombatsWon;
        _stats.TotalGoldEarned += game.PlayerGold;
        _stats.MaxTurnsSurvived = Math.Max(_stats.MaxTurnsSurvived, turns);
        _stats.MaxLevel = Math.Max(_stats.MaxLevel, game.PlayerLevel);

        // Quick progress indicator for batch runs
        if (!_config.ShowVisuals && runNumber % 10 == 0)
        {
            Console.Write($"\rCompleted {runNumber}/{totalRuns} runs... ");
        }

        if (_config.ShowVisuals)
        {
            _ui.AddMessage($"Run {runNumber} ended: {turns} turns, Level {game.PlayerLevel}, {game.PlayerGold}g");
            Thread.Sleep(_config.SimulationSpeed * 2);
            _ui.Cleanup();
        }

        return false; // Not aborted
    }
}
