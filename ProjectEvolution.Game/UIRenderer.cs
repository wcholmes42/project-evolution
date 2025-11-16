namespace ProjectEvolution.Game;

public class UIRenderer
{
    private const int StatusBarHeight = 6;
    private const int MapViewHeight = 15;
    private const int MessageLogHeight = 5;

    private List<string> _messageLog = new List<string>();
    private const int MaxMessages = 4;

    public void Initialize()
    {
        Console.Clear();
        Console.CursorVisible = false;
        DrawBorders();
    }

    private void DrawBorders()
    {
        // Top border
        Console.SetCursorPosition(0, 0);
        Console.Write("╔" + new string('═', 78) + "╗");

        // Status section border
        Console.SetCursorPosition(0, StatusBarHeight);
        Console.Write("╠" + new string('═', 78) + "╣");

        // Map section border
        Console.SetCursorPosition(0, StatusBarHeight + MapViewHeight + 1);
        Console.Write("╠" + new string('═', 78) + "╣");

        // Bottom border
        Console.SetCursorPosition(0, StatusBarHeight + MapViewHeight + MessageLogHeight + 2);
        Console.Write("╚" + new string('═', 78) + "╝");

        // Side borders
        for (int i = 1; i < StatusBarHeight + MapViewHeight + MessageLogHeight + 2; i++)
        {
            Console.SetCursorPosition(0, i);
            Console.Write("║");
            Console.SetCursorPosition(79, i);
            Console.Write("║");
        }
    }

    public void RenderStatusBar(RPGGame game)
    {
        Console.SetCursorPosition(2, 1);
        Console.Write(new string(' ', 76)); // Clear line
        Console.SetCursorPosition(2, 1);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"PROJECT EVOLUTION - GENERATION 26: ULTIMATE ASCII UI");
        Console.ResetColor();

