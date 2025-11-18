namespace ProjectEvolution.Game;

public class UIRenderer
{
    private const int StatusBarHeight = 6;
    private const int MapViewHeight = 15;
    private const int MessageLogHeight = 5;

    private List<string> _messageLog = new List<string>();
    private const int MaxMessages = 5; // Must match MessageLogHeight to prevent overflow!
    private Dictionary<string, int> _messageCount = new Dictionary<string, int>();

    public void Initialize()
    {
        // Target: 1920x1080 resolution = 240x67 chars (at 8x16 font)
        const int targetWidth = 240;
        const int targetHeight = 67;

        try
        {
            if (OperatingSystem.IsWindows())
            {
                // Windows: Set fixed window size (not resizable)
                Console.SetWindowSize(targetWidth, targetHeight);
                Console.SetBufferSize(targetWidth, targetHeight);
            }
            else
            {
                // macOS/Linux: Can't set window size programmatically
                // User must manually resize terminal to 240x67 (1920x1080)
                if (Console.WindowWidth < 120 || Console.WindowHeight < 30)
                {
                    Console.WriteLine("⚠️  WARNING: Terminal too small!");
                    Console.WriteLine($"   Current: {Console.WindowWidth}x{Console.WindowHeight}");
                    Console.WriteLine($"   Recommended: {targetWidth}x{targetHeight} (1920x1080)");
                    Console.WriteLine($"   Minimum: 120x30");
                    Console.WriteLine("\nResize your terminal window for best experience.");
                    Console.WriteLine("Press any key to continue...");
                    Console.ReadKey(intercept: true);
                }
            }
        }
        catch
        {
            // Ignore if platform doesn't support window sizing
        }

        Console.Clear();
        Console.CursorVisible = false;
        DrawBorders();
    }

    // Helper: Generate visual HP bar
    private string GenerateHPBar(int current, int max, int barWidth = 10)
    {
        if (max <= 0) return new string('░', barWidth);

        float percentage = (float)current / max;
        int filledBlocks = (int)(percentage * barWidth);
        filledBlocks = Math.Clamp(filledBlocks, 0, barWidth);

        string bar = new string('█', filledBlocks) + new string('░', barWidth - filledBlocks);
        return $"[{bar}]";
    }

    // Helper: Get HP bar color based on percentage
    private ConsoleColor GetHPColor(int current, int max)
    {
        if (max <= 0) return ConsoleColor.Gray;
        float percentage = (float)current / max;

        if (percentage > 0.66f) return ConsoleColor.Green;
        if (percentage > 0.33f) return ConsoleColor.Yellow;
        return ConsoleColor.Red;
    }

    private void DrawBorders()
    {
        const int Width = 80; // Fixed 80 column layout
        const int InnerWidth = 78; // Width - 2 for borders

        // Top border
        SafeWriteAt(0, 0, "╔" + new string('═', InnerWidth) + "╗");

        // Status section border
        SafeWriteAt(0, StatusBarHeight, "╠" + new string('═', InnerWidth) + "╣");

        // Map section border
        SafeWriteAt(0, StatusBarHeight + MapViewHeight + 1, "╠" + new string('═', InnerWidth) + "╣");

        // Bottom border
        SafeWriteAt(0, StatusBarHeight + MapViewHeight + MessageLogHeight + 2, "╚" + new string('═', InnerWidth) + "╝");

        // Side borders
        for (int i = 1; i < StatusBarHeight + MapViewHeight + MessageLogHeight + 2; i++)
        {
            SafeWriteAt(0, i, "║");
            SafeWriteAt(Width - 1, i, "║");
        }
    }

    // Helper: Safe console write with bounds checking
    private void SafeWriteAt(int x, int y, string text, ConsoleColor? color = null)
    {
        try
        {
            if (x < 0 || y < 0 || x >= Console.WindowWidth || y >= Console.WindowHeight) return;

            // Truncate text if it would exceed window width
            int maxLen = Math.Max(0, Console.WindowWidth - x);
            if (text.Length > maxLen)
            {
                text = text.Substring(0, maxLen);
            }

            Console.SetCursorPosition(x, y);
            if (color.HasValue) Console.ForegroundColor = color.Value;
            Console.Write(text);
            if (color.HasValue) Console.ResetColor();
        }
        catch
        {
            // Ignore rendering errors (window resize, etc.)
        }
    }

    // Helper: Truncate text to fit within width
    private string TruncateText(string text, int maxWidth)
    {
        if (string.IsNullOrEmpty(text)) return string.Empty;
        if (text.Length <= maxWidth) return text;
        return text.Substring(0, Math.Max(0, maxWidth - 3)) + "...";
    }

