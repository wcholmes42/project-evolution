namespace ProjectEvolution.Game;

public class SimulationConfig
{
    public int MobDetectionRange { get; set; } = 3;
    public int MaxMobs { get; set; } = 20;
    public int MinMobs { get; set; } = 5;
    public int PlayerStartHP { get; set; } = 10;
    public int PlayerStrength { get; set; } = 2;
    public int PlayerDefense { get; set; } = 1;
    public int EncounterRateMultiplier { get; set; } = 100; // Percentage
    public int SimulationSpeed { get; set; } = 100; // ms delay per turn
    public bool ShowVisuals { get; set; } = true;
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

    public SimulationStats RunSimulation(int numberOfRuns)
    {
        Console.WriteLine($"\n🎮 Starting {numberOfRuns} automated game runs...\n");

        for (int run = 0; run < numberOfRuns; run++)
        {
            RunSingleGame(run + 1, numberOfRuns);
        }

        return _stats;
    }

    private void RunSingleGame(int runNumber, int totalRuns)
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
            _ui.AddMessage($"Goal: {autoPlayer.GetCurrentGoalDescription()}");
            _ui.RenderCommandBar(false);
        }

        int maxTurns = 500; // Safety limit
        int turns = 0;

        while (autoPlayer.IsAlive && turns < maxTurns)
        {
            // Handle combat if in combat
            if (game.CombatEnded == false && game.EnemyHP > 0)
            {
                // In combat
                if (autoPlayer.ShouldUsePotion())
                {
                    game.UsePotion();
                    if (_config.ShowVisuals)
                    {
                        _ui.AddMessage("🤖 Used potion!");
                        Thread.Sleep(_config.SimulationSpeed);
                    }
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
                    if (!fled && game.PlayerHP <= 0)
                    {
                        break;
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
                        Thread.Sleep(_config.SimulationSpeed);
                    }

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (game.IsWon)
                        {
                            autoPlayer.CombatsWon++;
                        }
                    }
                }
            }
            else
            {
                // Normal exploration
                autoPlayer.PlayTurn();
                turns++;

                if (_config.ShowVisuals && turns % 5 == 0) // Update goal display every 5 turns
                {
                    _ui.AddMessage($"🤖 {autoPlayer.GetCurrentGoalDescription()} | Turn {turns}");
                    _ui.RenderStatusBar(game);
                    _ui.RenderMap(game);
                    Thread.Sleep(_config.SimulationSpeed);
                }
            }

            // Check if dead
            if (game.PlayerHP <= 0)
            {
                break;
            }
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
    }
}