        Console.SetCursorPosition(2, 2);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 2);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"❤️  HP: {game.PlayerHP,3}/{game.MaxPlayerHP,-3}");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($" ⚡ STA: 12/12");
        Console.ForegroundColor = ConsoleColor.Magenta;
        Console.Write($" ⭐ Lvl: {game.PlayerLevel}");
        Console.ResetColor();

        Console.SetCursorPosition(2, 3);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 3);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"💰 Gold: {game.PlayerGold,4}g");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"  📊 XP: {game.PlayerXP}/{game.XPForNextLevel}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"  🧪 Potions: {game.PotionCount}");
        Console.ResetColor();

        Console.SetCursorPosition(2, 4);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 4);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"💪 STR: {game.PlayerStrength,2}  🛡️  DEF: {game.PlayerDefense,2}");
        if (game.AvailableStatPoints > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  ⚡ UNSPENT POINTS: {game.AvailableStatPoints}!");
        }
        Console.ResetColor();

        Console.SetCursorPosition(2, 5);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 5);
        Console.Write($"📍 ({game.PlayerX,2},{game.PlayerY,2})  ");

        var terrain = game.GetCurrentTerrain();
        Console.ForegroundColor = terrain switch
        {
            "Forest" => ConsoleColor.DarkGreen,
            "Mountain" => ConsoleColor.DarkGray,
            "Town" => ConsoleColor.Yellow,
            "Dungeon" => ConsoleColor.Red,
            _ => ConsoleColor.Green
        };
        Console.Write($"🗺️  {terrain}");
        Console.ResetColor();

        if (game.InDungeon)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"  ⚔️  DEPTH: {game.DungeonDepth}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  ⏱️  Turn: {game.WorldTurn}");
            Console.ResetColor();
        }
    }

    public void RenderMap(RPGGame game)
    {
        if (game.InDungeon)
        {
            RenderDungeonMap(game);
        }
        else
        {
            RenderWorldMap(game);
        }
    }

    private void RenderWorldMap(RPGGame game)
    {
        const int viewRadius = 3;
        int startRow = StatusBarHeight + 1;

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            int screenY = startRow + (dy + viewRadius);
            Console.SetCursorPosition(2, screenY);
            Console.Write(new string(' ', 76)); // Clear line
            Console.SetCursorPosition(2, screenY);

            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                int worldX = game.PlayerX + dx;
                int worldY = game.PlayerY + dy;

                if (worldX < 0 || worldX >= game.WorldWidth || worldY < 0 || worldY >= game.WorldHeight)
                {
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" · "); // Out of bounds
                    Console.ResetColor();
                    continue;
                }

                // Fog of war - only show explored tiles
                bool explored = game.IsTileExplored(worldX, worldY);

                // Center is player
                if (dx == 0 && dy == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" @ ");
                    Console.ResetColor();
                }
                // Check for mob (only if visible/explored)
                else if (explored && game.IsMobAt(worldX, worldY))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" M ");
                    Console.ResetColor();
                }
                else if (explored)
                {
                    // Get terrain at this position
                    string terrain = GetTerrainAtPosition(game, worldX, worldY);
                    (char symbol, ConsoleColor color) = terrain switch
                    {
                        "Grassland" => ('.', ConsoleColor.Green),
                        "Forest" => ('♣', ConsoleColor.DarkGreen),
                        "Mountain" => ('▲', ConsoleColor.DarkGray),
                        "Town" => ('■', ConsoleColor.Yellow),
                        "Dungeon" => ('Ω', ConsoleColor.Red),
                        _ => ('.', ConsoleColor.Gray)
                    };

                    Console.ForegroundColor = color;
                    Console.Write($" {symbol} ");
                    Console.ResetColor();
                }
                else
                {
                    // Unexplored - fog of war
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    Console.Write(" ? ");
                    Console.ResetColor();
                }
            }
        }

        // Legend
        Console.SetCursorPosition(2, startRow + 8);
        Console.Write("Legend: @ = You  M = Mob  ? = Unexplored  ■ = Town  Ω = Dungeon          ");
    }

    private void RenderDungeonMap(RPGGame game)
    {
        const int viewRadius = 7; // Larger view in dungeons
        int startRow = StatusBarHeight + 1;

        for (int dy = -viewRadius; dy <= viewRadius; dy++)
        {
            int screenY = startRow + (dy + viewRadius);
            if (screenY >= StatusBarHeight + MapViewHeight + 1) break;

            Console.SetCursorPosition(2, screenY);
            Console.Write(new string(' ', 76)); // Clear line
            Console.SetCursorPosition(2, screenY);

            for (int dx = -viewRadius; dx <= viewRadius; dx++)
            {
                int dungeonX = game.PlayerX + dx;
                int dungeonY = game.PlayerY + dy;

                // Center is player
                if (dx == 0 && dy == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write("@");
                    Console.ResetColor();
                }
                else
                {
                    string tile = game.GetDungeonTile(dungeonX, dungeonY);

                    (char symbol, ConsoleColor color) = tile switch
                    {
                        "Floor" => ('.', ConsoleColor.DarkGray),
                        "Wall" => ('█', ConsoleColor.Gray),
                        "OutOfBounds" => (' ', ConsoleColor.Black),
                        _ => (' ', ConsoleColor.Black)
                    };

                    Console.ForegroundColor = color;
                    Console.Write(symbol);
                    Console.ResetColor();
                }
            }
        }

        // Legend
        Console.SetCursorPosition(2, startRow + 8);
        Console.Write("Dungeon: @ = You  . = Floor  █ = Wall  [X] Exit                      ");
    }

    private string GetTerrainAtPosition(RPGGame game, int x, int y)
    {
        return game.GetTerrainAt(x, y);
    }

    public void AddMessage(string message)
    {
        _messageLog.Add(message);
        if (_messageLog.Count > MaxMessages)
        {
            _messageLog.RemoveAt(0);
        }
        RenderMessageLog();
    }

    private void RenderMessageLog()
    {
        int startRow = StatusBarHeight + MapViewHeight + 2;

        // Calculate starting index in message log
        int startIndex = Math.Max(0, _messageLog.Count - MaxMessages);
        int messagesToShow = Math.Min(MaxMessages, _messageLog.Count);

        for (int i = 0; i < MaxMessages; i++)
        {
            Console.SetCursorPosition(2, startRow + i);
            Console.Write(new string(' ', 76)); // Clear

            if (i < messagesToShow)
            {
                Console.SetCursorPosition(2, startRow + i);
                string msg = _messageLog[startIndex + i];
                if (msg.Length > 76) msg = msg.Substring(0, 73) + "...";

                // Color code messages
                if (msg.Contains("Victory") || msg.Contains("Found") || msg.Contains("✅"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (msg.Contains("TRAP") || msg.Contains("damage") || msg.Contains("💀") || msg.Contains("💥"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (msg.Contains("LEVEL UP") || msg.Contains("⭐"))
                    Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write(msg);
                Console.ResetColor();
            }
        }
    }

    public void RenderCommandBar(bool inDungeon)
    {
        int row = StatusBarHeight + MapViewHeight + MessageLogHeight + 3;
        Console.SetCursorPosition(2, row);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, row);

        if (!inDungeon)
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write("[N/S/E/W] Move  [ENTER] Enter Location  [P] Use Potion  [Q] Quit");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write("[N/S/E/W] Move  [X] Exit Dungeon  [P] Use Potion  [Q] Quit");
        }
        Console.ResetColor();
    }

    public void RenderCombat(RPGGame game)
    {
        // Render "across the table" combat view
        int startRow = StatusBarHeight + 1;

        Console.SetCursorPosition(2, startRow);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("════════════════════ ⚔️  COMBAT ⚔️  ════════════════════");
        Console.ResetColor();

        // Player side
        Console.SetCursorPosition(2, startRow + 2);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 2);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("YOU:");
        Console.ResetColor();
        Console.SetCursorPosition(2, startRow + 3);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 3);
        Console.Write($"  ❤️  HP: {game.PlayerHP,3}/{game.MaxPlayerHP,-3}");
        Console.SetCursorPosition(2, startRow + 4);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 4);
        Console.Write($"  ⚡ STA: {game.PlayerStamina,3}/12");
        Console.SetCursorPosition(2, startRow + 5);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 5);
        Console.Write($"  💪 ATK: {game.PlayerStrength}  🛡️  DEF: {game.PlayerDefense}");

        // VS
        Console.SetCursorPosition(35, startRow + 3);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("VS");
        Console.ResetColor();

        // Enemy side
        Console.SetCursorPosition(45, startRow + 2);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
        Console.ResetColor();
        Console.SetCursorPosition(45, startRow + 3);
        Console.Write($"❤️  HP: {game.EnemyHP}");
        Console.SetCursorPosition(45, startRow + 4);
        Console.Write($"⚔️  DMG: {game.EnemyDamage}");

        // Combat options
        Console.SetCursorPosition(2, startRow + 7);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 7);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[A] Attack  [D] Defend  [P] Potion  [F] Flee (50% chance)");
        Console.ResetColor();
    }

    public void RenderDebugPanel(RPGGame game, AutoPlayer? autoPlayer)
    {
        if (autoPlayer == null) return;

        // Debug panel on the right side of the screen (37 char wide)
        int debugX = 82;
        int startY = 1;
        const int contentWidth = 35; // Width for text content inside borders

        // Header
        Console.SetCursorPosition(debugX, startY);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("╔═══════════════════════════════════╗");
        Console.SetCursorPosition(debugX, startY + 1);
        Console.Write("║      AI DEBUG PANEL 🤖            ║");
        Console.SetCursorPosition(debugX, startY + 2);
        Console.Write("╠═══════════════════════════════════╣");
        Console.ResetColor();

        // Current Goal
        Console.SetCursorPosition(debugX, startY + 3);
        Console.Write("║ CURRENT GOAL:                     ║");
        Console.SetCursorPosition(debugX, startY + 4);
        Console.Write("║ ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        string goalDesc = autoPlayer.GetCurrentGoalDescription();
        if (goalDesc.Length > contentWidth) goalDesc = goalDesc.Substring(0, contentWidth);
        Console.Write(goalDesc.PadRight(contentWidth));
        Console.ResetColor();
        Console.Write("║");

        // Primary Objective
        Console.SetCursorPosition(debugX, startY + 5);
        Console.Write("║ PRIMARY OBJECTIVE:                ║");
        Console.SetCursorPosition(debugX, startY + 6);
        Console.Write("║ ");
        Console.ForegroundColor = ConsoleColor.Yellow;
        string objective = autoPlayer.Goals.GetPrimaryObjective(game);
        if (objective.Length > contentWidth) objective = objective.Substring(0, contentWidth);
        Console.Write(objective.PadRight(contentWidth));
        Console.ResetColor();
        Console.Write("║");

        // Current Target
        Console.SetCursorPosition(debugX, startY + 7);
        Console.Write("║ CURRENT TARGET:                   ║");
        Console.SetCursorPosition(debugX, startY + 8);
        Console.Write("║ ");
        Console.ForegroundColor = ConsoleColor.Green;
        string target = autoPlayer.CurrentTarget;
        if (target.Length > contentWidth) target = target.Substring(0, contentWidth - 3) + "...";
        Console.Write(target.PadRight(contentWidth));
        Console.ResetColor();
        Console.Write("║");

        // Last Decision
        Console.SetCursorPosition(debugX, startY + 9);
        Console.Write("║ REASONING:                        ║");
        Console.SetCursorPosition(debugX, startY + 10);
        Console.Write("║ ");
        Console.ForegroundColor = ConsoleColor.Magenta;
        string decision = autoPlayer.LastDecision;
        if (decision.Length > contentWidth) decision = decision.Substring(0, contentWidth - 3) + "...";
        Console.Write(decision.PadRight(contentWidth));
        Console.ResetColor();
        Console.Write("║");

        // Progress
        Console.SetCursorPosition(debugX, startY + 11);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(debugX, startY + 12);
        Console.Write("║ PROGRESS:                         ║");

        Console.SetCursorPosition(debugX, startY + 13);
        Console.Write("║ ");
        string levelProg = $"Level: {game.PlayerLevel}/{autoPlayer.Goals.TargetLevel}";
        Console.Write(levelProg.PadRight(contentWidth));
        Console.Write("║");

        Console.SetCursorPosition(debugX, startY + 14);
        Console.Write("║ ");
        string goldProg = $"Gold: {game.PlayerGold}/{autoPlayer.Goals.TargetGold}";
        Console.Write(goldProg.PadRight(contentWidth));
        Console.Write("║");

        Console.SetCursorPosition(debugX, startY + 15);
        Console.Write("║ ");
        string dungProg = $"Dungeons: {autoPlayer.Goals.DungeonsExplored}/{autoPlayer.Goals.TargetDungeons}";
        Console.Write(dungProg.PadRight(contentWidth));
        Console.Write("║");

        // Stats
        Console.SetCursorPosition(debugX, startY + 16);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(debugX, startY + 17);
        Console.Write("║ SESSION STATS:                    ║");

        Console.SetCursorPosition(debugX, startY + 18);
        Console.Write("║ ");
        string turnsStat = $"Turns: {autoPlayer.TurnsSurvived}";
        Console.Write(turnsStat.PadRight(contentWidth));
        Console.Write("║");

        Console.SetCursorPosition(debugX, startY + 19);
        Console.Write("║ ");
        string combatsStat = $"Combats Won: {autoPlayer.CombatsWon}";
        Console.Write(combatsStat.PadRight(contentWidth));
        Console.Write("║");

        Console.SetCursorPosition(debugX, startY + 20);
        Console.Write("║ ");
        string fledStat = $"Fled: {autoPlayer.CombatsFled}";
        Console.Write(fledStat.PadRight(contentWidth));
        Console.Write("║");

        // Footer
        Console.SetCursorPosition(debugX, startY + 21);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(debugX, startY + 22);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write("║   [ESC] Stop Simulation           ║");
        Console.ResetColor();
        Console.SetCursorPosition(debugX, startY + 23);
        Console.Write("╚═══════════════════════════════════╝");
    }

    public void Cleanup()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ResetColor();
    }
}


