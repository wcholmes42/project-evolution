using Raylib_CsLo;
using System.Numerics;

namespace ProjectEvolution.Game;

/// <summary>
/// Generates Ultima IV-style tiles procedurally using simple geometric shapes.
/// NO external dependencies - pure code!
/// </summary>
public static class ProceduralTileGenerator
{
    private const int TILE_SIZE = 16;
    private const int TILE_SPACING = 1;
    private const int TILES_PER_ROW = 57;
    private const int TILES_PER_COLUMN = 31;

    // Authentic Ultima IV EGA color palette (using UltimaIVPalette)
    private static class Colors
    {
        // Terrain - Using authentic EGA colors
        public static readonly Color Grass = UltimaIVPalette.Terrain.Grass;
        public static readonly Color GrassDark = UltimaIVPalette.Terrain.GrassDark;
        public static readonly Color Water = UltimaIVPalette.Terrain.DeepWater;
        public static readonly Color WaterLight = UltimaIVPalette.Terrain.ShallowWater;
        public static readonly Color Mountain = UltimaIVPalette.Terrain.Mountain;
        public static readonly Color MountainSnow = UltimaIVPalette.Terrain.MountainSnow;
        public static readonly Color Forest = UltimaIVPalette.Terrain.Forest;
        public static readonly Color ForestTree = UltimaIVPalette.Terrain.ForestDark;

        // Structures
        public static readonly Color TownWall = UltimaIVPalette.Structures.TownWall;
        public static readonly Color TownRoof = UltimaIVPalette.Structures.TownRoof;
        public static readonly Color TempleGold = UltimaIVPalette.Structures.Temple;
        public static readonly Color TempleWhite = UltimaIVPalette.Structures.TempleAccent;
        public static readonly Color DungeonWall = UltimaIVPalette.Structures.Dungeon;
        public static readonly Color DungeonFloor = UltimaIVPalette.Structures.DungeonDark;

        // Characters
        public static readonly Color Player = UltimaIVPalette.Characters.Avatar;
        public static readonly Color PlayerArmor = UltimaIVPalette.Characters.AvatarCloak;

        // Enemies
        public static readonly Color Goblin = UltimaIVPalette.Monsters.Goblin;
        public static readonly Color Undead = UltimaIVPalette.Monsters.Undead;
        public static readonly Color Demon = UltimaIVPalette.Monsters.Demon;
        public static readonly Color Beast = UltimaIVPalette.Monsters.Beast;

        // Items
        public static readonly Color PotionRed = UltimaIVPalette.Items.Potion;
        public static readonly Color Gold = UltimaIVPalette.Items.Gold;
        public static readonly Color Chest = UltimaIVPalette.Items.Chest;
    }

    /// <summary>
    /// Generate the complete tileset texture (Ultima IV style)
    /// </summary>
    public static Texture GenerateTileset()
    {
        int textureWidth = TILES_PER_ROW * (TILE_SIZE + TILE_SPACING);
        int textureHeight = TILES_PER_COLUMN * (TILE_SIZE + TILE_SPACING);

        // Create render texture to draw tiles into
        var renderTexture = Raylib.LoadRenderTexture(textureWidth, textureHeight);

        Raylib.BeginTextureMode(renderTexture);
        Raylib.ClearBackground(new Color((byte)0, (byte)0, (byte)0, (byte)255));

        Console.WriteLine($"Generating tileset: {textureWidth}x{textureHeight} pixels");
        Console.WriteLine($"  Tiles: {TILES_PER_ROW}x{TILES_PER_COLUMN} = {TILES_PER_ROW * TILES_PER_COLUMN} total");
        Console.WriteLine($"  Tile size: {TILE_SIZE}x{TILE_SIZE} + {TILE_SPACING}px spacing");

        // DEBUG: Draw a test pattern to verify rendering works
        Console.WriteLine("  - Drawing test pattern...");
        for (int i = 0; i < 10; i++)
        {
            Raylib.DrawRectangle(i * 20, 0, 16, 16, Raylib.RED);
            Raylib.DrawRectangle(i * 20, 20, 16, 16, Raylib.GREEN);
            Raylib.DrawRectangle(i * 20, 40, 16, 16, Raylib.BLUE);
        }

        Console.WriteLine("Generating tiles...");

        // Generate all tiles (using TileMapper IDs for consistency)
        Console.WriteLine("  - Terrain tiles (0-3)...");
        GenerateTerrainTiles();

        Console.WriteLine("  - Structure tiles (10-12)...");
        GenerateStructureTiles();

        Console.WriteLine("  - Character tiles (20-21)...");
        GenerateCharacterTiles();

        Console.WriteLine("  - Enemy tiles (30-33)...");
        GenerateEnemyTiles();

        Console.WriteLine("  - Item tiles (40-42)...");
        GenerateItemTiles();

        Console.WriteLine("  - Dungeon tiles (50-52)...");
        GenerateDungeonTiles();

        Console.WriteLine("Tiles generated!");

        Raylib.EndTextureMode();

        // IMPORTANT: Get the texture before unloading render texture
        Texture result = renderTexture.texture;

        // NOTE: Don't unload render texture here - we're using its internal texture
        // Raylib.UnloadRenderTexture(renderTexture); // This would invalidate the texture!

        return result;
    }