    public void RenderStatusBar(RPGGame game)
    {
        const int InnerWidth = 76; // 80 - 2 for borders - 2 for padding

        SafeWriteAt(2, 1, new string(' ', InnerWidth));
        SafeWriteAt(2, 1, TruncateText("PROJECT EVOLUTION - GENERATION 35: UX EVOLUTION!", InnerWidth), ConsoleColor.Yellow);

        // STAT POINTS NOTIFICATION - Make it IMPOSSIBLE to miss!
        if (game.AvailableStatPoints > 0)
        {
            Console.SetCursorPosition(2, 2);
            Console.Write(new string(' ', 76));
            Console.SetCursorPosition(2, 2);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"*** UNSPENT STAT POINTS: {game.AvailableStatPoints} *** Press [L] to Level Up! ***");
            Console.ResetColor();
        }
        else
        {
            // Normal HP display with visual bar
            Console.SetCursorPosition(2, 2);
            Console.Write(new string(' ', 76));
            Console.SetCursorPosition(2, 2);
            Console.ForegroundColor = GetHPColor(game.PlayerHP, game.MaxPlayerHP);
            string hpBar = GenerateHPBar(game.PlayerHP, game.MaxPlayerHP, 10);
            float hpPercent = (float)game.PlayerHP / game.MaxPlayerHP * 100f;
            Console.Write($"HP: {hpBar} {game.PlayerHP,3}/{game.MaxPlayerHP,-3} ({hpPercent:F0}%)");
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"  Lvl: {game.PlayerLevel}");
            Console.ResetColor();
        }

        Console.SetCursorPosition(2, 3);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 3);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"Gold: {game.PlayerGold,4}g");
        Console.ForegroundColor = ConsoleColor.Blue;
        Console.Write($"  XP: {game.PlayerXP}/{game.XPForNextLevel}");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"  Potions: {game.PotionCount}");
        Console.ResetColor();

        Console.SetCursorPosition(2, 4);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 4);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"STR: {game.PlayerStrength,2}  DEF: {game.PlayerDefense,2}");
        if (game.AvailableStatPoints > 0)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"  UNSPENT POINTS: {game.AvailableStatPoints}!");
        }
        Console.ResetColor();

        Console.SetCursorPosition(2, 5);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, 5);
        Console.Write($"({game.PlayerX,2},{game.PlayerY,2})  ");

        var terrain = game.GetCurrentTerrain();
        Console.ForegroundColor = terrain switch
        {
            "Forest" => ConsoleColor.DarkGreen,
            "Mountain" => ConsoleColor.DarkGray,
            "Town" => ConsoleColor.Yellow,
            "Dungeon" => ConsoleColor.Red,
            _ => ConsoleColor.Green
        };
        Console.Write($"{terrain}");
        Console.ResetColor();

        if (game.InDungeon)
        {
            Console.ForegroundColor = ConsoleColor.DarkRed;
            Console.Write($"  DEPTH: {game.DungeonDepth}");
            Console.ResetColor();
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write($"  Turn: {game.WorldTurn}");
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
                    // Check if this is death location first
                    if (game.DeathLocationX == worldX && game.DeathLocationY == worldY && game.CanRetrieveDroppedItems())
                    {
                        Console.ForegroundColor = ConsoleColor.DarkRed;
                        Console.Write(" X ");
                        Console.ResetColor();
                    }
                    else
                    {
                        // Get terrain at this position
                        string terrain = GetTerrainAtPosition(game, worldX, worldY);
                        (char symbol, ConsoleColor color) = terrain switch
                        {
                            "Grassland" => ('.', ConsoleColor.Green),
                            "Forest" => ('♣', ConsoleColor.DarkGreen),
                            "Mountain" => ('▲', ConsoleColor.DarkGray),
                            "Town" => ('■', ConsoleColor.Yellow),
                            "Temple" => ('+', ConsoleColor.Cyan),  // Temple symbol
                            "Dungeon" => ('Ω', ConsoleColor.Red),
                            _ => ('.', ConsoleColor.Gray)
                        };

                        Console.ForegroundColor = color;
                        Console.Write($" {symbol} ");
                        Console.ResetColor();
                    }
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
        Console.Write("Legend: @ = You  M = Mob  + = Temple  # = Town  O = Dungeon  X = Corpse");
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
                    Console.Write(" @ ");
                    Console.ResetColor();
                }
                // Check for dungeon mob at this position
                else if (game.IsDungeonMobAt(dungeonX, dungeonY))
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.Write(" M ");
                    Console.ResetColor();
                }
                else
                {
                    string tile = game.GetDungeonTile(dungeonX, dungeonY);

                    (string symbol, ConsoleColor color) = tile switch
                    {
                        "Floor" => (" . ", ConsoleColor.DarkGreen),     // Floor
                        "Wall" => (" # ", ConsoleColor.DarkGray),       // Walls
                        "Treasure" => (" $ ", ConsoleColor.Yellow),     // Gold!
                        "Trap" => (" ! ", ConsoleColor.Red),            // Danger!
                        "Monster" => (" M ", ConsoleColor.Magenta),     // Enemy!
                        "Boss" => (" B ", ConsoleColor.DarkRed),        // BOSS!
                        "Artifact" => (" A ", ConsoleColor.White),      // Legendary item!
                        "Portal" => (" O ", ConsoleColor.Green),        // Exit portal!
                        "Stairs" => (" > ", ConsoleColor.Cyan),         // Next floor!
                        "OutOfBounds" => ("   ", ConsoleColor.Black),
                        _ => ("   ", ConsoleColor.Black)
                    };

                    Console.ForegroundColor = color;
                    Console.Write(symbol);
                    Console.ResetColor();
                }
            }
        }

        // No legend needed in dungeon - symbols are obvious!
        // Legend would overlap map view - command bar at bottom shows controls
    }

    private string GetTerrainAtPosition(RPGGame game, int x, int y)
    {
        return game.GetTerrainAt(x, y);
    }

    public void AddMessage(string message)
    {
        // Collapse duplicate consecutive messages
        if (_messageLog.Count > 0 && _messageLog[_messageLog.Count - 1] == message)
        {
            // Same message repeated - increment counter
            if (_messageCount.ContainsKey(message))
            {
                _messageCount[message]++;
            }
            else
            {
                _messageCount[message] = 2; // First repeat
            }
        }
        else
        {
            // New message - add normally
            _messageLog.Add(message);
            _messageCount[message] = 1;

            if (_messageLog.Count > MaxMessages)
            {
                string removed = _messageLog[0];
                _messageLog.RemoveAt(0);
                _messageCount.Remove(removed);
            }
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

                // Add repeat counter if applicable
                if (_messageCount.ContainsKey(msg) && _messageCount[msg] > 1)
                {
                    msg = $"{msg} (×{_messageCount[msg]})";
                }

                if (msg.Length > 76) msg = msg.Substring(0, 73) + "...";

                // Color code messages
                if (msg.Contains("Victory") || msg.Contains("Found") || msg.Contains("OK"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (msg.Contains("TRAP") || msg.Contains("damage") || msg.Contains("DEAD") || msg.Contains("BOOM"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (msg.Contains("LEVEL UP"))
                    Console.ForegroundColor = ConsoleColor.Yellow;
                else if (msg.Contains("STAT POINTS"))
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
            Console.Write("[NSEW] Move  [Enter] Interact  [L] Stats  [I] Sheet  [H] Help  [Q] Quit");
        }
        else
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("[NSEW] Move  [D] Descend  [P] Potion  [I] Sheet  [H] Help  [X] Exit");
        }
        Console.ResetColor();
    }

    public void RenderCombat(RPGGame game)
    {
        // Clear the entire map area first
        int startRow = StatusBarHeight + 1;
        for (int i = 0; i < MapViewHeight; i++)
        {
            Console.SetCursorPosition(1, startRow + i);
            Console.Write(new string(' ', 78));
        }

        // Render "across the table" combat view
        Console.SetCursorPosition(2, startRow);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("════════════════════ COMBAT ════════════════════");
        Console.ResetColor();

        // Player side - with HP bar!
        Console.SetCursorPosition(2, startRow + 2);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 2);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("YOU:");
        Console.ResetColor();

        Console.SetCursorPosition(2, startRow + 3);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 3);
        Console.ForegroundColor = GetHPColor(game.PlayerHP, game.MaxPlayerHP);
        string playerBar = GenerateHPBar(game.PlayerHP, game.MaxPlayerHP, 12);
        float playerPercent = (float)game.PlayerHP / game.MaxPlayerHP * 100f;
        Console.Write($"  HP: {playerBar} {playerPercent:F0}%");
        Console.ResetColor();

        Console.SetCursorPosition(2, startRow + 4);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 4);
        Console.Write($"      {game.PlayerHP,3}/{game.MaxPlayerHP,-3}");

        Console.SetCursorPosition(2, startRow + 5);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 5);
        Console.Write($"  ATK: {game.GetEffectiveStrength()}  DEF: {game.GetEffectiveDefense()}");

        // GENERATION 35: Show active buffs
        Console.SetCursorPosition(2, startRow + 6);
        Console.Write(new string(' ', 76));
        string buffs = game.GetActiveBuffsDisplay();
        if (!string.IsNullOrEmpty(buffs))
        {
            Console.SetCursorPosition(2, startRow + 6);
            Console.ForegroundColor = ConsoleColor.Magenta;
            Console.Write($"  {buffs}");
            Console.ResetColor();
        }

        // VS
        Console.SetCursorPosition(35, startRow + 3);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("VS");
        Console.ResetColor();

        // Enemy side - with HP bar!
        Console.SetCursorPosition(45, startRow + 2);
        Console.Write(new string(' ', 30));
        Console.SetCursorPosition(45, startRow + 2);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"{game.EnemyName} [Lvl {game.EnemyLevel}]");
        Console.ResetColor();

        // Calculate enemy max HP (estimate based on level and type)
        int enemyMaxHP = Math.Max(game.EnemyHP, game.EnemyLevel * 2 + 5);
        Console.SetCursorPosition(45, startRow + 3);
        Console.Write(new string(' ', 30));
        Console.SetCursorPosition(45, startRow + 3);
        Console.ForegroundColor = GetHPColor(game.EnemyHP, enemyMaxHP);
        string enemyBar = GenerateHPBar(game.EnemyHP, enemyMaxHP, 10);
        Console.Write($"HP: {enemyBar}");
        Console.ResetColor();

        Console.SetCursorPosition(45, startRow + 4);
        Console.Write(new string(' ', 30));
        Console.SetCursorPosition(45, startRow + 4);
        Console.Write($"    {game.EnemyHP} HP");

        Console.SetCursorPosition(45, startRow + 5);
        Console.Write(new string(' ', 30));
        Console.SetCursorPosition(45, startRow + 5);
        Console.Write($"DMG: {game.EnemyDamage}");

        // Combat options (GENERATION 35: Added Skills)
        Console.SetCursorPosition(2, startRow + 8);
        Console.Write(new string(' ', 76));
        Console.SetCursorPosition(2, startRow + 8);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("[A] Attack  [D] Defend  [S] Skills  [P] Potion  [F] Flee");
        Console.ResetColor();
    }

    // GENERATION 35: Skills Menu
    public void RenderSkillsMenu(RPGGame game)
    {
        int startRow = StatusBarHeight + 2;
        int menuWidth = 60;
        int menuLeft = (80 - menuWidth) / 2; // Center on 80-column console

        // Draw skills menu box
        Console.SetCursorPosition(menuLeft, startRow);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("╔" + new string('═', menuWidth - 2) + "╗");

        Console.SetCursorPosition(menuLeft, startRow + 1);
        Console.Write("║");
        Console.SetCursorPosition(menuLeft + menuWidth / 2 - 5, startRow + 1);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("⚡ SKILLS ⚡");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.SetCursorPosition(menuLeft + menuWidth - 1, startRow + 1);
        Console.Write("║");

        Console.SetCursorPosition(menuLeft, startRow + 2);
        Console.Write("╠" + new string('═', menuWidth - 2) + "╣");
        Console.ResetColor();

        var skills = game.GetAvailableSkills();
        int row = startRow + 3;

        foreach (var skill in skills)
        {
            Console.SetCursorPosition(menuLeft, row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("║");
            Console.ResetColor();

            Console.SetCursorPosition(menuLeft + 2, row);

            bool canUse = game.CanUseSkill(skill);
            Console.ForegroundColor = canUse ? ConsoleColor.Green : ConsoleColor.DarkGray;

            // Show skill number (1-5)
            int skillNum = skills.IndexOf(skill) + 1;
            Console.Write($"[{skillNum}] ");

            Console.ForegroundColor = canUse ? ConsoleColor.White : ConsoleColor.DarkGray;
            Console.Write($"{skill.Name,-20}");

            Console.ForegroundColor = canUse ? ConsoleColor.Cyan : ConsoleColor.DarkGray;
            Console.Write($" ({skill.StaminaCost} ⚡)");

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(menuLeft + menuWidth - 1, row);
            Console.Write("║");
            Console.ResetColor();

            // Skill description on next line
            row++;
            Console.SetCursorPosition(menuLeft, row);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("║");
            Console.SetCursorPosition(menuLeft + 5, row);
            Console.ForegroundColor = canUse ? ConsoleColor.Gray : ConsoleColor.DarkGray;
            string desc = skill.Description;
            if (desc.Length > menuWidth - 8) desc = desc.Substring(0, menuWidth - 11) + "...";
            Console.Write(desc);
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.SetCursorPosition(menuLeft + menuWidth - 1, row);
            Console.Write("║");
            Console.ResetColor();

            row++;
        }

        // Bottom border
        Console.SetCursorPosition(menuLeft, row);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("╠" + new string('═', menuWidth - 2) + "╣");

        Console.SetCursorPosition(menuLeft, row + 1);
        Console.Write("║");
        Console.SetCursorPosition(menuLeft + 2, row + 1);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"Stamina: {game.PlayerStamina}/12");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write($"  |  [1-5] Use Skill  [ESC] Cancel");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.SetCursorPosition(menuLeft + menuWidth - 1, row + 1);
        Console.Write("║");

        Console.SetCursorPosition(menuLeft, row + 2);
        Console.Write("╚" + new string('═', menuWidth - 2) + "╝");
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
        Console.Write("║      AI DEBUG PANEL               ║");
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

    public void RenderCharacterPanel(RPGGame game)
    {
        int panelX = 82;
        int startY = 1;

        // Header
        Console.SetCursorPosition(panelX, startY);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("╔═══════════════════════════════════╗");
        Console.SetCursorPosition(panelX, startY + 1);
        Console.Write("║      CHARACTER SHEET              ║");
        Console.SetCursorPosition(panelX, startY + 2);
        Console.Write("╠═══════════════════════════════════╣");
        Console.ResetColor();

        // Stats
        Console.SetCursorPosition(panelX, startY + 3);
        Console.Write("║ LEVEL:                            ║");
        Console.SetCursorPosition(panelX, startY + 4);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"║    Level {game.PlayerLevel}  ({game.PlayerXP}/{game.XPForNextLevel} XP)           ║");
        Console.ResetColor();

        Console.SetCursorPosition(panelX, startY + 5);
        Console.Write("║ VITALITY:                         ║");
        Console.SetCursorPosition(panelX, startY + 6);
        Console.ForegroundColor = game.PlayerHP < game.MaxPlayerHP * 0.3 ? ConsoleColor.Red : ConsoleColor.Green;
        Console.Write($"║    HP: {game.PlayerHP,3}/{game.MaxPlayerHP,-3}                   ║");
        Console.ResetColor();

        Console.SetCursorPosition(panelX, startY + 7);
        Console.Write("║ COMBAT:                           ║");
        Console.SetCursorPosition(panelX, startY + 8);
        Console.Write($"║    STR: {game.PlayerStrength,2}  DEF: {game.PlayerDefense,2}             ║");
        Console.SetCursorPosition(panelX, startY + 9);
        Console.Write($"║    Victories: {game.CombatsWon,3}                 ║");

        // Inventory
        Console.SetCursorPosition(panelX, startY + 10);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(panelX, startY + 11);
        Console.Write("║ INVENTORY:                        ║");
        Console.SetCursorPosition(panelX, startY + 12);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"║    Gold: {game.PlayerGold,5}g                   ║");
        Console.ResetColor();
        Console.SetCursorPosition(panelX, startY + 13);
        Console.ForegroundColor = game.PotionCount > 0 ? ConsoleColor.Green : ConsoleColor.Red;
        Console.Write($"║    Potions: {game.PotionCount,2}                      ║");
        Console.ResetColor();

        // Equipment
        Console.SetCursorPosition(panelX, startY + 14);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(panelX, startY + 15);
        Console.Write("║ EQUIPMENT:                        ║");
        Console.SetCursorPosition(panelX, startY + 16);
        Console.ForegroundColor = ConsoleColor.Cyan;
        string weaponName = game.PlayerInventory.EquippedWeapon.Name;
        if (weaponName.Length > 32) weaponName = weaponName.Substring(0, 29) + "...";
        Console.Write($"║    {weaponName.PadRight(32)} ║");
        Console.ResetColor();
        Console.SetCursorPosition(panelX, startY + 17);
        Console.ForegroundColor = ConsoleColor.Green;
        string armorName = game.PlayerInventory.EquippedArmor.Name;
        if (armorName.Length > 32) armorName = armorName.Substring(0, 29) + "...";
        Console.Write($"║    {armorName.PadRight(32)} ║");
        Console.ResetColor();
        Console.SetCursorPosition(panelX, startY + 18);
        Console.Write($"║    +{game.GetEffectiveStrength() - game.PlayerStrength} STR  +{game.GetEffectiveDefense() - game.PlayerDefense} DEF                   ║");

        // Victory Progress
        Console.SetCursorPosition(panelX, startY + 19);
        Console.Write("╠═══════════════════════════════════╣");
        Console.SetCursorPosition(panelX, startY + 20);
        Console.Write("║ VICTORY PROGRESS:                 ║");
        Console.SetCursorPosition(panelX, startY + 21);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"║    Dungeons: {game.DungeonsCompleted,2}                   ║");
        Console.SetCursorPosition(panelX, startY + 22);
        string progress = game.GetVictoryProgress();
        if (progress.Length > 35) progress = progress.Substring(0, 32) + "...";
        Console.Write($"║    {progress.PadRight(35).Substring(0, 35)}║");
        Console.ResetColor();

        // Footer
        Console.SetCursorPosition(panelX, startY + 23);
        Console.Write("╚═══════════════════════════════════╝");
    }

    public void Cleanup()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ResetColor();
    }

    // NEW: Stat Allocation Screen
    public void RenderStatAllocationScreen(RPGGame game)
    {
        Console.Clear();
        Console.CursorVisible = false;

        int startY = 5;
        Console.SetCursorPosition(20, startY);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.SetCursorPosition(20, startY + 1);
        Console.WriteLine("║          LEVEL UP!                             ║");
        Console.SetCursorPosition(20, startY + 2);
        Console.WriteLine($"║   You have {game.AvailableStatPoints} stat points to allocate!        ║");
        Console.SetCursorPosition(20, startY + 3);
        Console.WriteLine("╠════════════════════════════════════════════════╣");
        Console.ResetColor();

        Console.SetCursorPosition(20, startY + 4);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 5);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write("║  [S] +1 STRENGTH");
        Console.ResetColor();
        Console.Write($"  (Current: {game.PlayerStrength,2})           ║");

        Console.SetCursorPosition(20, startY + 6);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("║      Increases damage dealt                   ║");
        Console.ResetColor();

        Console.SetCursorPosition(20, startY + 7);
        Console.WriteLine("║                                                ║");

        Console.SetCursorPosition(20, startY + 8);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("║  [D] +1 DEFENSE");
        Console.ResetColor();
        Console.Write($"   (Current: {game.PlayerDefense,2})           ║");

        Console.SetCursorPosition(20, startY + 9);
        Console.ForegroundColor = ConsoleColor.DarkGray;
        Console.WriteLine("║      Reduces damage taken                     ║");
        Console.ResetColor();

        Console.SetCursorPosition(20, startY + 10);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 11);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("║  [Q] Done allocating                           ║");
        Console.SetCursorPosition(20, startY + 12);
        Console.WriteLine("╚════════════════════════════════════════════════╝");
        Console.ResetColor();

        Console.SetCursorPosition(20, startY + 14);
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write("Current Stats: ");
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.Write($"STR:{game.PlayerStrength} ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write($"DEF:{game.PlayerDefense} ");
        Console.ForegroundColor = ConsoleColor.Red;
        Console.Write($"HP:{game.PlayerHP}/{game.MaxPlayerHP}");
        Console.ResetColor();
    }

    // NEW: Help Menu
    public void RenderHelpMenu(bool inDungeon)
    {
        Console.Clear();
        Console.CursorVisible = false;

        int startY = 2;
        Console.SetCursorPosition(15, startY);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔══════════════════════════════════════════════════════════╗");
        Console.SetCursorPosition(15, startY + 1);
        Console.WriteLine("║                    HELP MENU                             ║");
        Console.SetCursorPosition(15, startY + 2);
        Console.WriteLine("╠══════════════════════════════════════════════════════════╣");
        Console.ResetColor();

        string[] helpLines = {
            "║  MOVEMENT:                                               ║",
            "║    [N/S/E/W] or [Arrow Keys] - Move in world/dungeon    ║",
            "║    Terrain affects movement cost (Forest=2, Mountain=3) ║",
            "║                                                          ║",
            "║  COMBAT:                                                 ║",
            "║    [A] Attack - Deal damage to enemy                    ║",
            "║    [D] Defend - Block incoming damage                   ║",
            "║    [P] Potion - Heal +5 HP (if you have potions)        ║",
            "║    [F] Flee   - 50% chance to escape (takes damage)     ║",
            "║                                                          ║",
            "║  WORLD:                                                  ║",
            "║    [Enter] - Interact with Towns/Dungeons/Temple        ║",
            "║    [L] - Allocate stat points when leveling up          ║",
            "║    [I] - View character sheet & stats                   ║",
            "║    [H] - This help menu                                 ║",
            "║    [Q] - Quit game                                       ║",
            "║                                                          ║",
            "║  TEMPLE (+):                                             ║",
            "║    [P]ray - FREE healing from the gods!                 ║",
            "║    RESPAWN POINT - You return here when you die!        ║",
            "║    Walk over X to auto-retrieve dropped items           ║",
            "║                                                          ║",
            "║  TOWNS:                                                  ║",
            "║    [I]nn - Heal to full HP (costs 10g)                  ║",
            "║    [B]uy Potion - Buy potion (costs 5g)                 ║",
            "║    [X]it - Leave town                                   ║",
            "║                                                          ║",
            "║  DUNGEONS:                                               ║",
            "║    [D] Descend stairs (find > symbol)                   ║",
            "║    [X] Exit dungeon                                      ║",
            "║    Explore to find treasure $, avoid traps !            ║",
            "║    Defeat boss B to get artifact A                      ║",
            "║                                                          ║",
            "║  DEATH: Respawn at Temple, lose 50% gold & items!       ║",
            "║  VICTORY: Collect artifacts from 2 dungeons!             ║"
        };

        for (int i = 0; i < helpLines.Length; i++)
        {
            Console.SetCursorPosition(15, startY + 3 + i);
            Console.WriteLine(helpLines[i]);
        }

        Console.SetCursorPosition(15, startY + 3 + helpLines.Length);
        Console.WriteLine("╚══════════════════════════════════════════════════════════╝");

        Console.SetCursorPosition(15, startY + helpLines.Length + 5);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press any key to return to game...");
        Console.ResetColor();
    }

    // NEW: Death & Respawn Screen
    public void RenderDeathScreen(RPGGame game, string killerName, int goldLost, List<string> droppedItems)
    {
        Console.Clear();
        Console.CursorVisible = false;

        int startY = 5;
        Console.SetCursorPosition(25, startY);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine("╔═══════════════════════════════════════╗");
        Console.SetCursorPosition(25, startY + 1);
        Console.WriteLine("║          YOU DIED                 ║");
        Console.SetCursorPosition(25, startY + 2);
        Console.WriteLine("╠═══════════════════════════════════════╣");
        Console.ResetColor();

        Console.SetCursorPosition(25, startY + 3);
        Console.ForegroundColor = ConsoleColor.DarkRed;
        Console.WriteLine($"║  Slain by: {killerName,-25}  ║");
        Console.ResetColor();

        Console.SetCursorPosition(25, startY + 4);
        Console.WriteLine("║                                       ║");
        Console.SetCursorPosition(25, startY + 5);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("║  RESPAWNING AT TEMPLE...             ║");
        Console.ResetColor();

        Console.SetCursorPosition(25, startY + 6);
        Console.WriteLine("║                                       ║");
        Console.SetCursorPosition(25, startY + 7);
        Console.WriteLine("║  PENALTIES:                           ║");
        Console.SetCursorPosition(25, startY + 8);
        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine($"║    • Lost {goldLost,4}g (50% penalty)          ║");
        Console.ResetColor();

        int line = startY + 9;
        if (droppedItems.Count > 0)
        {
            Console.SetCursorPosition(25, line++);
            Console.ForegroundColor = ConsoleColor.DarkGray;
            Console.WriteLine("║    • Dropped equipment:               ║");
            foreach (var item in droppedItems.Take(2))
            {
                Console.SetCursorPosition(25, line++);
                Console.WriteLine($"║      - {item,-29}  ║");
            }
            Console.ResetColor();
        }

        Console.SetCursorPosition(25, line++);
        Console.WriteLine("║                                       ║");
        Console.SetCursorPosition(25, line++);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("║  KEPT:                                ║");
        Console.ResetColor();
        Console.SetCursorPosition(25, line++);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"║    • Level {game.PlayerLevel} & XP                       ║");
        Console.WriteLine($"║    • {game.PlayerGold}g remaining                   ║");
        Console.ResetColor();

        Console.SetCursorPosition(25, line++);
        Console.WriteLine("║                                       ║");
        Console.SetCursorPosition(25, line++);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("║  Return to X to retrieve items!       ║");
        Console.ResetColor();
        Console.SetCursorPosition(25, line++);
        Console.WriteLine("║                                       ║");
        Console.SetCursorPosition(25, line++);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"║  Total Deaths: {game.TotalDeaths,3}                    ║");
        Console.ResetColor();
        Console.SetCursorPosition(25, line++);
        Console.WriteLine("╚═══════════════════════════════════════╝");

        Console.SetCursorPosition(25, line + 2);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press any key to continue from Temple...");
        Console.ResetColor();
    }

    // NEW: Character Sheet (Full Screen)
    public void RenderFullCharacterSheet(RPGGame game)
    {
        Console.Clear();
        Console.CursorVisible = false;

        int startY = 2;
        Console.SetCursorPosition(20, startY);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine("╔════════════════════════════════════════════════╗");
        Console.SetCursorPosition(20, startY + 1);
        Console.WriteLine("║           CHARACTER SHEET                      ║");
        Console.SetCursorPosition(20, startY + 2);
        Console.WriteLine("╠════════════════════════════════════════════════╣");
        Console.ResetColor();

        // Stats
        Console.SetCursorPosition(20, startY + 3);
        Console.WriteLine($"║  LEVEL: {game.PlayerLevel}                                       ║");
        Console.SetCursorPosition(20, startY + 4);
        string hpBar = GenerateHPBar(game.PlayerHP, game.MaxPlayerHP, 15);
        Console.ForegroundColor = GetHPColor(game.PlayerHP, game.MaxPlayerHP);
        Console.Write($"║  HP: {hpBar}");
        Console.ResetColor();
        Console.WriteLine("              ║");
        Console.SetCursorPosition(20, startY + 5);
        Console.WriteLine($"║       {game.PlayerHP}/{game.MaxPlayerHP} HP                              ║");

        Console.SetCursorPosition(20, startY + 6);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 7);
        Console.WriteLine($"║  STRENGTH:  {game.GetEffectiveStrength(),2}  (Base: {game.PlayerStrength})                ║");
        Console.SetCursorPosition(20, startY + 8);
        Console.WriteLine($"║  DEFENSE:   {game.GetEffectiveDefense(),2}  (Base: {game.PlayerDefense})                 ║");

        Console.SetCursorPosition(20, startY + 9);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 10);
        Console.WriteLine("╠════════════════════════════════════════════════╣");

        // Progress
        Console.SetCursorPosition(20, startY + 11);
        Console.WriteLine("║  PROGRESS:                                     ║");
        Console.SetCursorPosition(20, startY + 12);
        string xpBar = GenerateHPBar(game.PlayerXP, game.XPForNextLevel, 15);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write($"║  XP: {xpBar}");
        Console.ResetColor();
        Console.WriteLine("              ║");
        Console.SetCursorPosition(20, startY + 13);
        Console.WriteLine($"║      {game.PlayerXP}/{game.XPForNextLevel} to next level                   ║");
        Console.SetCursorPosition(20, startY + 14);
        Console.WriteLine($"║  Gold: {game.PlayerGold,5}g                                ║");
        Console.SetCursorPosition(20, startY + 15);
        Console.WriteLine($"║  Victories: {game.CombatsWon,3}                              ║");

        Console.SetCursorPosition(20, startY + 16);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 17);
        Console.WriteLine("╠════════════════════════════════════════════════╣");

        // Equipment
        Console.SetCursorPosition(20, startY + 18);
        Console.WriteLine("║  EQUIPMENT:                                    ║");
        Console.SetCursorPosition(20, startY + 19);
        Console.ForegroundColor = ConsoleColor.Cyan;
        Console.WriteLine($"║    Weapon: {game.PlayerInventory.EquippedWeapon.Name,-28} ║");
        Console.ResetColor();
        Console.SetCursorPosition(20, startY + 20);
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"║    Armor:  {game.PlayerInventory.EquippedArmor.Name,-28} ║");
        Console.ResetColor();
        Console.SetCursorPosition(20, startY + 21);
        Console.WriteLine($"║    Potions: {game.PotionCount,2}                                 ║");

        Console.SetCursorPosition(20, startY + 22);
        Console.WriteLine("║                                                ║");
        Console.SetCursorPosition(20, startY + 23);
        Console.WriteLine("╠════════════════════════════════════════════════╣");

        // Quest
        Console.SetCursorPosition(20, startY + 24);
        Console.WriteLine("║  QUEST PROGRESS:                               ║");
        Console.SetCursorPosition(20, startY + 25);
        Console.ForegroundColor = game.DungeonsCompleted >= 2 ? ConsoleColor.Green : ConsoleColor.Yellow;
        Console.WriteLine($"║    Artifacts Collected: {game.DungeonsCompleted}/2                  ║");
        Console.ResetColor();
        Console.SetCursorPosition(20, startY + 26);
        Console.WriteLine($"║    {game.GetVictoryProgress(),-46} ║");

        Console.SetCursorPosition(20, startY + 27);
        Console.WriteLine("╚════════════════════════════════════════════════╝");

        Console.SetCursorPosition(20, startY + 29);
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine("Press any key to return to game...");
        Console.ResetColor();
    }
}



