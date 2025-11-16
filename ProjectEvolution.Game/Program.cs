using ProjectEvolution.Game;

var ui = new UIRenderer();
var game = new RPGGame();

game.SetPlayerStats(strength: 2, defense: 1);
game.StartWorldExploration();

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

                var townKey = Console.ReadKey(intercept: true).Key;
                if (townKey == ConsoleKey.I)
                {
                    if (game.VisitInn())
                    {
                        ui.AddMessage("✅ Rested at Inn - Fully healed! -10g");
                        ui.RenderStatusBar(game);
                    }
                    else ui.AddMessage("❌ Not enough gold (need 10g)");
                }
                else if (townKey == ConsoleKey.B)
                {
                    if (game.BuyPotion())
                    {
                        ui.AddMessage("✅ Bought healing potion! -5g");
                        ui.RenderStatusBar(game);
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
            }
            else ui.AddMessage("No potions!");
        }

        if (moved)
        {
            ui.AddMessage($"Moved {direction} to {game.GetCurrentTerrain()}");
            ui.RenderStatusBar(game);
            ui.RenderMap(game);

            // Check for encounter
            if (game.RollForEncounter())
            {
                ui.AddMessage("💥 AMBUSH! Enemy encountered!");
                game.TriggerEncounter();
                ui.RenderStatusBar(game);

                // Combat
                while (!game.CombatEnded && playing)
                {
                    ui.AddMessage($"⚔️  {game.EnemyName} [Lvl{game.EnemyLevel}] {game.EnemyHP}HP");
                    ui.AddMessage("[A]ttack [D]efend [P]otion");

                    var combatKey = Console.ReadKey(intercept: true).Key;

                    if (combatKey == ConsoleKey.P && game.UsePotion())
                    {
                        ui.AddMessage("🧪 Potion! +5 HP");
                        ui.RenderStatusBar(game);
                        continue;
                    }

                    var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    ui.AddMessage(game.CombatLog);

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (game.IsWon)
                        {
                            ui.AddMessage("✅ Victory!");
                            ui.RenderStatusBar(game);
                        }
                        else
                        {
                            ui.AddMessage("💀 YOU DIED! GAME OVER!");
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

            if (roomType == "Monster")
            {
                game.TriggerDungeonCombat();
                ui.AddMessage($"👹 Monster appears! {game.EnemyName} [Lvl{game.EnemyLevel}]");

                while (!game.CombatEnded && playing)
                {
                    ui.AddMessage($"[A]ttack [D]efend [P]otion");
                    var combatKey = Console.ReadKey(intercept: true).Key;

                    if (combatKey == ConsoleKey.P && game.UsePotion())
                    {
                        ui.AddMessage("🧪 +5 HP");
                        ui.RenderStatusBar(game);
                        continue;
                    }

                    var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    ui.AddMessage(game.CombatLog);

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (!game.IsWon)
                        {
                            ui.AddMessage("💀 GAME OVER!");
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
            }
            else // Empty room - check for events
            {
                var eventType = game.RollForEvent();
                if (eventType == "Trap")
                {
                    int dmg = game.TriggerTrap();
                    ui.AddMessage($"💥 TRAP! Took {dmg} damage!");
                    ui.RenderStatusBar(game);
                }
                else if (eventType == "Discovery")
                {
                    var bonus = game.TriggerDiscovery();
                    ui.AddMessage($"✨ Discovery! {bonus}");
                    ui.RenderStatusBar(game);
                }
                else
                {
                    ui.AddMessage("Empty room. Nothing here.");
                }
            }
        }
        else if (key == ConsoleKey.D)
        {
            game.DescendDungeon();
            ui.AddMessage($"⬇️  Descended to Depth {game.DungeonDepth}!");
            ui.RenderStatusBar(game);
        }
        else if (key == ConsoleKey.X)
        {
            game.ExitDungeon();
            ui.AddMessage("Exited dungeon back to world!");
            ui.RenderStatusBar(game);
            ui.RenderMap(game);
            ui.RenderCommandBar(false);
        }
    }
}

ui.Cleanup();
Console.WriteLine("\n╔════════════════════════════════════════╗");
Console.WriteLine("║      THANKS FOR PLAYING!               ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine($"\nFinal Stats: Level {game.PlayerLevel} | {game.PlayerGold}g | {game.CombatsWon} victories");
Console.WriteLine("\nPress any key to exit...");
Console.ReadKey();
