using ProjectEvolution.Game;

Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║  PROJECT EVOLUTION - GENERATION 25     ║");
Console.WriteLine("║  WARHAMMER QUEST + ULTIMA IV FUSION    ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("🗺️  EXPLORE THE WORLD:");
Console.WriteLine("  N/S/E/W - Move on 20x20 map");
Console.WriteLine("  E - Enter Town/Dungeon");
Console.WriteLine("  Q - Quit game");
Console.WriteLine();
Console.WriteLine("🏘️  TOWNS: Inn (heal 10g) | Shop (potion 5g)");
Console.WriteLine("⚔️  DUNGEONS: Descend deeper for treasure!");
Console.WriteLine("🎲 ENCOUNTERS: Forest 40% | Mountain 30% | Grass 20%");
Console.WriteLine("⚡ EVENTS: Traps | Discoveries | Treasures");
Console.WriteLine();

var game = new RPGGame();
game.SetPlayerStats(strength: 2, defense: 1);
game.StartWorldExploration();

bool playing = true;

while (playing)
{
    Console.WriteLine("════════════════════════════════════════");
    Console.WriteLine($"YOU: Lvl{game.PlayerLevel} HP:{game.PlayerHP}/{game.MaxPlayerHP} STR:{game.PlayerStrength} DEF:{game.PlayerDefense}");
    Console.WriteLine($"XP:{game.PlayerXP}/{game.XPForNextLevel} | Gold:{game.PlayerGold}g | Potions:{game.PotionCount}");
    Console.WriteLine($"Position: ({game.PlayerX},{game.PlayerY}) | Terrain: {game.GetCurrentTerrain()}");
    if (game.InDungeon) Console.WriteLine($"⚔️  IN DUNGEON - Depth {game.DungeonDepth}");
    Console.WriteLine();

    if (!game.InDungeon)
    {
        Console.WriteLine("Move: [N]orth [S]outh [E]ast [W]est | [Enter] location | [Q]uit");
    }
    else
    {
        Console.WriteLine("[R]oll for room | [D]escend deeper | E[x]it dungeon");
    }
    Console.Write("> ");

    var input = Console.ReadLine()?.ToUpper();

    if (input == "Q")
    {
        playing = false;
        Console.WriteLine("Thanks for playing!");
        continue;
    }

    // World movement
    if (!game.InDungeon)
    {
        bool moved = false;
        if (input == "N") moved = game.MoveNorth();
        else if (input == "S") moved = game.MoveSouth();
        else if (input == "E") moved = game.MoveEast();
        else if (input == "W") moved = game.MoveWest();
        else if (input == "ENTER")
        {
            if (game.GetCurrentTerrain() == "Town")
            {
                game.EnterLocation();
                Console.WriteLine("🏘️  Entered Town! [I]nn | [B]uy potion | E[x]it");
                var townChoice = Console.ReadLine()?.ToUpper();
                if (townChoice == "I")
                {
                    if (game.VisitInn()) Console.WriteLine("✅ Healed to full! -10g");
                    else Console.WriteLine("❌ Not enough gold (need 10g)");
                }
                else if (townChoice == "B")
                {
                    if (game.BuyPotion()) Console.WriteLine("✅ Bought potion! -5g");
                    else Console.WriteLine("❌ Not enough gold (need 5g)");
                }
                game.ExitLocation();
            }
            else if (game.GetCurrentTerrain() == "Dungeon")
            {
                game.EnterDungeon();
                Console.WriteLine("⚔️  Entered Dungeon! Depth 1");
            }
        }

        if (moved)
        {
            Console.WriteLine($"Moved to {game.GetCurrentTerrain()}");

            // Roll for encounter
            if (game.RollForEncounter())
            {
                Console.WriteLine("💥 ENCOUNTER!");
                game.TriggerEncounter();

                // Combat loop
                while (!game.CombatEnded)
                {
                    Console.WriteLine($"HP:{game.PlayerHP}/{game.MaxPlayerHP} STA:{game.PlayerStamina}/12 | Enemy:{game.EnemyName} HP:{game.EnemyHP}");
                    Console.WriteLine("[A]ttack | [D]efend | [P]otion");
                    var combatChoice = Console.ReadLine()?.ToUpper();

                    CombatAction action = combatChoice == "A" ? CombatAction.Attack : CombatAction.Defend;
                    if (combatChoice == "P" && game.UsePotion())
                    {
                        Console.WriteLine("Used potion! +5 HP");
                        continue;
                    }

                    game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);
                    Console.WriteLine(game.CombatLog);

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (game.IsWon) Console.WriteLine("✅ Victory!");
                        else
                        {
                            Console.WriteLine("💀 GAME OVER!");
                            playing = false;
                        }
                    }
                }
            }
        }
    }
    else // In dungeon
    {
        if (input == "R")
        {
            var roomType = game.RollForRoom();
            Console.WriteLine($"🎲 Room: {roomType}");

            if (roomType == "Monster")
            {
                game.TriggerDungeonCombat();
                while (!game.CombatEnded)
                {
                    Console.WriteLine($"HP:{game.PlayerHP}/{game.MaxPlayerHP} | Enemy:{game.EnemyName}[Lvl{game.EnemyLevel}] HP:{game.EnemyHP}");
                    Console.WriteLine("[A]ttack | [D]efend | [P]otion");
                    var choice = Console.ReadLine()?.ToUpper();
                    if (choice == "P" && game.UsePotion()) { Console.WriteLine("+5 HP"); continue; }

                    game.ExecuteGameLoopRoundWithRandomHits(choice == "A" ? CombatAction.Attack : CombatAction.Defend, CombatAction.Attack);
                    Console.WriteLine(game.CombatLog);

                    if (game.CombatEnded)
                    {
                        game.ProcessGameLoopVictory();
                        if (!game.IsWon) { Console.WriteLine("💀 DEAD!"); playing = false; }
                    }
                }
            }
            else if (roomType == "Treasure")
            {
                int gold = game.RollForTreasure(game.DungeonDepth);
                Console.WriteLine($"💎 Found treasure! +{gold}g");
            }
            else
            {
                var eventRoll = game.RollForEvent();
                if (eventRoll == "Trap")
                {
                    int dmg = game.TriggerTrap();
                    Console.WriteLine($"💥 TRAP! {dmg} damage!");
                }
                else if (eventRoll == "Discovery")
                {
                    Console.WriteLine($"✨ {game.TriggerDiscovery()}");
                }
            }
        }
        else if (input == "D")
        {
            game.DescendDungeon();
            Console.WriteLine($"⬇️  Descended to depth {game.DungeonDepth}!");
        }
        else if (input == "X")
        {
            game.ExitDungeon();
            Console.WriteLine("Exited dungeon!");
        }
    }

    Console.WriteLine();
}

Console.WriteLine("Press any key to exit...");
Console.ReadKey();
