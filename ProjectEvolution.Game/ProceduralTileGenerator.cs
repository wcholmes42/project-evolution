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

    // Ultima IV-inspired color palette
    private static class Colors
    {
        // Terrain
        public static readonly Color Grass = new Color(68, 154, 42, 255);      // Vibrant green
        public static readonly Color GrassDark = new Color(48, 124, 32, 255);  // Darker green for texture
        public static readonly Color Water = new Color(42, 100, 234, 255);     // Blue
        public static readonly Color WaterLight = new Color(62, 120, 254, 255); // Light blue for waves
        public static readonly Color Mountain = new Color(120, 100, 80, 255);  // Brown-gray
        public static readonly Color MountainSnow = new Color(240, 240, 240, 255); // Snow cap
        public static readonly Color Forest = new Color(34, 94, 24, 255);      // Dark green
        public static readonly Color ForestTree = new Color(54, 44, 24, 255);  // Tree trunk brown

        // Structures
        public static readonly Color TownWall = new Color(160, 120, 80, 255);  // Brown
        public static readonly Color TownRoof = new Color(180, 60, 40, 255);   // Red roof
        public static readonly Color TempleGold = new Color(255, 215, 0, 255); // Gold
        public static readonly Color TempleWhite = new Color(240, 240, 240, 255);
        public static readonly Color DungeonWall = new Color(60, 60, 60, 255); // Dark gray
        public static readonly Color DungeonFloor = new Color(40, 40, 40, 255); // Darker gray

        // Characters
        public static readonly Color Player = new Color(255, 200, 100, 255);   // Golden hero
        public static readonly Color PlayerArmor = new Color(200, 160, 80, 255);

        // Enemies (simple colors like Ultima IV)
        public static readonly Color Goblin = new Color(100, 180, 80, 255);    // Green
        public static readonly Color Undead = new Color(200, 200, 220, 255);   // Pale
        public static readonly Color Demon = new Color(200, 60, 60, 255);      // Red
        public static readonly Color Beast = new Color(140, 100, 60, 255);     // Brown

        // Items
        public static readonly Color PotionRed = new Color(220, 60, 60, 255);
        public static readonly Color Gold = new Color(255, 215, 0, 255);
        public static readonly Color Chest = new Color(139, 90, 43, 255);
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
        Raylib.ClearBackground(Raylib.BLACK);

        // Generate all tiles (using TileMapper IDs for consistency)
        GenerateTerrainTiles();
        GenerateStructureTiles();
        GenerateCharacterTiles();
        GenerateEnemyTiles();
        GenerateItemTiles();
        GenerateDungeonTiles();

        Raylib.EndTextureMode();

        // Convert render texture to regular texture
        return renderTexture.texture;
    }

    private static void GenerateTerrainTiles()
    {
        // Grassland (tile 0) - Simple solid green with texture dots
        DrawTileAt(0, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Grass);
            // Add grass texture (random-looking but deterministic)
            for (int i = 0; i < 8; i++)
            {
                int px = tileX + (i * 7) % TILE_SIZE;
                int py = tileY + (i * 11) % TILE_SIZE;
                Raylib.DrawPixel(px, py, Colors.GrassDark);
            }
        });

        // Forest (tile 1) - Dark green with simple tree
        DrawTileAt(1, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Forest);
            // Tree trunk (vertical line)
            Raylib.DrawRectangle(tileX + 7, tileY + 8, 2, 6, Colors.ForestTree);
            // Tree crown (triangle approximation)
            Raylib.DrawRectangle(tileX + 4, tileY + 4, 8, 2, Colors.Grass);
            Raylib.DrawRectangle(tileX + 5, tileY + 6, 6, 2, Colors.Grass);
        });

        // Mountain (tile 2) - Gray triangle (peak)
        DrawTileAt(2, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Mountain);
            // Triangle peak (draw progressively smaller lines)
            for (int row = 0; row < 8; row++)
            {
                int width = 16 - (row * 2);
                int xOffset = row;
                Raylib.DrawRectangle(tileX + xOffset, tileY + row, width, 1, Colors.MountainSnow);
            }
        });

        // Water (tile 3) - Blue with wave pattern
        DrawTileAt(3, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Water);
            // Horizontal wave lines
            Raylib.DrawRectangle(tileX + 2, tileY + 4, 12, 1, Colors.WaterLight);
            Raylib.DrawRectangle(tileX + 1, tileY + 8, 14, 1, Colors.WaterLight);
            Raylib.DrawRectangle(tileX + 3, tileY + 12, 10, 1, Colors.WaterLight);
        });
    }

    private static void GenerateStructureTiles()
    {
        // Town (tile 10) - Simple building (Ultima IV style)
        DrawTileAt(10, (tileX, tileY) =>
        {
            // Building base
            Raylib.DrawRectangle(tileX + 2, tileY + 6, 12, 8, Colors.TownWall);
            // Roof (triangle)
            Raylib.DrawRectangle(tileX + 4, tileY + 4, 8, 2, Colors.TownRoof);
            Raylib.DrawRectangle(tileX + 6, tileY + 2, 4, 2, Colors.TownRoof);
            // Door
            Raylib.DrawRectangle(tileX + 6, tileY + 10, 4, 4, Raylib.BLACK);
        });

        // Temple (tile 11) - Golden structure with columns
        DrawTileAt(11, (tileX, tileY) =>
        {
            // Base
            Raylib.DrawRectangle(tileX, tileY + 10, TILE_SIZE, 6, Colors.TempleWhite);
            // Columns (3 pillars)
            Raylib.DrawRectangle(tileX + 2, tileY + 4, 2, 6, Colors.TempleGold);
            Raylib.DrawRectangle(tileX + 7, tileY + 4, 2, 6, Colors.TempleGold);
            Raylib.DrawRectangle(tileX + 12, tileY + 4, 2, 6, Colors.TempleGold);
            // Roof
            Raylib.DrawRectangle(tileX + 1, tileY + 3, 14, 1, Colors.TempleGold);
        });

        // Dungeon entrance (tile 12) - Dark opening
        DrawTileAt(12, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Mountain);
            // Dark opening
            Raylib.DrawRectangle(tileX + 4, tileY + 6, 8, 8, Raylib.BLACK);
            // Stone arch
            Raylib.DrawRectangle(tileX + 3, tileY + 5, 2, 10, Colors.DungeonWall);
            Raylib.DrawRectangle(tileX + 11, tileY + 5, 2, 10, Colors.DungeonWall);
        });
    }

    private static void GenerateCharacterTiles()
    {
        // Player (tile 20) - Ultima IV "@" symbol style
        DrawTileAt(20, (tileX, tileY) =>
        {
            // Head (circle)
            Raylib.DrawCircle(tileX + 8, tileY + 5, 3, Colors.Player);
            // Body (rectangle)
            Raylib.DrawRectangle(tileX + 6, tileY + 8, 4, 5, Colors.PlayerArmor);
            // Arms (horizontal bar)
            Raylib.DrawRectangle(tileX + 4, tileY + 9, 8, 2, Colors.PlayerArmor);
            // Legs
            Raylib.DrawRectangle(tileX + 6, tileY + 13, 2, 3, Colors.PlayerArmor);
            Raylib.DrawRectangle(tileX + 8, tileY + 13, 2, 3, Colors.PlayerArmor);
        });

        // NPC (tile 21) - Similar but different color
        DrawTileAt(21, (tileX, tileY) =>
        {
            Raylib.DrawCircle(tileX + 8, tileY + 5, 3, new Color(220, 180, 140, 255));
            Raylib.DrawRectangle(tileX + 6, tileY + 8, 4, 5, new Color(100, 100, 180, 255));
            Raylib.DrawRectangle(tileX + 4, tileY + 9, 8, 2, new Color(100, 100, 180, 255));
        });
    }

    private static void GenerateEnemyTiles()
    {
        // Goblin (tile 30) - Simple green creature
        DrawTileAt(30, (tileX, tileY) =>
        {
            // Body (oval)
            Raylib.DrawCircle(tileX + 8, tileY + 10, 5, Colors.Goblin);
            // Eyes (white dots)
            Raylib.DrawPixel(tileX + 6, tileY + 9, Raylib.WHITE);
            Raylib.DrawPixel(tileX + 10, tileY + 9, Raylib.WHITE);
            // Ears (triangles)
            Raylib.DrawPixel(tileX + 4, tileY + 8, Colors.Goblin);
            Raylib.DrawPixel(tileX + 12, tileY + 8, Colors.Goblin);
        });

        // Undead (tile 31) - Skeleton (pale)
        DrawTileAt(31, (tileX, tileY) =>
        {
            // Skull
            Raylib.DrawCircle(tileX + 8, tileY + 6, 4, Colors.Undead);
            // Eye sockets (black)
            Raylib.DrawPixel(tileX + 6, tileY + 6, Raylib.BLACK);
            Raylib.DrawPixel(tileX + 10, tileY + 6, Raylib.BLACK);
            // Ribcage (lines)
            for (int i = 0; i < 3; i++)
            {
                Raylib.DrawRectangle(tileX + 5, tileY + 10 + (i * 2), 6, 1, Colors.Undead);
            }
        });

        // Demon (tile 32) - Red menacing figure
        DrawTileAt(32, (tileX, tileY) =>
        {
            // Head with horns
            Raylib.DrawCircle(tileX + 8, tileY + 7, 4, Colors.Demon);
            // Horns
            Raylib.DrawPixel(tileX + 5, tileY + 4, Raylib.BLACK);
            Raylib.DrawPixel(tileX + 11, tileY + 4, Raylib.BLACK);
            Raylib.DrawPixel(tileX + 4, tileY + 5, Raylib.BLACK);
            Raylib.DrawPixel(tileX + 12, tileY + 5, Raylib.BLACK);
            // Body
            Raylib.DrawRectangle(tileX + 6, tileY + 11, 4, 4, Colors.Demon);
        });

        // Beast (tile 33) - Brown animal
        DrawTileAt(33, (tileX, tileY) =>
        {
            // Body (horizontal oval)
            Raylib.DrawRectangle(tileX + 4, tileY + 8, 8, 6, Colors.Beast);
            // Head
            Raylib.DrawCircle(tileX + 11, tileY + 9, 3, Colors.Beast);
            // Legs
            Raylib.DrawRectangle(tileX + 5, tileY + 14, 1, 2, Colors.Beast);
            Raylib.DrawRectangle(tileX + 10, tileY + 14, 1, 2, Colors.Beast);
        });
    }

    private static void GenerateItemTiles()
    {
        // Potion (tile 40) - Classic RPG potion
        DrawTileAt(40, (tileX, tileY) =>
        {
            // Bottle body
            Raylib.DrawRectangle(tileX + 6, tileY + 8, 4, 6, Colors.PotionRed);
            // Cork/top
            Raylib.DrawRectangle(tileX + 6, tileY + 7, 4, 1, Raylib.DARKGRAY);
            // Highlight (shine)
            Raylib.DrawPixel(tileX + 7, tileY + 9, Raylib.WHITE);
        });

        // Gold (tile 41) - Coin or pile
        DrawTileAt(41, (tileX, tileY) =>
        {
            // Coins (circles)
            Raylib.DrawCircle(tileX + 6, tileY + 10, 3, Colors.Gold);
            Raylib.DrawCircle(tileX + 10, tileY + 9, 3, Colors.Gold);
            Raylib.DrawCircle(tileX + 8, tileY + 12, 3, Colors.Gold);
        });

        // Treasure chest (tile 42)
        DrawTileAt(42, (tileX, tileY) =>
        {
            // Chest base
            Raylib.DrawRectangle(tileX + 4, tileY + 9, 8, 6, Colors.Chest);
            // Lid (curved top)
            Raylib.DrawRectangle(tileX + 4, tileY + 7, 8, 2, Colors.Chest);
            Raylib.DrawRectangle(tileX + 5, tileY + 6, 6, 1, Colors.Chest);
            // Lock (gold)
            Raylib.DrawRectangle(tileX + 7, tileY + 10, 2, 2, Colors.Gold);
        });
    }

    private static void GenerateDungeonTiles()
    {
        // Dungeon wall (tile 50) - Stone wall
        DrawTileAt(50, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonWall);
            // Stone texture (brick pattern)
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int bx = tileX + (col * 4) + (row % 2 == 0 ? 0 : 2);
                    int by = tileY + (row * 4);
                    Raylib.DrawRectangleLines(bx, by, 4, 4, Raylib.BLACK);
                }
            }
        });

        // Dungeon floor (tile 51) - Dark stone
        DrawTileAt(51, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            // Subtle cracks
            Raylib.DrawPixel(tileX + 5, tileY + 6, Raylib.BLACK);
            Raylib.DrawPixel(tileX + 11, tileY + 10, Raylib.BLACK);
        });

        // Stairs down (tile 52) - ">" symbol
        DrawTileAt(52, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            // ">" shape for stairs down
            Raylib.DrawRectangle(tileX + 4, tileY + 8, 1, 1, Raylib.WHITE);
            Raylib.DrawRectangle(tileX + 5, tileY + 7, 1, 1, Raylib.WHITE);
            Raylib.DrawRectangle(tileX + 6, tileY + 6, 1, 1, Raylib.WHITE);
            Raylib.DrawRectangle(tileX + 7, tileY + 7, 1, 1, Raylib.WHITE);
            Raylib.DrawRectangle(tileX + 8, tileY + 8, 1, 1, Raylib.WHITE);
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
        Raylib.ExportImage(image, path);
        Raylib.UnloadImage(image);

        Console.WriteLine($"✓ Saved procedural tileset to: {path}");
    }
}
