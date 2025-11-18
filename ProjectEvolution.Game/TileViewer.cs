using Raylib_CsLo;
using System.Numerics;

namespace ProjectEvolution.Game;

/// <summary>
/// Utility to view the tileset and identify tile IDs
/// </summary>
public static class TileViewer
{
    private const int TILE_SIZE = 16;
    private const int TILE_SPACING = 1;
    private const int TILES_PER_ROW = 57;
    private const int SCALE = 2;

    public static void Run()
    {
        string tilesetPath = "Assets/Tilesets/roguelike-pack.png";

        // Get monitor size
        int monitor = Raylib.GetCurrentMonitor();
        int screenWidth = Raylib.GetMonitorWidth(monitor);
        int screenHeight = Raylib.GetMonitorHeight(monitor);

        Raylib.SetConfigFlags(ConfigFlags.FLAG_WINDOW_UNDECORATED);
        Raylib.InitWindow(screenWidth, screenHeight, "Tile Viewer - Press ESC to exit");
        Raylib.SetTargetFPS(60);

        if (!File.Exists(tilesetPath))
        {
            Console.WriteLine($"ERROR: Tileset not found at {tilesetPath}");
            Console.WriteLine("Run the game first to download the tileset.");
            Console.ReadKey();
            return;
        }

        var tileset = Raylib.LoadTexture(tilesetPath);

        // Calculate total tiles
        int tilesHigh = (tileset.height + TILE_SPACING) / (TILE_SIZE + TILE_SPACING);
        int totalTiles = TILES_PER_ROW * tilesHigh;

        int offsetX = 10;
        int offsetY = 10;
        int hoveredTileId = -1;
        bool showGrid = true;
        bool showIds = true;

        Console.WriteLine($"Tileset Viewer Started");
        Console.WriteLine($"Tileset: {tileset.width}x{tileset.height}");
        Console.WriteLine($"Tiles per row: {TILES_PER_ROW}");
        Console.WriteLine($"Rows: {tilesHigh}");
        Console.WriteLine($"Total tiles: {totalTiles}");
        Console.WriteLine($"\nControls:");
        Console.WriteLine($"  Mouse - Hover to see tile ID");
        Console.WriteLine($"  G - Toggle grid");
        Console.WriteLine($"  I - Toggle tile IDs");
        Console.WriteLine($"  ESC - Exit");

        while (!Raylib.WindowShouldClose())
        {
            // Input
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_G))
                showGrid = !showGrid;
            if (Raylib.IsKeyPressed(KeyboardKey.KEY_I))
                showIds = !showIds;

            // Get mouse position
            var mousePos = Raylib.GetMousePosition();
            int mouseTileX = (int)((mousePos.X - offsetX) / ((TILE_SIZE + TILE_SPACING) * SCALE));
            int mouseTileY = (int)((mousePos.Y - offsetY) / ((TILE_SIZE + TILE_SPACING) * SCALE));

            if (mouseTileX >= 0 && mouseTileX < TILES_PER_ROW && mouseTileY >= 0 && mouseTileY < tilesHigh)
            {
                hoveredTileId = mouseTileY * TILES_PER_ROW + mouseTileX;
            }
            else
            {
                hoveredTileId = -1;
            }

            // Render
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color(30, 30, 30, 255));

            // Draw title
            Raylib.DrawText("TILE VIEWER - Kenney Roguelike Pack", 10, screenHeight - 40, 20, Raylib.WHITE);
            Raylib.DrawText($"[G] Grid: {(showGrid ? "ON" : "OFF")}  [I] IDs: {(showIds ? "ON" : "OFF")}  [ESC] Exit", 10, screenHeight - 20, 16, Raylib.GRAY);

            // Draw all tiles
            for (int row = 0; row < tilesHigh; row++)
            {
                for (int col = 0; col < TILES_PER_ROW; col++)
                {
                    int tileId = row * TILES_PER_ROW + col;

                    // Source rectangle in tileset
                    Rectangle source = new Rectangle
                    {
                        x = col * (TILE_SIZE + TILE_SPACING),
                        y = row * (TILE_SIZE + TILE_SPACING),
                        width = TILE_SIZE,
                        height = TILE_SIZE
                    };

                    // Destination rectangle on screen
                    Rectangle dest = new Rectangle
                    {
                        x = offsetX + col * (TILE_SIZE + TILE_SPACING) * SCALE,
                        y = offsetY + row * (TILE_SIZE + TILE_SPACING) * SCALE,
                        width = TILE_SIZE * SCALE,
                        height = TILE_SIZE * SCALE
                    };

                    // Draw tile
                    Raylib.DrawTexturePro(tileset, source, dest, Vector2.Zero, 0f, Raylib.WHITE);

                    // Highlight hovered tile
                    if (tileId == hoveredTileId)
                    {
                        Raylib.DrawRectangleLinesEx(dest, 2, Raylib.YELLOW);

                        // Show info for hovered tile
                        string info = $"Tile ID: {tileId} | Row: {row} | Col: {col} | Pos: ({source.x}, {source.y})";
                        Raylib.DrawText(info, 10, 10, 24, Raylib.YELLOW);
                    }

                    // Draw grid
                    if (showGrid)
                    {
                        Raylib.DrawRectangleLines((int)dest.x, (int)dest.y, (int)dest.width, (int)dest.height, new Color(100, 100, 100, 100));
                    }

                    // Draw tile IDs
                    if (showIds && SCALE >= 2)
                    {
                        string idText = tileId.ToString();
                        int textWidth = Raylib.MeasureText(idText, 8);
                        Raylib.DrawText(idText, (int)(dest.x + (dest.width - textWidth) / 2), (int)(dest.y + dest.height - 10), 8, new Color(0, 0, 0, 150));
                    }
                }
            }

            Raylib.EndDrawing();
        }

        Raylib.UnloadTexture(tileset);
        Raylib.CloseWindow();
    }
}
