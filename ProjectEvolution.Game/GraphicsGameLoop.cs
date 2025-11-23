using Raylib_CsLo;

namespace ProjectEvolution.Game;

/// <summary>
/// Main game loop for graphics mode using Raylib
/// </summary>
public static class GraphicsGameLoop
{
    public static void Run()
    {
        var renderer = new GraphicsRenderer();
        var game = new RPGGame();
        var logger = new GameLogger();
        var random = new Random();

        // Initialize graphics
        renderer.Initialize();

        logger.LogEvent("GAME", "Project Evolution started! (Graphics Mode)");

        // GENERATION 47: Create combat screen and death screen
        var combatScreen = new GraphicsCombatScreen(renderer.GetScreenWidth(), renderer.GetScreenHeight());
        var deathScreen = new GraphicsDeathScreen(renderer.GetScreenWidth(), renderer.GetScreenHeight());

        // Load AI-tuned config
        var optimalConfig = ConfigPersistence.LoadOptimalConfig();
        if (optimalConfig != null)
        {
            game.SetOptimalConfig(optimalConfig);
            game.SetPlayerStats(optimalConfig.PlayerStrength, optimalConfig.PlayerDefense);
            logger.LogEvent("CONFIG", $"🤖 AI-TUNED: HP={optimalConfig.PlayerStartHP}, Det={optimalConfig.MobDetectionRange}, Mobs={optimalConfig.MaxMobs}");
        }
        else
        {
            game.SetPlayerStats(strength: 5, defense: 3);
        }

        // Start world
        game.StartWorldExploration();

        // Give starting resources
        game.SetGoldForTesting(50);
        game.BuyPotion();
        game.BuyPotion();
        game.BuyPotion();
        logger.LogEvent("INIT", $"Player spawned at ({game.PlayerX},{game.PlayerY})");

        bool playing = true;

        // GENERATION 47: Combat loop with graphical UI
        void RunGraphicalCombat(Mob? mob)
        {
            combatScreen.Reset();
            combatScreen.LogMessage($"Battle started: {game.EnemyName} [Lvl {game.EnemyLevel}]");

            while (!game.CombatEnded && playing && !renderer.ShouldClose())
            {
                renderer.BeginFrame();

                // Render combat screen
                combatScreen.Render(game, renderer);

                renderer.EndFrame();

                // Handle combat input
                CombatAction? action = null;

                if (renderer.IsKeyPressed(KeyboardKey.KEY_A))
                    action = CombatAction.Attack;
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_D))
                    action = CombatAction.Defend;
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_P))
                {
                    if (game.UsePotion())
                    {
                        combatScreen.LogMessage("Used potion! +5 HP");
                        logger.LogEvent("COMBAT", "Used potion");
                    }
                    continue;
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_F))
                {
                    bool fled = game.AttemptFlee();
                    combatScreen.LogMessage(game.CombatLog);
                    logger.LogEvent("FLEE", fled ? "Fled successfully" : "Failed to flee");
                    if (fled) break;
                    continue;
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_S))
                {
                    // TODO: Skills menu (for now, just attack)
                    action = CombatAction.Attack;
                }

                if (action.HasValue)
                {
                    game.ExecuteGameLoopRoundWithRandomHits(action.Value, CombatAction.Attack);
                    combatScreen.LogMessage(game.CombatLog);
                    logger.LogEvent("COMBAT", game.CombatLog);
                }
            }

            // Victory or defeat
            if (game.IsWon)
            {
                game.ProcessGameLoopVictory();
                if (mob != null) game.RemoveMob(mob);
                combatScreen.LogMessage("⚔️ VICTORY! ⚔️");
                logger.LogEvent("VICTORY", $"Defeated {game.EnemyName}");

                // Show victory message for 2 seconds
                for (int i = 0; i < 120; i++)
                {
                    renderer.BeginFrame();
                    combatScreen.Render(game, renderer);
                    renderer.EndFrame();
                    System.Threading.Thread.Sleep(16); // ~60 FPS
                }
            }
        }

        // Handle death and respawn
        void HandleDeath(string killerName)
        {
            var droppedItems = new List<string>();
            if (game.PlayerInventory.EquippedWeapon.BonusStrength > 0)
                droppedItems.Add(game.PlayerInventory.EquippedWeapon.Name);
            if (game.PlayerInventory.EquippedArmor.BonusDefense > 0)
                droppedItems.Add(game.PlayerInventory.EquippedArmor.Name);

            int goldBeforeDeath = game.PlayerGold;
            int deathsBeforeRespawn = game.TotalDeaths;
            game.HandlePlayerDeath();
            int goldLost = goldBeforeDeath - game.PlayerGold;

            logger.LogEvent("DEATH", $"Killed by {killerName}. Respawned at Temple. Deaths: {game.TotalDeaths}");

            // GENERATION 47 EVOLUTION 2: Show dramatic death screen!
            deathScreen.ShowDeathSequence(game, killerName, goldLost, droppedItems);
        }

        // Main game loop
        while (playing && !renderer.ShouldClose())
        {
            renderer.BeginFrame();

            // DEBUG: Draw test rectangles DIRECTLY to verify rendering works
            Raylib.DrawRectangle(50, 50, 200, 200, Raylib.RED);
            Raylib.DrawRectangle(300, 50, 200, 200, Raylib.GREEN);
            Raylib.DrawRectangle(550, 50, 200, 200, Raylib.BLUE);
            Raylib.DrawText("TEST - Can you see these colored squares?", 50, 270, 20, Raylib.WHITE);

            // Render game
            renderer.DrawMap(game);

            renderer.EndFrame();

            // Handle input
            if (renderer.IsKeyPressed(KeyboardKey.KEY_ESCAPE))
            {
                playing = false;
            }

            // F12: Screenshot for debugging
            if (renderer.IsKeyPressed(KeyboardKey.KEY_F12))
            {
                renderer.TakeScreenshot();
            }

            // Movement
            if (!game.InDungeon)
            {
                bool moved = false;

                if (renderer.IsKeyPressed(KeyboardKey.KEY_UP) || renderer.IsKeyPressed(KeyboardKey.KEY_W))
                {
                    moved = game.MoveNorth();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_DOWN) || renderer.IsKeyPressed(KeyboardKey.KEY_S))
                {
                    moved = game.MoveSouth();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_RIGHT) || renderer.IsKeyPressed(KeyboardKey.KEY_D))
                {
                    moved = game.MoveEast();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_LEFT) || renderer.IsKeyPressed(KeyboardKey.KEY_A))
                {
                    moved = game.MoveWest();
                }

                if (moved)
                {
                    // Living world: mobs move after player
                    game.UpdateWorldMobs();

                    // Auto-retrieve corpse items
                    if (game.CanRetrieveDroppedItems())
                    {
                        string retrieved = game.RetrieveDroppedItems();
                        logger.LogEvent("RETRIEVE", retrieved);
                    }

                    string terrain = game.GetCurrentTerrain();
                    logger.LogEvent("MOVE", $"Moved to ({game.PlayerX},{game.PlayerY}) - {terrain}");

                    // Check for mob encounter
                    var mob = game.GetMobAt(game.PlayerX, game.PlayerY);
                    if (mob != null)
                    {
                        // Walked into a mob - trigger combat
                        logger.LogEvent("ENCOUNTER", $"Mob encounter: {mob.Name} at ({game.PlayerX},{game.PlayerY})");
                        game.TriggerMobEncounter(mob);

                        // GENERATION 47: Graphical combat screen!
                        RunGraphicalCombat(mob);

                        if (!game.IsWon && game.PlayerHP <= 0)
                        {
                            HandleDeath($"{mob.Name} [Lvl {mob.Level}]");
                        }
                    }
                    else if (game.RollForEncounter())
                    {
                        // Random encounter - spawn visible mob
                        int spawnX = game.PlayerX + random.Next(-1, 2);
                        int spawnY = game.PlayerY + random.Next(-1, 2);
                        spawnX = Math.Clamp(spawnX, 0, game.WorldWidth - 1);
                        spawnY = Math.Clamp(spawnY, 0, game.WorldHeight - 1);

                        var encounterMob = new Mob(
                            spawnX, spawnY,
                            "Ambusher",
                            Math.Max(1, game.PlayerLevel + random.Next(-1, 2)),
                            (EnemyType)random.Next(3)
                        );

                        game.AddMobForTesting(encounterMob);
                        logger.LogEvent("ENCOUNTER", $"Random encounter: {encounterMob.Name} appeared!");
                    }
                }

                // Interactions
                if (renderer.IsKeyPressed(KeyboardKey.KEY_ENTER) || renderer.IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    var terrain = game.GetCurrentTerrain();

                    if (terrain == "Temple")
                    {
                        // Free heal at temple
                        game.SetHPForTesting(game.MaxPlayerHP);
                        logger.LogEvent("TEMPLE", "Healed at Temple!");

                        // Retrieve corpse items if available
                        if (game.CanRetrieveDroppedItems())
                        {
                            string msg = game.RetrieveDroppedItems();
                            logger.LogEvent("RETRIEVE", msg);
                        }
                    }
                    else if (terrain == "Town")
                    {
                        // Auto-heal at inn (if enough gold)
                        if (game.VisitInn())
                        {
                            logger.LogEvent("TOWN", "Rested at Inn - Fully healed! -10g");
                        }
                    }
                    else if (terrain == "Dungeon")
                    {
                        game.EnterDungeon();
                        logger.LogEvent("DUNGEON", "Entered Dungeon!");
                    }
                }

                // Use potion
                if (renderer.IsKeyPressed(KeyboardKey.KEY_P))
                {
                    if (game.UsePotion())
                    {
                        logger.LogEvent("ITEM", "Used potion! +5 HP");
                    }
                }
            }
            else // In dungeon
            {
                bool dungeonMoved = false;

                if (renderer.IsKeyPressed(KeyboardKey.KEY_UP) || renderer.IsKeyPressed(KeyboardKey.KEY_W))
                {
                    dungeonMoved = game.MoveNorth();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_DOWN) || renderer.IsKeyPressed(KeyboardKey.KEY_S))
                {
                    dungeonMoved = game.MoveSouth();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_RIGHT) || renderer.IsKeyPressed(KeyboardKey.KEY_D))
                {
                    dungeonMoved = game.MoveEast();
                }
                else if (renderer.IsKeyPressed(KeyboardKey.KEY_LEFT) || renderer.IsKeyPressed(KeyboardKey.KEY_A))
                {
                    dungeonMoved = game.MoveWest();
                }

                if (dungeonMoved)
                {
                    game.MarkDungeonTileExplored(game.PlayerX, game.PlayerY);
                    game.MoveDungeonMobs();

                    // Check for dungeon mob collision
                    var dungeonMob = game.GetDungeonMobAt(game.PlayerX, game.PlayerY);
                    if (dungeonMob != null)
                    {
                        game.TriggerDungeonCombat();
                        logger.LogEvent("COMBAT", $"Fighting {game.EnemyName}");

                        // Auto-resolve combat for now
                        while (!game.CombatEnded)
                        {
                            game.ExecuteGameLoopRoundWithRandomHits(CombatAction.Attack, CombatAction.Attack);
                        }

                        if (game.IsWon)
                        {
                            game.ProcessGameLoopVictory();
                            game.RemoveDungeonMob(dungeonMob);
                            logger.LogEvent("VICTORY", "Dungeon mob defeated!");
                        }
                        else
                        {
                            HandleDeath($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
                        }
                    }

                    // Check dungeon tiles
                    string currentTile = game.GetDungeonTile(game.PlayerX, game.PlayerY);

                    if (currentTile == "Treasure")
                    {
                        int gold = game.RollForTreasure(game.DungeonDepth);
                        logger.LogEvent("TREASURE", $"Found {gold} gold!");
                        game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
                    }
                    else if (currentTile == "Trap")
                    {
                        int dmg = game.TriggerTrap();
                        logger.LogEvent("TRAP", $"Trap! Took {dmg} damage!");
                        game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");

                        if (game.PlayerHP <= 0)
                        {
                            HandleDeath("Trap");
                        }
                    }
                    else if (currentTile == "Boss")
                    {
                        game.TriggerBossCombat();
                        logger.LogEvent("BOSS", $"Boss fight: {game.EnemyName}!");

                        // Auto-resolve boss combat
                        while (!game.CombatEnded)
                        {
                            game.ExecuteGameLoopRoundWithRandomHits(CombatAction.Attack, CombatAction.Attack);
                        }

                        if (game.IsWon)
                        {
                            game.ProcessGameLoopVictory();
                            game.MarkBossDefeated();
                            game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
                            logger.LogEvent("BOSS", "BOSS DEFEATED!");
                        }
                        else
                        {
                            HandleDeath($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
                        }
                    }
                    else if (currentTile == "Artifact")
                    {
                        string msg = game.CollectArtifact();
                        logger.LogEvent("ARTIFACT", msg);
                        game.SetDungeonTileForTesting(game.PlayerX, game.PlayerY, "Floor");
                    }
                    else if (currentTile == "Portal")
                    {
                        string msg = game.UseDungeonPortal();
                        logger.LogEvent("PORTAL", msg);

                        if (game.RunWon)
                        {
                            logger.LogEvent("VICTORY", "GAME WON!");
                            playing = false;
                        }
                    }
                }

                // Descend stairs
                if (renderer.IsKeyPressed(KeyboardKey.KEY_ENTER) || renderer.IsKeyPressed(KeyboardKey.KEY_SPACE))
                {
                    if (game.GetDungeonTile(game.PlayerX, game.PlayerY) == "Stairs")
                    {
                        game.DescendDungeon();
                        logger.LogEvent("DUNGEON", $"Descended to Depth {game.DungeonDepth}");
                    }
                }

                // Exit dungeon
                if (renderer.IsKeyPressed(KeyboardKey.KEY_X))
                {
                    game.ExitDungeon();
                    logger.LogEvent("DUNGEON", "Exited dungeon");
                }

                // Use potion
                if (renderer.IsKeyPressed(KeyboardKey.KEY_P))
                {
                    if (game.UsePotion())
                    {
                        logger.LogEvent("ITEM", "Used potion! +5 HP");
                    }
                }
            }
        }

        // Cleanup
        renderer.Dispose();

        logger.LogEvent("GAME", "Game session ended");
        logger.ShowRecentEvents(20, excludeDumps: true);
        logger.DumpGameState(game, playing ? "Player quit" : "Game completed");

        Console.WriteLine("\nLog saved to: game_log.txt");
    }
}
