using ProjectEvolution.Game;

// Main menu
Console.Clear();
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              PROJECT EVOLUTION - GENERATION 32                 ║");
Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║  [P] Play Game (Normal Mode)                                   ║");
Console.WriteLine("║  [T] Automated Testing & Tuning (Simulation Mode)              ║");
Console.WriteLine("║  [Q] Quit                                                      ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.Write("\nChoice: ");

var menuChoice = Console.ReadKey(intercept: true).Key;
Console.Clear();

if (menuChoice == ConsoleKey.T)
{
    // Run simulation/tuning mode
    SimulationRunner.RunInteractiveTuning();
    return;
}
else if (menuChoice == ConsoleKey.Q)
{
    return;
}

// Normal play mode
var ui = new UIRenderer();
var game = new RPGGame();
var logger = new GameLogger();
var _random = new Random();

logger.LogEvent("GAME", "Project Evolution started!");
game.SetPlayerStats(strength: 2, defense: 1);
game.StartWorldExploration();
logger.LogEvent("INIT", $"Player spawned at ({game.PlayerX},{game.PlayerY})");

ui.Initialize();
ui.RenderStatusBar(game);
ui.RenderMap(game);
ui.AddMessage("Welcome to Project Evolution!");
ui.AddMessage("Explore the world, fight monsters, find treasure!");
ui.AddMessage($"Towns at (5,5) and (15,15) | Dungeons at (10,5) and (10,15)");
ui.RenderCommandBar(game.InDungeon);

bool playing = true;

while (playing)
{
    // Position cursor for input
    Console.SetCursorPosition(2, 29);
    Console.Write("> " + new string(' ', 74));
    Console.SetCursorPosition(4, 29);

    var input = Console.ReadKey(intercept: true);
    var key = input.Key;

    if (key == ConsoleKey.Q)
    {
        playing = false;
        continue;
    }

    // World movement
    if (!game.InDungeon)
    {
        bool moved = false;
        string direction = "";

        if (key == ConsoleKey.N || key == ConsoleKey.UpArrow)
        {
            moved = game.MoveNorth();
            direction = "North";
        }
        else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
        {
            moved = game.MoveSouth();
            direction = "South";
        }
        else if (key == ConsoleKey.E || key == ConsoleKey.RightArrow)
        {
            moved = game.MoveEast();
            direction = "East";
        }
        else if (key == ConsoleKey.W || key == ConsoleKey.LeftArrow)
        {
            moved = game.MoveWest();
            direction = "West";
        }
        else if (key == ConsoleKey.Enter)
        {
            var terrain = game.GetCurrentTerrain();
            if (terrain == "Town")
            {
                ui.AddMessage("🏘️  Entered Town!");
                ui.AddMessage("[I]nn (10g heal) | [B]uy Potion (5g) | [X]it");
                Thread.Sleep(400); // Brief pause to see options

                var townKey = Console.ReadKey(intercept: true).Key;
                if (townKey == ConsoleKey.I)
                {
                    if (game.VisitInn())
                    {
                        ui.AddMessage("✅ Rested at Inn - Fully healed! -10g");
                        ui.RenderStatusBar(game);
                        Thread.Sleep(700); // Ahhh, refreshing!
                    }
                    else ui.AddMessage("❌ Not enough gold (need 10g)");
                }
                else if (townKey == ConsoleKey.B)
                {
                    if (game.BuyPotion())
                    {
                        ui.AddMessage("✅ Bought healing potion! -5g");
                        ui.RenderStatusBar(game);
                        Thread.Sleep(600); // Potion acquired!
                    }
                    else ui.AddMessage("❌ Not enough gold (need 5g)");
                }
            }
            else if (terrain == "Dungeon")
            {
                game.EnterDungeon();
                ui.AddMessage("⚔️  Entered Dungeon! Depth 1 - Danger awaits!");
                ui.RenderStatusBar(game);
                ui.RenderMap(game); // Show the dungeon map!
                ui.RenderCommandBar(true);
                Thread.Sleep(600); // Ominous pause before adventure
            }
            else
            {
                ui.AddMessage($"Cannot enter {terrain}");
            }
        }
        else if (key == ConsoleKey.P)
        {
            if (game.UsePotion())
            {
                ui.AddMessage("🧪 Used potion! +5 HP");
                ui.RenderStatusBar(game);
                Thread.Sleep(600); // Potion effect!
            }
            else ui.AddMessage("No potions!");
        }

        if (moved)
        {
            string terrain = game.GetCurrentTerrain();
            int turnCost = terrain switch
            {
                "Forest" => 2,
                "Mountain" => 3,
                _ => 1
            };

            string moveMsg = turnCost > 1
                ? $"Moved {direction} to {terrain} (-{turnCost} turns, difficult terrain!)"
                : $"Moved {direction} to {terrain}";

            logger.LogEvent("MOVE", $"{direction} to ({game.PlayerX},{game.PlayerY}) - {terrain} (-{turnCost} turns)");
            ui.AddMessage(moveMsg);
            ui.RenderStatusBar(game);
            ui.RenderMap(game);

            // Check for mob encounter first (visible mobs on map)
            var mob = game.GetMobAt(game.PlayerX, game.PlayerY);
            bool hasEncounter = false;

            if (mob != null)
            {
                // Walked into a visible mob!
                logger.LogEvent("ENCOUNTER", $"Mob encounter: {mob.Name} at ({game.PlayerX},{game.PlayerY})");
                ui.AddMessage($"⚔️  Encountered {mob.Name}!");
                Thread.Sleep(800);
                game.TriggerMobEncounter(mob);
                hasEncounter = true;
            }
            else if (game.RollForEncounter())
            {
                // Random encounter (less common now that we have visible mobs)
                logger.LogEvent("ENCOUNTER", $"Random encounter at ({game.PlayerX},{game.PlayerY})");
                ui.AddMessage("💥 AMBUSH! Enemy appeared!");
                Thread.Sleep(800);
                game.TriggerEncounter();
                hasEncounter = true;
            }

            if (hasEncounter)
            {
                logger.LogEvent("COMBAT", $"Fighting {game.EnemyName} [Lvl{game.EnemyLevel}] HP:{game.EnemyHP}");
                ui.RenderStatusBar(game);
                Thread.Sleep(400); // Brief pause to see enemy stats

                // Combat
                while (!game.CombatEnded && playing)
                {
                    ui.RenderCombat(game);

                    var combatKey = Console.ReadKey(intercept: true).Key;

                    if (combatKey == ConsoleKey.P && game.UsePotion())
                    {
                        ui.AddMessage("🧪 Potion! +5 HP");
                        ui.RenderStatusBar(game);
                        ui.RenderCombat(game);
                        Thread.Sleep(600); // Let player see the healing
                        continue;
                    }

                    if (combatKey == ConsoleKey.F)
                    {
                        bool fled = game.AttemptFlee();
                        logger.LogEvent("FLEE", fled ? "Successfully fled!" : "Failed to flee, took damage");
                        ui.AddMessage(game.CombatLog);
                        ui.RenderStatusBar(game);
                        Thread.Sleep(fled ? 600 : 800); // Longer pause if failed (took damage)
                        if (fled)
                        {
                            ui.RenderMap(game); // Return to map view
                            break;
                        }
                        if (game.PlayerHP <= 0)
                        {
                            ui.AddMessage("💀 DIED WHILE FLEEING!");
                            Thread.Sleep(1200); // Dramatic pause for death
                            playing = false;
                            break;
                        }
                        continue;
                    }

                    var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                    int hpBefore = game.PlayerHP;
                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    int hpAfter = game.PlayerHP;

                    if (hpAfter < hpBefore)
                    {
                        logger.LogEvent("DAMAGE", $"Player took {hpBefore - hpAfter} damage. HP: {hpAfter}/{game.MaxPlayerHP}");
                    }

                    logger.LogEvent("COMBAT_ROUND", game.CombatLog);
                    ui.AddMessage(game.CombatLog);
                    Thread.Sleep(900); // Pause to read combat results

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (game.IsWon)
                        {
                            logger.LogEvent("VICTORY", $"Defeated {game.EnemyName}. XP: {game.PlayerXP}");
                            ui.AddMessage("✅ Victory!");

                            // Remove defeated mob from map if it was a mob encounter
                            if (mob != null)
                            {
                                game.RemoveMob(mob);
                                logger.LogEvent("MOB", $"Removed {mob.Name} from map");
                            }

                            ui.RenderStatusBar(game);
                            Thread.Sleep(1000); // Celebrate victory!
                            ui.RenderMap(game); // Return to map view after combat
                        }
                        else
                        {
                            logger.LogEvent("DEATH", $"Killed by {game.EnemyName}. HP: 0");
                            ui.AddMessage("💀 YOU DIED! GAME OVER!");
                            ui.AddMessage($"Killed by: {game.EnemyName} [Lvl {game.EnemyLevel}]");
                            Thread.Sleep(1500); // Dramatic pause for death
                            playing = false;
                        }
                    }
                    else
                    {
                        ui.RenderStatusBar(game);
                    }
                }
            }
        }
    }
    else // In dungeon - use movement-based exploration!
    {
        bool dungeonMoved = false;
        string dungeonDirection = "";

        // Same movement keys work in dungeons
        if (key == ConsoleKey.N || key == ConsoleKey.UpArrow)
        {
            dungeonMoved = game.MoveNorth();
            dungeonDirection = "North";
        }
        else if (key == ConsoleKey.S || key == ConsoleKey.DownArrow)
        {
            dungeonMoved = game.MoveSouth();
            dungeonDirection = "South";
        }
        else if (key == ConsoleKey.E || key == ConsoleKey.RightArrow)
        {
            dungeonMoved = game.MoveEast();
            dungeonDirection = "East";
        }
        else if (key == ConsoleKey.W || key == ConsoleKey.LeftArrow)
        {
            dungeonMoved = game.MoveWest();
            dungeonDirection = "West";
        }
        else if (key == ConsoleKey.P)
        {
            if (game.UsePotion())
            {
                ui.AddMessage("🧪 Used potion! +5 HP");
                ui.RenderStatusBar(game);
                Thread.Sleep(600);
            }
            else ui.AddMessage("No potions!");
        }
        else if (key == ConsoleKey.X)
        {
            game.ExitDungeon();
            ui.AddMessage("Exited dungeon back to world!");
            ui.RenderStatusBar(game);
            ui.RenderMap(game);
            ui.RenderCommandBar(false);
            Thread.Sleep(500);
        }

        if (dungeonMoved)
        {
            ui.AddMessage($"Explored {dungeonDirection}");
            ui.RenderStatusBar(game);
            ui.RenderMap(game); // Update dungeon view

            // Check for encounters/events in dungeons
            int eventRoll = _random.Next(100);
            if (eventRoll < 30) // 30% chance for something to happen
            {
                int encounterType = _random.Next(3);
                if (encounterType == 0) // Monster
                {
                    game.TriggerDungeonCombat();
                    ui.AddMessage($"👹 {game.EnemyName} [Lvl{game.EnemyLevel}] blocks your path!");
                    Thread.Sleep(800);

                    while (!game.CombatEnded && playing)
                    {
                        ui.RenderCombat(game);
                        var combatKey = Console.ReadKey(intercept: true).Key;

                        if (combatKey == ConsoleKey.P && game.UsePotion())
                        {
                            ui.AddMessage("🧪 +5 HP");
                            ui.RenderStatusBar(game);
                            Thread.Sleep(600);
                            continue;
                        }

                        if (combatKey == ConsoleKey.F)
                        {
                            bool fled = game.AttemptFlee();
                            ui.AddMessage(game.CombatLog);
                            ui.RenderStatusBar(game);
                            Thread.Sleep(fled ? 600 : 800);
                            if (fled)
                            {
                                ui.RenderMap(game);
                                break;
                            }
                            if (game.PlayerHP <= 0)
                            {
                                ui.AddMessage("💀 GAME OVER!");
                                Thread.Sleep(1200);
                                playing = false;
                                break;
                            }
                            continue;
                        }

                        var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                        game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                        ui.AddMessage(game.CombatLog);
                        Thread.Sleep(900);

                        if (game.CombatEnded)
                        {
                            game.ProcessGameLoopVictory();
                            if (game.IsWon)
                            {
                                ui.AddMessage("✅ Victory!");
                                ui.RenderStatusBar(game);
                                Thread.Sleep(1000);
                                ui.RenderMap(game);
                            }
                            else
                            {
                                ui.AddMessage("💀 GAME OVER!");
                                Thread.Sleep(1500);
                                playing = false;
                            }
                        }
                        ui.RenderStatusBar(game);
                    }
                }
                else if (encounterType == 1) // Treasure
                {
                    int gold = game.RollForTreasure(game.DungeonDepth);
                    ui.AddMessage($"💎 TREASURE! Found {gold} gold!");
                    ui.RenderStatusBar(game);
                    Thread.Sleep(800);
                }
                else // Trap
                {
                    int dmg = game.TriggerTrap();
                    ui.AddMessage($"💥 TRAP! Took {dmg} damage!");
                    ui.RenderStatusBar(game);
                    Thread.Sleep(900);
                }
            }
        }
        else if (key == ConsoleKey.N || key == ConsoleKey.S || key == ConsoleKey.E || key == ConsoleKey.W ||
                 key == ConsoleKey.UpArrow || key == ConsoleKey.DownArrow ||
                 key == ConsoleKey.LeftArrow || key == ConsoleKey.RightArrow)
        {
            // Tried to move but hit a wall
            ui.AddMessage("🧱 Wall blocks your path!");
            Thread.Sleep(300);
        }
    }
}

ui.Cleanup();

logger.LogEvent("GAME", "Game session ended");
Console.WriteLine("\n╔════════════════════════════════════════╗");
Console.WriteLine("║      GAME ENDED                        ║");
Console.WriteLine("╚════════════════════════════════════════╝");

// Determine why game ended
string endReason = "Unknown";
if (game.PlayerHP <= 0)
    endReason = "Player Death (HP reached 0)";
else if (!playing && game.RunEnded)
    endReason = "Player died in combat";
else if (!playing)
    endReason = "Player quit";

Console.WriteLine($"\nEnd Reason: {endReason}");
Console.WriteLine($"Final Stats: Level {game.PlayerLevel} | {game.PlayerGold}g | {game.CombatsWon} victories");

// Show recent events FIRST (before they get buried by dump)
logger.ShowRecentEvents(20, excludeDumps: true);

// Then full state dump
logger.DumpGameState(game, endReason);

Console.WriteLine("\nLog saved to: game_log.txt");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
