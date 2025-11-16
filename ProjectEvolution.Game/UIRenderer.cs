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
        Console.Write($"📍 Position: ({game.PlayerX,2},{game.PlayerY,2})  ");

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
    }

    public void RenderMap(RPGGame game)
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
                    Console.Write("   "); // Out of bounds
                    continue;
                }

                // Center is player
                if (dx == 0 && dy == 0)
                {
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.Write(" @ ");
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
                        "Dungeon" => ('Ω', ConsoleColor.Red),
                        _ => ('.', ConsoleColor.Gray)
                    };

                    Console.ForegroundColor = color;
                    Console.Write($" {symbol} ");
                    Console.ResetColor();
                }
            }
        }

        // Legend
        Console.SetCursorPosition(2, startRow + 8);
        Console.Write("Legend: @ = You  ■ = Town  Ω = Dungeon  ♣ = Forest  ▲ = Mountain  . = Grass");
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

        for (int i = 0; i < MaxMessages; i++)
        {
            Console.SetCursorPosition(2, startRow + i);
            Console.Write(new string(' ', 76)); // Clear

            if (i < _messageLog.Count)
            {
                Console.SetCursorPosition(2, startRow + i);
                string msg = _messageLog[_messageLog.Count - MaxMessages + i];
                if (msg.Length > 76) msg = msg.Substring(0, 73) + "...";

                // Color code messages
                if (msg.Contains("Victory") || msg.Contains("Found"))
                    Console.ForegroundColor = ConsoleColor.Green;
                else if (msg.Contains("TRAP") || msg.Contains("damage"))
                    Console.ForegroundColor = ConsoleColor.Red;
                else if (msg.Contains("LEVEL UP"))
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
            Console.Write("[R] Roll Room  [D] Descend  [X] Exit Dungeon  [P] Use Potion");
        }
        Console.ResetColor();
    }

    public void Cleanup()
    {
        Console.Clear();
        Console.CursorVisible = true;
        Console.ResetColor();
    }
}
