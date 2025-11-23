using Raylib_CsLo;
using System.Numerics;

namespace ProjectEvolution.Game;

/// <summary>
/// Graphics renderer using Raylib for tile-based rendering (Ultima 4 style)
/// </summary>
public class GraphicsRenderer : IDisposable
{
    private const int TILE_SIZE = 16;
    private const int TILE_SPACING = 1; // 1px margin between tiles
    private const int TILES_PER_ROW = 57; // Kenney's roguelike pack: 968px / (16 + 1) = 57 tiles per row
    private int SCREEN_WIDTH;
    private int SCREEN_HEIGHT;
    private const int SCALE = 3; // 3x scaling for better visibility on modern displays
    private const int SCALED_TILE_SIZE = TILE_SIZE * SCALE;

    private Texture tileset;
    private bool initialized = false;
    private string cacheDir = "Assets/Generated";
    private string cachedTilesetPath = "Assets/Generated/procedural_tileset.png";

    public GraphicsRenderer()
    {
        // No external dependencies - we generate our own tiles!
    }

    /// <summary>
    /// Initialize the Raylib window and load/generate tileset
    /// </summary>
    public void Initialize(bool fullscreen = true)
    {
        // GENERATION 47: EXCLUSIVE FULLSCREEN MODE!
        SCREEN_WIDTH = 1920;
        SCREEN_HEIGHT = 1080;

        if (fullscreen)
        {
            // Exclusive fullscreen - best performance and immersion
            Raylib.SetConfigFlags(ConfigFlags.FLAG_FULLSCREEN_MODE | ConfigFlags.FLAG_VSYNC_HINT);
            Console.WriteLine("🎮 Initializing EXCLUSIVE FULLSCREEN mode...");
        }
        else
        {
            // Windowed mode for development/testing
            Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_RESIZABLE | ConfigFlags.FLAG_WINDOW_MAXIMIZED);
            Console.WriteLine("🪟 Initializing windowed mode...");
        }

        Raylib.InitWindow(SCREEN_WIDTH, SCREEN_HEIGHT, "Project Evolution - Gen 47 (Ultima IV Style!)");
        Raylib.SetTargetFPS(60);

        // Get actual screen dimensions after window creation
        SCREEN_WIDTH = Raylib.GetScreenWidth();
        SCREEN_HEIGHT = Raylib.GetScreenHeight();

        Console.WriteLine($"✓ Window created at {SCREEN_WIDTH}x{SCREEN_HEIGHT}");

        // Load or generate tileset (Ultima IV style!)
        if (File.Exists(cachedTilesetPath))
        {
            Console.WriteLine("📦 Loading cached procedural tileset...");
            tileset = Raylib.LoadTexture(cachedTilesetPath);
            Console.WriteLine($"✓ Tileset loaded from cache: {tileset.width}x{tileset.height}");
        }
        else
        {
            Console.WriteLine("🎨 Generating Ultima IV-style tileset procedurally...");
            Console.WriteLine("   (This only happens once - will be cached!)");

            var startTime = DateTime.Now;
            tileset = ProceduralTileGenerator.GenerateTileset();
            var elapsed = (DateTime.Now - startTime).TotalMilliseconds;

            Console.WriteLine($"✓ Tileset generated in {elapsed:F0}ms: {tileset.width}x{tileset.height}");

            // Save to cache for future runs
            ProceduralTileGenerator.SaveTileset(tileset, cachedTilesetPath);
        }

        initialized = true;

