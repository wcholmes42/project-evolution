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

        // Generate all tiles
        GenerateTerrainTiles();
        GenerateStructureTiles();
        GenerateCharacterTiles();
        GenerateEnemyTiles();
        GenerateItemTiles();
        GenerateDungeonTiles();

        Raylib.EndTextureMode();

        // IMPORTANT: Get the texture before unloading render texture
        Texture result = renderTexture.texture;

        // NOTE: Don't unload render texture here - we're using its internal texture
        // Raylib.UnloadRenderTexture(renderTexture); // This would invalidate the texture!

        return result;
    }

    private static void GenerateTerrainTiles()
    {
        // Grassland (tile 0) - Bright green with grass texture
        DrawTileAt(0, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Grass);
            // Grass blades (simple dots)
            for (int i = 0; i < 12; i++)
            {
                int px = tileX + ((i * 7) % 14) + 1;
                int py = tileY + ((i * 11) % 14) + 1;
                Raylib.DrawPixel(px, py, Colors.GrassDark);
            }
        });

        // Forest (tile 1) - Dark green with tree
        DrawTileAt(1, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Forest);
            // Tree trunk
            Raylib.DrawRectangle(tileX + 7, tileY + 10, 2, 5, Colors.ForestTree);
            // Tree crown (simple circle)
            Raylib.DrawCircle(tileX + 8, tileY + 7, 4, Colors.Grass);
        });

        // Mountain (tile 2) - Gray with peak
        DrawTileAt(2, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Mountain);
            // Mountain peak (triangle)
            for (int row = 0; row < 6; row++)
            {
                int width = 12 - (row * 2);
                Raylib.DrawRectangle(tileX + row + 2, tileY + row + 2, width, 1, Colors.MountainSnow);
            }
        });

        // Water (tile 3) - Blue with waves
        DrawTileAt(3, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Water);
            // Wave pattern
            Raylib.DrawRectangle(tileX + 2, tileY + 4, 12, 1, Colors.WaterLight);
            Raylib.DrawRectangle(tileX + 1, tileY + 8, 14, 1, Colors.WaterLight);
            Raylib.DrawRectangle(tileX + 3, tileY + 12, 10, 1, Colors.WaterLight);
        });
    }

    private static void GenerateStructureTiles()
    {
        // Town (tile 10) - Simple house
        DrawTileAt(10, (tileX, tileY) =>
        {
            // Building walls
            Raylib.DrawRectangle(tileX + 3, tileY + 7, 10, 7, Colors.TownWall);
            // Roof (red triangle)
            Raylib.DrawRectangle(tileX + 5, tileY + 5, 6, 2, Colors.TownRoof);
            Raylib.DrawRectangle(tileX + 6, tileY + 3, 4, 2, Colors.TownRoof);
            // Door
            Raylib.DrawRectangle(tileX + 6, tileY + 11, 4, 3, UltimaIVPalette.Black);
        });

        // Temple (tile 11) - Golden temple with pillars
        DrawTileAt(11, (tileX, tileY) =>
        {
            // Base platform
            Raylib.DrawRectangle(tileX + 1, tileY + 11, 14, 4, Colors.TempleWhite);
            // Three golden pillars
            Raylib.DrawRectangle(tileX + 3, tileY + 5, 2, 6, Colors.TempleGold);
            Raylib.DrawRectangle(tileX + 7, tileY + 5, 2, 6, Colors.TempleGold);
            Raylib.DrawRectangle(tileX + 11, tileY + 5, 2, 6, Colors.TempleGold);
            // Roof line
            Raylib.DrawRectangle(tileX + 2, tileY + 4, 12, 1, Colors.TempleGold);
        });

        // Dungeon entrance (tile 12) - Dark cave opening
        DrawTileAt(12, (tileX, tileY) =>
        {
            // Mountain/rock background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Mountain);
            // Dark opening (cave)
            Raylib.DrawRectangle(tileX + 5, tileY + 7, 6, 7, UltimaIVPalette.Black);
            // Stone arch
            Raylib.DrawRectangle(tileX + 4, tileY + 6, 1, 8, Colors.DungeonWall);
            Raylib.DrawRectangle(tileX + 11, tileY + 6, 1, 8, Colors.DungeonWall);
            Raylib.DrawRectangle(tileX + 5, tileY + 6, 6, 1, Colors.DungeonWall);
        });
    }

    private static void GenerateCharacterTiles()
    {
        // Player (tile 20) - Ultima IV Avatar style (white figure with red cloak)
        DrawTileAt(20, (tileX, tileY) =>
        {
            // Background (grass underneath)
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Grass);
            // Red cloak background
            Raylib.DrawCircle(tileX + 8, tileY + 10, 6, Colors.PlayerArmor);
            // Head (white)
            Raylib.DrawCircle(tileX + 8, tileY + 6, 3, Colors.Player);
            // Body
            Raylib.DrawRectangle(tileX + 7, tileY + 9, 2, 5, Colors.Player);
        });

        // NPC (tile 21) - Villager (cyan/blue clothes)
        DrawTileAt(21, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.Grass);
            // Body (cyan)
            Raylib.DrawCircle(tileX + 8, tileY + 10, 5, UltimaIVPalette.Cyan);
            // Head
            Raylib.DrawCircle(tileX + 8, tileY + 6, 3, UltimaIVPalette.LightGray);
        });
    }

    private static void GenerateEnemyTiles()
    {
        // Goblin (tile 30) - Green creature
        DrawTileAt(30, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Body
            Raylib.DrawCircle(tileX + 8, tileY + 10, 5, Colors.Goblin);
            // Eyes
            Raylib.DrawPixel(tileX + 6, tileY + 9, UltimaIVPalette.White);
            Raylib.DrawPixel(tileX + 10, tileY + 9, UltimaIVPalette.White);
            // Pointy ears
            Raylib.DrawPixel(tileX + 4, tileY + 8, Colors.Goblin);
            Raylib.DrawPixel(tileX + 12, tileY + 8, Colors.Goblin);
        });

        // Undead (tile 31) - Skeleton
        DrawTileAt(31, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Skull
            Raylib.DrawCircle(tileX + 8, tileY + 6, 4, Colors.Undead);
            // Eye sockets
            Raylib.DrawPixel(tileX + 6, tileY + 6, UltimaIVPalette.Black);
            Raylib.DrawPixel(tileX + 10, tileY + 6, UltimaIVPalette.Black);
            // Ribcage
            for (int i = 0; i < 3; i++)
                Raylib.DrawRectangle(tileX + 5, tileY + 11 + (i * 2), 6, 1, Colors.Undead);
        });

        // Demon (tile 32) - Red demon with horns
        DrawTileAt(32, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Body
            Raylib.DrawCircle(tileX + 8, tileY + 10, 5, Colors.Demon);
            // Head with horns
            Raylib.DrawCircle(tileX + 8, tileY + 6, 3, Colors.Demon);
            // Horns (black points)
            Raylib.DrawPixel(tileX + 5, tileY + 4, UltimaIVPalette.Black);
            Raylib.DrawPixel(tileX + 11, tileY + 4, UltimaIVPalette.Black);
        });

        // Beast (tile 33) - Brown wolf/bear
        DrawTileAt(33, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Body (horizontal)
            Raylib.DrawRectangle(tileX + 4, tileY + 9, 8, 5, Colors.Beast);
            // Head
            Raylib.DrawCircle(tileX + 11, tileY + 10, 3, Colors.Beast);
            // Legs
            Raylib.DrawRectangle(tileX + 5, tileY + 14, 1, 2, Colors.Beast);
            Raylib.DrawRectangle(tileX + 10, tileY + 14, 1, 2, Colors.Beast);
        });
    }

    private static void GenerateItemTiles()
    {
        // Potion (tile 40) - Classic RPG potion bottle
        DrawTileAt(40, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Bottle
            Raylib.DrawRectangle(tileX + 6, tileY + 9, 4, 5, Colors.PotionRed);
            // Cork/stopper
            Raylib.DrawRectangle(tileX + 6, tileY + 8, 4, 1, UltimaIVPalette.Brown);
            // Shine
            Raylib.DrawPixel(tileX + 7, tileY + 10, UltimaIVPalette.White);
        });

        // Gold (tile 41) - Pile of coins
        DrawTileAt(41, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Coins (yellow circles)
            Raylib.DrawCircle(tileX + 6, tileY + 10, 3, Colors.Gold);
            Raylib.DrawCircle(tileX + 10, tileY + 9, 3, Colors.Gold);
            Raylib.DrawCircle(tileX + 8, tileY + 12, 2, Colors.Gold);
        });

        // Treasure chest (tile 42) - Chest with lock
        DrawTileAt(42, (tileX, tileY) =>
        {
            // Background
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, UltimaIVPalette.Black);
            // Chest body
            Raylib.DrawRectangle(tileX + 4, tileY + 9, 8, 5, Colors.Chest);
            // Lid (slightly lighter)
            Raylib.DrawRectangle(tileX + 4, tileY + 7, 8, 2, Colors.Chest);
            Raylib.DrawRectangle(tileX + 5, tileY + 6, 6, 1, Colors.Chest);
            // Lock (gold)
            Raylib.DrawRectangle(tileX + 7, tileY + 10, 2, 2, Colors.Gold);
        });
    }

    private static void GenerateDungeonTiles()
    {
        // Dungeon wall (tile 50) - Stone wall with bricks
        DrawTileAt(50, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonWall);
            // Brick pattern (simple grid)
            for (int row = 0; row < 4; row++)
            {
                for (int col = 0; col < 4; col++)
                {
                    int bx = tileX + (col * 4) + (row % 2 == 0 ? 0 : 2);
                    int by = tileY + (row * 4);
                    Raylib.DrawRectangleLines(bx, by, 4, 4, UltimaIVPalette.Black);
                }
            }
        });

        // Dungeon floor (tile 51) - Dark stone floor
        DrawTileAt(51, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            // Subtle cracks/texture
            Raylib.DrawPixel(tileX + 4, tileY + 5, UltimaIVPalette.DarkGray);
            Raylib.DrawPixel(tileX + 9, tileY + 8, UltimaIVPalette.DarkGray);
            Raylib.DrawPixel(tileX + 12, tileY + 11, UltimaIVPalette.DarkGray);
        });

        // Stairs down (tile 52) - White ">" on dark background
        DrawTileAt(52, (tileX, tileY) =>
        {
            Raylib.DrawRectangle(tileX, tileY, TILE_SIZE, TILE_SIZE, Colors.DungeonFloor);
            // ">" symbol (stairs down)
            for (int i = 0; i < 4; i++)
            {
                Raylib.DrawPixel(tileX + 5 + i, tileY + 8 - i, UltimaIVPalette.White);
                Raylib.DrawPixel(tileX + 5 + i, tileY + 8 + i, UltimaIVPalette.White);
            }
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
