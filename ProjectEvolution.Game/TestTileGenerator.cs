using Raylib_CsLo;

namespace ProjectEvolution.Game;

/// <summary>
/// Quick test program to verify procedural tile generation works
/// </summary>
public static class TestTileGenerator
{
    public static void Main(string[] args)
    {
        Console.WriteLine("Testing Procedural Tile Generator...");
        Console.WriteLine("Initializing Raylib...");

        // Initialize window (small test window)
        Raylib.InitWindow(800, 600, "Tile Generator Test");
        Raylib.SetTargetFPS(60);

        Console.WriteLine("Generating tileset...");
        var tileset = ProceduralTileGenerator.GenerateTileset();

        Console.WriteLine($"Tileset created: {tileset.width}x{tileset.height}");
        Console.WriteLine($"Tileset ID: {tileset.id}");

        // Test: Draw the tileset directly to screen
        while (!Raylib.WindowShouldClose())
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            // Draw some text
            Raylib.DrawText("Procedural Tileset Test", 10, 10, 20, Raylib.WHITE);
            Raylib.DrawText("If you see colored tiles below, generation works!", 10, 40, 16, Raylib.GRAY);
            Raylib.DrawText("ESC to quit", 10, 560, 16, Raylib.LIGHTGRAY);

            // Draw first 10 tiles from tileset (should show grass, forest, mountain, etc.)
            for (int i = 0; i < 10; i++)
            {
                int tileRow = i / 57;
                int tileCol = i % 57;

                Rectangle source = new Rectangle
                {
                    x = tileCol * 17,  // 16 + 1 spacing
                    y = tileRow * 17,
                    width = 16,
                    height = 16
                };

                Rectangle dest = new Rectangle
                {
                    x = 10 + (i * 50),
                    y = 100,
                    width = 48,  // 3x scale
                    height = 48
                };

                Raylib.DrawTexturePro(tileset, source, dest, System.Numerics.Vector2.Zero, 0f, Raylib.WHITE);

                // Label each tile
                Raylib.DrawText($"Tile {i}", 10 + (i * 50), 150, 10, Raylib.YELLOW);
            }

            Raylib.EndDrawing();
        }

        Raylib.CloseWindow();
        Console.WriteLine("Test complete!");
    }
}
