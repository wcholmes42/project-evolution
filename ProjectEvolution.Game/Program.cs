using ProjectEvolution.Game;

// Main menu
Console.Clear();
Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
Console.WriteLine("║              PROJECT EVOLUTION - GENERATION 35                 ║");
Console.WriteLine("║                \"Skills & Abilities Update\"                     ║");
Console.WriteLine("╠════════════════════════════════════════════════════════════════╣");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║  🎮 [P] PLAY GAME                                              ║");
Console.WriteLine("║      Experience the evolved RPG with skills & progression      ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║  🧬 [R] PROGRESSION FRAMEWORK RESEARCH                         ║");
Console.WriteLine("║      Continuous evolution of game balance & formulas           ║");
Console.WriteLine("║      → Tests combat, economy, equipment, skills, builds        ║");
Console.WriteLine("║      → Auto-generates balanced code & progression curves       ║");
Console.WriteLine("║      → Adaptive mutation finds optimal parameters              ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("║  [Q] Quit                                                      ║");
Console.WriteLine("║                                                                ║");
Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
Console.Write("\nChoice: ");

var menuChoice = Console.ReadKey(intercept: true).Key;
Console.Clear();

if (menuChoice == ConsoleKey.R)
{
    // Progression Framework Research - encompasses all tuning!
    ProgressionFrameworkResearcher.RunContinuousResearch();
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

// CRITICAL: Load and apply AI-tuned values BEFORE starting!
var optimalConfig = ConfigPersistence.LoadOptimalConfig();
if (optimalConfig != null)
{
    // Apply AI-tuned values to the game!
    game.SetOptimalConfig(optimalConfig);

    // Override stats with tuned values
    game.SetPlayerStats(optimalConfig.PlayerStrength, optimalConfig.PlayerDefense);

    logger.LogEvent("CONFIG", $"🤖 AI-TUNED: HP={optimalConfig.PlayerStartHP}, Det={optimalConfig.MobDetectionRange}, Mobs={optimalConfig.MaxMobs}, STR={optimalConfig.PlayerStrength}, DEF={optimalConfig.PlayerDefense}");
}
else
{
    // TUTORIAL MODE defaults - Balanced for learning!
    game.SetPlayerStats(strength: 5, defense: 3);
    // Note: Starting HP is 100 in MaxPlayerHP, but we'll keep it for tutorial ease
}

// Start world BEFORE UI (need world to render!)
game.StartWorldExploration();

// Give starting potions and gold!
game.SetGoldForTesting(50);
game.BuyPotion();
game.BuyPotion();
game.BuyPotion(); // Start with 3 potions!
logger.LogEvent("INIT", $"Player spawned at ({game.PlayerX},{game.PlayerY})");

// NOW initialize UI after world exists
ui.Initialize();
ui.RenderStatusBar(game);
ui.RenderMap(game);
ui.AddMessage("Welcome to Project Evolution! Generation 35: Skills & Abilities!");
ui.AddMessage("† You're at the Temple - FREE healing & respawn point!");
ui.AddMessage("Death is not the end! You'll respawn here (lose 50% gold & gear)");
ui.AddMessage($"Towns at (5,5) & (15,15) | Dungeons at (10,5) & (10,15) | Press [H] for help");
ui.RenderCommandBar(game.InDungeon);

bool playing = true;

// Helper: Handle death and respawn
void HandleDeath(string killerName)
{
    // Track dropped items for UI
    var droppedItems = new List<string>();
    if (game.PlayerInventory.EquippedWeapon.BonusStrength > 0)
        droppedItems.Add(game.PlayerInventory.EquippedWeapon.Name);
    if (game.PlayerInventory.EquippedArmor.BonusDefense > 0)
        droppedItems.Add(game.PlayerInventory.EquippedArmor.Name);

    int goldBeforeDeath = game.PlayerGold;

    // Handle death (drops equipment, loses gold, respawns)
    game.HandlePlayerDeath();

    int goldLost = goldBeforeDeath - game.PlayerGold;

    // Show death screen
    ui.RenderDeathScreen(game, killerName, goldLost, droppedItems);
    Console.ReadKey(intercept: true);

    logger.LogEvent("DEATH", $"Killed by {killerName}. Respawned at Temple. Deaths: {game.TotalDeaths}");

    // Redraw game UI at temple
    ui.Initialize();
    ui.RenderStatusBar(game);
    ui.RenderMap(game);
    ui.AddMessage($"💀 You died! Respawned at Temple. Lost {goldLost}g.");
    if (droppedItems.Count > 0)
        ui.AddMessage($"💀 Return to your corpse to retrieve: {string.Join(", ", droppedItems)}");
    ui.RenderCommandBar(false);
}

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

    // NEW: Help Menu [H]
    if (key == ConsoleKey.H)
    {
        ui.RenderHelpMenu(game.InDungeon);
        Console.ReadKey(intercept: true);
        // Redraw game UI
        ui.Initialize();
        ui.RenderStatusBar(game);
        ui.RenderMap(game);
        ui.RenderCommandBar(game.InDungeon);
        continue;
    }

    // NEW: Character Sheet [I]
    if (key == ConsoleKey.I)
    {
        ui.RenderFullCharacterSheet(game);
        Console.ReadKey(intercept: true);
        // Redraw game UI
        ui.Initialize();
        ui.RenderStatusBar(game);
        ui.RenderMap(game);
        ui.RenderCommandBar(game.InDungeon);
        continue;
    }

    // NEW: Stat Allocation [L]
    if (key == ConsoleKey.L && game.AvailableStatPoints > 0)
    {
        // Enter stat allocation mode
        while (game.AvailableStatPoints > 0)
        {
            ui.RenderStatAllocationScreen(game);
            var statKey = Console.ReadKey(intercept: true).Key;

            if (statKey == ConsoleKey.S)
            {
                game.SpendStatPoint(StatType.Strength);
            }
            else if (statKey == ConsoleKey.D)
            {
                game.SpendStatPoint(StatType.Defense);
            }
            else if (statKey == ConsoleKey.Q)
            {
                break; // Exit allocation even if points remain
            }
        }

        // Redraw game UI
        ui.Initialize();
        ui.RenderStatusBar(game);
        ui.RenderMap(game);
        ui.RenderCommandBar(game.InDungeon);
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

            // NEW: Temple interaction
            if (terrain == "Temple")
            {
                ui.AddMessage("† Entered Temple of Respawn †");
                ui.AddMessage("[P]ray for blessing (FREE heal) | [R]etrieve corpse items | [X]it");
                Thread.Sleep(400);

                var templeKey = Console.ReadKey(intercept: true).Key;
                if (templeKey == ConsoleKey.P)
                {
                    // Free heal at temple
                    game.SetHPForTesting(game.MaxPlayerHP);
                    ui.AddMessage("✨ The gods have blessed you! Fully healed!");
                    ui.RenderStatusBar(game);
                    Thread.Sleep(800);
                }
                else if (templeKey == ConsoleKey.R)
                {
                    // Auto-retrieve if standing on corpse
                    if (game.CanRetrieveDroppedItems())
                    {
                        string msg = game.RetrieveDroppedItems();
                        ui.AddMessage($"✅ {msg}");
                        ui.RenderStatusBar(game);
                        Thread.Sleep(800);
                    }
                    else
                    {
                        ui.AddMessage("❌ No corpse here. Look for 💀 on map.");
                    }
                }
            }
            else if (terrain == "Town")
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
            // LIVING WORLD: Mobs move after player
            game.UpdateWorldMobs();

            // AUTO-RETRIEVE: Check if we're on our corpse
            if (game.CanRetrieveDroppedItems())
            {
                string retrieved = game.RetrieveDroppedItems();
                ui.AddMessage($"💀 {retrieved}");
                Thread.Sleep(800);
            }

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
            ui.RenderMap(game); // Will show mobs in their new positions!

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
                // Random encounter - SPAWN VISIBLE MOB!
                logger.LogEvent("ENCOUNTER", $"Random encounter at ({game.PlayerX},{game.PlayerY})");

                // Spawn mob at adjacent tile
                int spawnX = game.PlayerX + _random.Next(-1, 2);
                int spawnY = game.PlayerY + _random.Next(-1, 2);
                spawnX = Math.Clamp(spawnX, 0, game.WorldWidth - 1);
                spawnY = Math.Clamp(spawnY, 0, game.WorldHeight - 1);

                var encounterMob = new Mob(
                    spawnX, spawnY,
                    "Ambusher",
                    Math.Max(1, game.PlayerLevel + _random.Next(-1, 2)),
                    (EnemyType)_random.Next(3)
                );

                game.AddMobForTesting(encounterMob);

                ui.AddMessage($"💥 {encounterMob.Name} [Lvl{encounterMob.Level}] appears!");
                ui.RenderMap(game); // Show mob appearing!
                Thread.Sleep(800);

                // Combat happens next move if they walk into it
                hasEncounter = false; // Don't trigger combat immediately
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
                            HandleDeath(game.EnemyName);
                            break;
                        }
                        continue;
                    }

                    // GENERATION 35: Skills Menu
                    if (combatKey == ConsoleKey.S)
                    {
                        ui.RenderSkillsMenu(game);
                        var skillKey = Console.ReadKey(intercept: true).Key;

                        if (skillKey == ConsoleKey.Escape)
                        {
                            // Cancel - redraw combat
                            ui.RenderStatusBar(game);
                            ui.RenderCombat(game);
                            continue;
                        }

                        // Try to use skill 1-5
                        int skillIndex = skillKey switch
                        {
                            ConsoleKey.D1 or ConsoleKey.NumPad1 => 0,
                            ConsoleKey.D2 or ConsoleKey.NumPad2 => 1,
                            ConsoleKey.D3 or ConsoleKey.NumPad3 => 2,
                            ConsoleKey.D4 or ConsoleKey.NumPad4 => 3,
                            ConsoleKey.D5 or ConsoleKey.NumPad5 => 4,
                            _ => -1
                        };

                        if (skillIndex >= 0)
                        {
                            var skills = game.GetAvailableSkills();
                            if (skillIndex < skills.Count)
                            {
                                var skill = skills[skillIndex];
                                game.QueueSkillForNextRound(skill);
                                // Use skill action
                                game.ExecuteGameLoopRoundWithRandomHits(CombatAction.UseSkill, CombatAction.Attack);
                                logger.LogEvent("SKILL", $"Used {skill.Name}");
                            }
                            else
                            {
                                // Invalid skill - redraw combat
                                ui.RenderStatusBar(game);
                                ui.RenderCombat(game);
                                continue;
                            }
                        }
                        else
                        {
                            // Invalid key - redraw combat
                            ui.RenderStatusBar(game);
                            ui.RenderCombat(game);
                            continue;
                        }
                    }
                    else
                    {
                        var action = combatKey == ConsoleKey.A ? CombatAction.Attack : CombatAction.Defend;
                        game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    }

                    // Common combat result handling
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
                            HandleDeath($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
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
            // Mark explored tiles (fog of war)
            game.MarkDungeonTileExplored(game.PlayerX, game.PlayerY);

            // Move dungeon mobs after player moves!
            game.MoveDungeonMobs();

            ui.AddMessage($"Explored {dungeonDirection}");
            ui.RenderStatusBar(game);
            ui.RenderMap(game); // Update dungeon view

            // CHECK FOR DUNGEON MOB COLLISION FIRST!
            var dungeonMob = game.GetDungeonMobAt(game.PlayerX, game.PlayerY);
            if (dungeonMob != null)
            {
                // Walked into a dungeon mob!
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
                            HandleDeath(game.EnemyName);
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
                            // Remove defeated dungeon mob!
                            game.RemoveDungeonMob(dungeonMob);
                            ui.RenderStatusBar(game);
                            Thread.Sleep(1000);
                            ui.RenderMap(game);
                        }
                        else
                        {
                            HandleDeath($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
                        }
                    }
                    ui.RenderStatusBar(game);
                }

                // Skip tile checking if we fought a mob
                continue;
            }

            // CHECK WHAT TILE WE'RE ON!
            string currentTile = game.GetDungeonTile(game.PlayerX, game.PlayerY);

            if (currentTile == "Treasure")
            {
                // Stepped on treasure!
                int gold = game.RollForTreasure(game.DungeonDepth);
                ui.AddMessage($"💎 TREASURE CHEST! Found {gold} gold!");
                ui.RenderStatusBar(game);
                Thread.Sleep(800);

                // Remove treasure from map
                game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
            }
            else if (currentTile == "Trap")
            {
                // Stepped on trap!
                int dmg = game.TriggerTrap();
                ui.AddMessage($"💥 TRAP! Took {dmg} damage!");
                ui.RenderStatusBar(game);
                Thread.Sleep(900);

                // Remove trap from map
                game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");

                if (game.PlayerHP <= 0)
                {
                    HandleDeath("Trap");
                }
            }
            else if (currentTile == "Boss")
            {
                // Boss fight!
                game.TriggerBossCombat();
                ui.AddMessage(game.CombatLog);
                Thread.Sleep(1200); // Dramatic pause for boss entrance

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
                            HandleDeath(game.EnemyName);
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
                            ui.AddMessage("🏆 BOSS DEFEATED!");
                            game.MarkBossDefeated(); // Spawn artifact and portal!
                            game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
                            ui.RenderStatusBar(game);
                            ui.RenderMap(game);
                            Thread.Sleep(1500);
                        }
                        else
                        {
                            HandleDeath($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
                        }
                    }
                    ui.RenderStatusBar(game);
                }
            }
            else if (currentTile == "Artifact")
            {
                // Collect artifact!
                string msg = game.CollectArtifact();
                ui.AddMessage(msg);
                ui.RenderStatusBar(game);
                Thread.Sleep(1200);
                game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
                ui.RenderMap(game);
            }
            else if (currentTile == "Portal")
            {
                // Use portal to complete dungeon
                string msg = game.UseDungeonPortal();
                ui.AddMessage(msg);

                // Check for victory!
                if (game.RunWon)
                {
                    ui.AddMessage("🎉🎉🎉 VICTORY! YOU WON THE GAME! 🎉🎉🎉");
                    ui.AddMessage(game.GetVictoryProgress());
                    Thread.Sleep(3000);
                    playing = false;
                }

                // Clear and re-render entire UI to avoid dungeon/world map blending
                ui.Initialize();
                ui.RenderStatusBar(game);
                ui.RenderMap(game);
                ui.RenderCommandBar(false);
                Thread.Sleep(1000);
            }
            else if (currentTile == "Monster")
            {
                // Stepped on monster!
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
                                // Remove defeated monster from map!
                                game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
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
            }
            else if (currentTile == "Stairs")
            {
                ui.AddMessage("🚪 Found stairs! Press [D] to descend or keep exploring");
            }
        }
        else if (key == ConsoleKey.D && game.InDungeon)
        {
            // Check if on stairs
            if (game.GetDungeonTile(game.PlayerX, game.PlayerY) == "Stairs")
            {
                game.DescendDungeon();
                ui.AddMessage($"⬇️  Descended to Depth {game.DungeonDepth}!");
                ui.RenderStatusBar(game);
                ui.RenderMap(game); // Show new floor!
                Thread.Sleep(600);
            }
            else
            {
                ui.AddMessage("No stairs here - find the > symbol!");
            }
            // else: just regular floor, nothing happens
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
