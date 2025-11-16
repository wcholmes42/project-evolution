using ProjectEvolution.Game;

var ui = new UIRenderer();
var game = new RPGGame();
var logger = new GameLogger();

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
            logger.LogEvent("MOVE", $"{direction} to ({game.PlayerX},{game.PlayerY}) - {game.GetCurrentTerrain()}");
            ui.AddMessage($"Moved {direction} to {game.GetCurrentTerrain()}");
            ui.RenderStatusBar(game);
            ui.RenderMap(game);

            // Check for encounter
            if (game.RollForEncounter())
            {
                logger.LogEvent("ENCOUNTER", $"Random encounter at ({game.PlayerX},{game.PlayerY})");
                ui.AddMessage("💥 AMBUSH! Enemy encountered!");
                Thread.Sleep(800); // Pause to build tension
                game.TriggerEncounter();
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
    else // In dungeon
    {
        if (key == ConsoleKey.R)
        {
            var roomType = game.RollForRoom();
            ui.AddMessage($"🎲 Rolled: {roomType} Room");
            Thread.Sleep(500); // Suspenseful pause for roll result

            if (roomType == "Monster")
            {
                game.TriggerDungeonCombat();
                ui.AddMessage($"👹 Monster appears! {game.EnemyName} [Lvl{game.EnemyLevel}]");
                Thread.Sleep(800); // Build tension before combat starts

                while (!game.CombatEnded && playing)
                {
                    ui.RenderCombat(game);
                    var combatKey = Console.ReadKey(intercept: true).Key;

                    if (combatKey == ConsoleKey.P && game.UsePotion())
                    {
                        ui.AddMessage("🧪 +5 HP");
                        ui.RenderStatusBar(game);
                        Thread.Sleep(600); // Let player see the healing
                        continue;
                    }

                    if (combatKey == ConsoleKey.F)
                    {
                        bool fled = game.AttemptFlee();
                        ui.AddMessage(game.CombatLog);
                        ui.RenderStatusBar(game);
                        Thread.Sleep(fled ? 600 : 800);
                        if (fled) break;
                        if (game.PlayerHP <= 0)
                        {
                            ui.AddMessage("💀 DIED WHILE FLEEING!");
                            Thread.Sleep(1200);
                            playing = false;
                            break;
                        }
                        continue;
                    }

                    var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    ui.AddMessage(game.CombatLog);
                    Thread.Sleep(900); // Pause to read combat results

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (game.IsWon)
                        {
                            ui.AddMessage("✅ Victory!");
                            ui.RenderStatusBar(game);
                            Thread.Sleep(1000); // Celebrate victory!
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
            else if (roomType == "Treasure")
            {
                int gold = game.RollForTreasure(game.DungeonDepth);
                ui.AddMessage($"💎 TREASURE! Found {gold} gold!");
                ui.RenderStatusBar(game);
                Thread.Sleep(800); // Let player appreciate the loot!
            }
            else // Empty room - check for events
            {
                var eventType = game.RollForEvent();
                if (eventType == "Trap")
                {
                    int dmg = game.TriggerTrap();
                    ui.AddMessage($"💥 TRAP! Took {dmg} damage!");
                    ui.RenderStatusBar(game);
                    Thread.Sleep(900); // Ouch! Let that sink in
                }
                else if (eventType == "Discovery")
                {
                    var bonus = game.TriggerDiscovery();
                    ui.AddMessage($"✨ Discovery! {bonus}");
                    ui.RenderStatusBar(game);
                    Thread.Sleep(700); // Nice find!
                }
                else
                {
                    ui.AddMessage("Empty room. Nothing here.");
                    Thread.Sleep(400); // Brief pause
                }
            }
        }
        else if (key == ConsoleKey.D)
        {
            game.DescendDungeon();
            ui.AddMessage($"⬇️  Descended to Depth {game.DungeonDepth}!");
            ui.RenderStatusBar(game);
            Thread.Sleep(600); // Going deeper...
        }
        else if (key == ConsoleKey.X)
        {
            game.ExitDungeon();
            ui.AddMessage("Exited dungeon back to world!");
            ui.RenderStatusBar(game);
            ui.RenderMap(game);
            ui.RenderCommandBar(false);
            Thread.Sleep(500); // Return to surface
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