    private static void GenerateTerrainTiles()
    {
        // Grassland (tile 0) - BRIGHT GREEN for visibility testing
        DrawTileAt(0, (tileX, tileY) =>
        {
            // Fill entire tile with bright green
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Grass);
            // Border to verify exact tile bounds
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Forest (tile 1) - DARK GREEN filled
        DrawTileAt(1, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Forest);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Yellow);
        });

        // Mountain (tile 2) - GRAY filled
        DrawTileAt(2, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Mountain);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Water (tile 3) - BLUE filled
        DrawTileAt(3, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Water);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.BrightCyan);
        });
    }

    private static void GenerateStructureTiles()
    {
        // Town (tile 10) - BROWN filled
        DrawTileAt(10, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.TownWall);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.TownRoof);
        });

        // Temple (tile 11) - BRIGHT YELLOW/GOLD filled
        DrawTileAt(11, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.TempleGold);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.TempleWhite);
        });

        // Dungeon entrance (tile 12) - DARK filled
        DrawTileAt(12, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.DarkGray);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Red);
        });
    }

    private static void GenerateCharacterTiles()
    {
        // Player (tile 20) - BRIGHT WHITE/RED for maximum visibility
        DrawTileAt(20, (tileX, tileY) =>
        {
            // Fill with bright red background for testing
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.PlayerArmor);
            // White center to stand out
            Raylib.DrawRectangle(tileX + 4, tileY + 4, 8, 8, Colors.Player);
            // Border
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Yellow);
        });

        // NPC (tile 21) - CYAN for visibility
        DrawTileAt(21, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Cyan);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });
    }

    private static void GenerateEnemyTiles()
    {
        // Goblin (tile 30) - BRIGHT GREEN filled
        DrawTileAt(30, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Goblin);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Undead (tile 31) - LIGHT GRAY filled
        DrawTileAt(31, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Undead);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Demon (tile 32) - BRIGHT RED filled
        DrawTileAt(32, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Demon);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Yellow);
        });

        // Beast (tile 33) - BROWN filled
        DrawTileAt(33, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Beast);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });
    }

    private static void GenerateItemTiles()
    {
        // Potion (tile 40) - BRIGHT MAGENTA filled
        DrawTileAt(40, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.PotionRed);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Gold (tile 41) - BRIGHT YELLOW filled
        DrawTileAt(41, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Gold);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });

        // Treasure chest (tile 42) - BROWN filled
        DrawTileAt(42, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Chest);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Gold);
        });
    }

    private static void GenerateDungeonTiles()
    {
        // Dungeon wall (tile 50) - DARK GRAY filled
        DrawTileAt(50, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonWall);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.LightGray);
        });

        // Dungeon floor (tile 51) - BLACK filled
        DrawTileAt(51, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.DarkGray);
        });

        // Stairs down (tile 52) - YELLOW ">" symbol on dark
        DrawTileAt(52, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            // Large visible ">" shape
            Raylib.DrawRectangle(tileX + 4, tileY + 4, 8, 8, UltimaIVPalette.Yellow);
            Raylib.DrawRectangleLines(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.White);
        });
    }

    /// <summary>
    /// Helper to draw a tile at a specific tile ID position
    /// </summary>
    private static void DrawTileAt(int tileId, Action<int, int> drawAction)
    {
        int tileRow = tileId / TILES_PER_ROW;
        int tileCol = tileId % TILES_PER_ROW;

        int tileX = tileCol * (TILE_SIZE + TILE_SPACING);
        int tileY = tileRow * (TILE_SIZE + TILE_SPACING);

        Console.WriteLine($"    Drawing tile {tileId} at ({tileX}, {tileY}) [row {tileRow}, col {tileCol}]");

        drawAction(tileX, tileY);
    }

    /// <summary>
    /// Save generated tileset to disk for caching
    /// </summary>
    public static void SaveTileset(Texture tileset, string path)
    {
        // Ensure directory exists
        string? directory = Path.GetDirectoryName(path);
        if (directory != null && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Export texture as PNG
        var image = Raylib.LoadImageFromTexture(tileset);

        // CRITICAL: Flip Y-axis because Raylib textures are upside down when saved!
        unsafe
        {
            Raylib.ImageFlipVertical(&image);
        }

        Raylib.ExportImage(image, path);
        Raylib.UnloadImage(image);

        Console.WriteLine($"✓ Saved procedural tileset to: {path}");
    }
}