        Console.WriteLine($"✓ Graphics initialized: {SCREEN_WIDTH}x{SCREEN_HEIGHT}");
        Console.WriteLine($"✓ Tile size: {TILE_SIZE}x{TILE_SIZE} + {TILE_SPACING}px spacing (scaled {SCALE}x)");
        Console.WriteLine($"✓ Tiles per row: {TILES_PER_ROW}");
        Console.WriteLine("✓ NO EXTERNAL DEPENDENCIES - Pure procedural generation!");
    }

    /// <summary>
    /// Begin a new frame
    /// </summary>
    public void BeginFrame()
    {
        Raylib.BeginDrawing();
        Raylib.ClearBackground(Raylib.BLACK);
    }

    /// <summary>
    /// End the current frame
    /// </summary>
    public void EndFrame()
    {
        Raylib.EndDrawing();
    }

    /// <summary>
    /// Draw a single tile from the tileset at pixel coordinates
    /// </summary>
    /// <param name="tileId">The tile ID (0-based index in the spritesheet)</param>
    /// <param name="pixelX">Screen X position (in pixels)</param>
    /// <param name="pixelY">Screen Y position (in pixels)</param>
    /// <param name="tint">Optional color tint (default: white)</param>
    public void DrawTileAt(int tileId, int pixelX, int pixelY, Color? tint = null)
    {
        if (!initialized) return;

        // Calculate source position in the tileset (accounting for 1px spacing)
        int tileRow = tileId / TILES_PER_ROW;
        int tileCol = tileId % TILES_PER_ROW;

        Rectangle source = new Rectangle
        {
            x = tileCol * (TILE_SIZE + TILE_SPACING),
            y = tileRow * (TILE_SIZE + TILE_SPACING),
            width = TILE_SIZE,
            height = TILE_SIZE
        };

        Rectangle dest = new Rectangle
        {
            x = pixelX,
            y = pixelY,
            width = SCALED_TILE_SIZE,
            height = SCALED_TILE_SIZE
        };

        Raylib.DrawTexturePro(tileset, source, dest, Vector2.Zero, 0f, tint ?? Raylib.WHITE);
    }

    /// <summary>
    /// Draw a single tile from the tileset at the specified screen position (in tiles)
    /// </summary>
    [Obsolete("Use DrawTileAt with pixel coordinates instead")]
    public void DrawTile(int tileId, int x, int y, Color? tint = null)
    {
        DrawTileAt(tileId, x * SCALED_TILE_SIZE, y * SCALED_TILE_SIZE, tint);
    }

    /// <summary>
    /// Draw the game map
    /// </summary>
    public void DrawMap(RPGGame game)
    {
        // Calculate optimal viewport size based on screen (leave room for UI on right)
        int uiWidth = 400;  // Pixels for UI panel
        int mapOffsetX = 10;  // Left margin
        int mapOffsetY = 10;  // Top margin

        int viewPixelsWidth = SCREEN_WIDTH - uiWidth - mapOffsetX - 20;  // Remaining space for map
        int viewPixelsHeight = SCREEN_HEIGHT - mapOffsetY - 20;

        int viewWidth = Math.Max(10, viewPixelsWidth / SCALED_TILE_SIZE);
        int viewHeight = Math.Max(10, viewPixelsHeight / SCALED_TILE_SIZE);

        // Calculate viewport centered on player
        int startX = Math.Max(0, game.PlayerX - viewWidth / 2);
        int startY = Math.Max(0, game.PlayerY - viewHeight / 2);
        int endX = Math.Min(game.WorldWidth, startX + viewWidth);
        int endY = Math.Min(game.WorldHeight, startY + viewHeight);

        // Adjust if we're at the edge
        if (endX - startX < viewWidth)
            startX = Math.Max(0, endX - viewWidth);
        if (endY - startY < viewHeight)
            startY = Math.Max(0, endY - viewHeight);

        // Draw map background
        Raylib.DrawRectangle(mapOffsetX, mapOffsetY, viewWidth * SCALED_TILE_SIZE, viewHeight * SCALED_TILE_SIZE, Raylib.BLACK);

        // Draw terrain
        for (int worldY = startY; worldY < endY; worldY++)
        {
            for (int worldX = startX; worldX < endX; worldX++)
            {
                int screenX = worldX - startX;
                int screenY = worldY - startY;

                string terrain = game.GetTerrainAt(worldX, worldY);
                int tileId = TileMapper.GetTerrainTileId(terrain);
                DrawTileAt(tileId, mapOffsetX + screenX * SCALED_TILE_SIZE, mapOffsetY + screenY * SCALED_TILE_SIZE);
            }
        }

        // Draw mobs
        var mobs = game.GetAllWorldMobs();
        foreach (var mob in mobs)
        {
            if (mob.X >= startX && mob.X < endX && mob.Y >= startY && mob.Y < endY)
            {
                int screenX = mob.X - startX;
                int screenY = mob.Y - startY;
                int mobTileId = TileMapper.GetMobTileId(mob.Type);
                DrawTileAt(mobTileId, mapOffsetX + screenX * SCALED_TILE_SIZE, mapOffsetY + screenY * SCALED_TILE_SIZE);
            }
        }

        // Draw player
        int playerScreenX = game.PlayerX - startX;
        int playerScreenY = game.PlayerY - startY;
        DrawTileAt(TileMapper.PLAYER_TILE, mapOffsetX + playerScreenX * SCALED_TILE_SIZE, mapOffsetY + playerScreenY * SCALED_TILE_SIZE);

        // Draw UI overlay
        DrawUI(game, viewPixelsWidth + mapOffsetX + 20, mapOffsetY);
    }

    /// <summary>
    /// Draw UI overlay (health, gold, etc.)
    /// </summary>
    private void DrawUI(RPGGame game, int uiX, int uiY)
    {
        int lineHeight = 25;

        // Draw panel background
        Raylib.DrawRectangle(uiX - 10, 10, 300, 400, new Color(0, 0, 0, 180));
        Raylib.DrawRectangleLines(uiX - 10, 10, 300, 400, Raylib.GOLD);

        // Draw stats
        Raylib.DrawText($"PROJECT EVOLUTION", uiX, uiY, 20, Raylib.GOLD);
        uiY += lineHeight + 10;

        Raylib.DrawText($"Level: {game.PlayerLevel}", uiX, uiY, 20, Raylib.WHITE);
        uiY += lineHeight;

        // Health bar
        Raylib.DrawText($"HP: {game.PlayerHP}/{game.MaxPlayerHP}", uiX, uiY, 20, Raylib.RED);
        DrawBar(uiX, uiY + 22, 200, 10, game.PlayerHP, game.MaxPlayerHP, Raylib.RED, Raylib.DARKGRAY);
        uiY += lineHeight + 15;

        // XP bar
        int xpToNext = game.XPForNextLevel - game.PlayerXP;
        Raylib.DrawText($"XP: {game.PlayerXP}/{game.XPForNextLevel}", uiX, uiY, 20, Raylib.SKYBLUE);
        DrawBar(uiX, uiY + 22, 200, 10, game.PlayerXP, game.XPForNextLevel, Raylib.SKYBLUE, Raylib.DARKGRAY);
        uiY += lineHeight + 15;

        Raylib.DrawText($"Gold: {game.PlayerGold}g", uiX, uiY, 20, Raylib.YELLOW);
        uiY += lineHeight;

        Raylib.DrawText($"Potions: {game.PotionCount}", uiX, uiY, 20, Raylib.GREEN);
        uiY += lineHeight;

        Raylib.DrawText($"STR: {game.PlayerStrength}", uiX, uiY, 20, Raylib.ORANGE);
        uiY += lineHeight;

        Raylib.DrawText($"DEF: {game.PlayerDefense}", uiX, uiY, 20, Raylib.BLUE);
        uiY += lineHeight;

        uiY += 10;
        Raylib.DrawText($"Position: ({game.PlayerX},{game.PlayerY})", uiX, uiY, 16, Raylib.GRAY);
        uiY += lineHeight;

        string terrain = game.GetCurrentTerrain();
        Raylib.DrawText($"Terrain: {terrain}", uiX, uiY, 16, Raylib.GRAY);
        uiY += lineHeight + 10;

        // Controls
        Raylib.DrawText("--- CONTROLS ---", uiX, uiY, 16, Raylib.GOLD);
        uiY += lineHeight;
        Raylib.DrawText("Arrows: Move", uiX, uiY, 14, Raylib.LIGHTGRAY);
        uiY += 20;
        Raylib.DrawText("Enter: Interact", uiX, uiY, 14, Raylib.LIGHTGRAY);
        uiY += 20;
        Raylib.DrawText("P: Use Potion", uiX, uiY, 14, Raylib.LIGHTGRAY);
        uiY += 20;
        Raylib.DrawText("I: Character", uiX, uiY, 14, Raylib.LIGHTGRAY);
        uiY += 20;
        Raylib.DrawText("F12: Screenshot", uiX, uiY, 14, Raylib.YELLOW);
        uiY += 20;
        Raylib.DrawText("ESC: Quit", uiX, uiY, 14, Raylib.LIGHTGRAY);
    }

    /// <summary>
    /// Draw a progress bar
    /// </summary>
    private void DrawBar(int x, int y, int width, int height, int current, int max, Color fillColor, Color bgColor)
    {
        Raylib.DrawRectangle(x, y, width, height, bgColor);
        if (max > 0)
        {
            int fillWidth = (int)((float)current / max * width);
            Raylib.DrawRectangle(x, y, fillWidth, height, fillColor);
        }
        Raylib.DrawRectangleLines(x, y, width, height, Raylib.WHITE);
    }

    /// <summary>
    /// Check if the window should close
    /// </summary>
    public bool ShouldClose()
    {
        return Raylib.WindowShouldClose();
    }

    /// <summary>
    /// Check if a key was pressed this frame
    /// </summary>
    public bool IsKeyPressed(KeyboardKey key)
    {
        return Raylib.IsKeyPressed(key);
    }

    /// <summary>
    /// Get current screen width
    /// </summary>
    public int GetScreenWidth()
    {
        return SCREEN_WIDTH;
    }

    /// <summary>
    /// Get current screen height
    /// </summary>
    public int GetScreenHeight()
    {
        return SCREEN_HEIGHT;
    }

    /// <summary>
    /// Take a screenshot and save to file (for debugging)
    /// Call this when F12 is pressed
    /// </summary>
    public void TakeScreenshot(string filename = "")
    {
        if (string.IsNullOrEmpty(filename))
        {
            filename = $"screenshot_{DateTime.Now:yyyyMMdd_HHmmss}.png";
        }

        string path = Path.Combine("Screenshots", filename);

        // Ensure directory exists
        if (!Directory.Exists("Screenshots"))
        {
            Directory.CreateDirectory("Screenshots");
        }

        // Capture current framebuffer
        Raylib.TakeScreenshot(path);

        Console.WriteLine($"📸 Screenshot saved to: {path}");
    }

    /// <summary>
    /// Cleanup resources
    /// </summary>
    public void Dispose()
    {
        if (initialized)
        {
            Raylib.UnloadTexture(tileset);
            Raylib.CloseWindow();
            initialized = false;
        }
    }
}
