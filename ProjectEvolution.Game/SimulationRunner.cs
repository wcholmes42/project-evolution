namespace ProjectEvolution.Game;

public class SimulationRunner
{
    public static void RunInteractiveTuning()
    {
        Console.Clear();
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║           PROJECT EVOLUTION - FUN KNOB TUNER 🎛️                ║");
        Console.WriteLine("║              Interactive Automated Testing                     ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝\n");

        var config = new SimulationConfig();
        bool running = true;

        while (running)
        {
            DisplayCurrentConfig(config);

            Console.WriteLine("\n🎛️  TUNE THE FUN:");
            Console.WriteLine("[1] Mob Detection Range    [2] Max Mobs         [3] Min Mobs");
            Console.WriteLine("[4] Player Start HP        [5] Player Strength  [6] Player Defense");
            Console.WriteLine("[7] Encounter Rate (%)     [8] Simulation Speed (ms)");
            Console.WriteLine("[V] Toggle Visuals         [R] Run Simulation   [Q] Quit");
            Console.Write("\nChoice: ");

            var key = Console.ReadKey(intercept: true).Key;
            Console.WriteLine();

            switch (key)
            {
                case ConsoleKey.D1:
                    config.MobDetectionRange = PromptInt("Mob Detection Range (tiles)", config.MobDetectionRange, 1, 10);
                    break;
                case ConsoleKey.D2:
                    config.MaxMobs = PromptInt("Max Mobs", config.MaxMobs, 5, 50);
                    break;
                case ConsoleKey.D3:
                    config.MinMobs = PromptInt("Min Mobs", config.MinMobs, 0, config.MaxMobs);
                    break;
                case ConsoleKey.D4:
                    config.PlayerStartHP = PromptInt("Player Start HP", config.PlayerStartHP, 5, 50);
                    break;
                case ConsoleKey.D5:
                    config.PlayerStrength = PromptInt("Player Strength", config.PlayerStrength, 1, 10);
                    break;
                case ConsoleKey.D6:
                    config.PlayerDefense = PromptInt("Player Defense", config.PlayerDefense, 0, 10);
                    break;
                case ConsoleKey.D7:
                    config.EncounterRateMultiplier = PromptInt("Encounter Rate %", config.EncounterRateMultiplier, 0, 500);
                    break;
                case ConsoleKey.D8:
                    config.SimulationSpeed = PromptInt("Simulation Speed (ms)", config.SimulationSpeed, 0, 1000);
                    break;
                case ConsoleKey.V:
                    config.ShowVisuals = !config.ShowVisuals;
                    Console.WriteLine($"Visuals: {(config.ShowVisuals ? "ON" : "OFF")}");
                    Thread.Sleep(500);
                    break;
                case ConsoleKey.R:
                    RunSimulationBatch(config);
                    break;
                case ConsoleKey.Q:
                    running = false;
                    break;
            }

            Console.Clear();
        }
    }

    private static void DisplayCurrentConfig(SimulationConfig config)
    {
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║                    CURRENT SETTINGS                            ║");
        Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
        Console.WriteLine($"║ Mob Detection Range:      {config.MobDetectionRange,3} tiles                            ║");
        Console.WriteLine($"║ Max Mobs:                 {config.MaxMobs,3}                                  ║");
        Console.WriteLine($"║ Min Mobs:                 {config.MinMobs,3}                                  ║");
        Console.WriteLine($"║ Player Start HP:          {config.PlayerStartHP,3}                                  ║");
        Console.WriteLine($"║ Player Strength:          {config.PlayerStrength,3}                                  ║");
        Console.WriteLine($"║ Player Defense:           {config.PlayerDefense,3}                                  ║");
        Console.WriteLine($"║ Encounter Rate:           {config.EncounterRateMultiplier,3}%                                 ║");
        Console.WriteLine($"║ Simulation Speed:         {config.SimulationSpeed,3}ms                                 ║");
        Console.WriteLine($"║ Show Visuals:             {(config.ShowVisuals ? "ON " : "OFF")}                                 ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
    }

    private static int PromptInt(string label, int current, int min, int max)
    {
        Console.Write($"\n{label} (current: {current}, range: {min}-{max}): ");
        string input = Console.ReadLine() ?? "";
        if (int.TryParse(input, out int value))
        {
            return Math.Clamp(value, min, max);
        }
        return current;
    }

    private static void RunSimulationBatch(SimulationConfig config)
    {
        Console.Write("\nHow many runs? (1-100): ");
        string input = Console.ReadLine() ?? "10";
        int runs = int.TryParse(input, out int r) ? Math.Clamp(r, 1, 100) : 10;

        Console.Clear();
        Console.WriteLine($"🎮 Running {runs} simulations with current settings...\n");

        var simulator = new GameSimulator(config);
        var stats = simulator.RunSimulation(runs);

        stats.Display();

        Console.WriteLine("\n📊 ANALYSIS:");
        if (stats.AverageTurnsPerRun < 20)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine("⚠️  TOO HARD: Players dying very quickly!");
            Console.WriteLine("   → Reduce mob detection range");
            Console.WriteLine("   → Reduce max mobs");
            Console.WriteLine("   → Increase player HP or defense");
        }
        else if (stats.AverageTurnsPerRun > 200)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("⚠️  TOO EASY: Players surviving too long!");
            Console.WriteLine("   → Increase mob detection range");
            Console.WriteLine("   → Increase max mobs");
            Console.WriteLine("   → Reduce player stats");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("✅ BALANCED: Good challenge level!");
            Console.WriteLine($"   Average survival: {stats.AverageTurnsPerRun:F0} turns");
        }
        Console.ResetColor();

        Console.WriteLine("\nPress any key to continue...");
        Console.ReadKey(intercept: true);
    }
}
