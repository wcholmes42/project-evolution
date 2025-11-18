namespace ProjectEvolution.Game;

/// <summary>
/// Maps game objects (terrain, mobs, items) to tile IDs in the Kenney Roguelike Pack spritesheet
/// Spritesheet: 57 tiles per row, 31 rows total (1,767 tiles)
/// </summary>
public static class TileMapper
{
    // Kenney's roguelike pack has 57 tiles per row
    private const int TILES_PER_ROW = 57;

    // Helper to calculate tile ID from row/col
    private static int Tile(int row, int col) => row * TILES_PER_ROW + col;

    // ═══════════════════════════════════════════════════════════════
    // TERRAIN TILES - Based on standard Kenney Roguelike Pack layout
    // ═══════════════════════════════════════════════════════════════

    // Floor/Ground tiles (Row 0 = typical ground tiles in Kenney packs)
    public static readonly int GRASS_TILE = Tile(0, 1);          // Grass - green tile
    public static readonly int DIRT_TILE = Tile(0, 6);           // Dirt - brown tile
    public static readonly int STONE_FLOOR = Tile(0, 11);        // Stone floor - gray
    public static readonly int WOOD_FLOOR = Tile(1, 0);          // Wood planks

    // Water tiles (Row 0, blue tiles)
    public static readonly int WATER_TILE = Tile(0, 14);         // Deep water
    public static readonly int SHALLOW_WATER = Tile(0, 15);      // Shallow water

    // Forest/Nature (Rows 6-8 typically have nature tiles)
    public static readonly int TREE_TILE = Tile(7, 0);           // Large tree
    public static readonly int PINE_TREE = Tile(7, 3);           // Pine/conifer
    public static readonly int BUSH = Tile(7, 12);               // Bush/shrub

    // Mountain/Rock (Row 6-7 typically)
    public static readonly int MOUNTAIN_TILE = Tile(6, 0);       // Large mountain/cliff
    public static readonly int ROCK_TILE = Tile(6, 3);           // Small rock

    // Dungeon walls (Rows 2-3 are usually walls)
    public static readonly int DUNGEON_WALL = Tile(2, 21);       // Stone wall
    public static readonly int DUNGEON_FLOOR = Tile(1, 6);       // Dark dungeon floor

    // Special terrain (Row 0, far right usually has stairs/doors)
    public static readonly int STAIRS_DOWN = Tile(0, 51);        // Stairs descending
    public static readonly int STAIRS_UP = Tile(0, 52);          // Stairs ascending
    public static readonly int DOOR_CLOSED = Tile(2, 3);         // Closed door
    public static readonly int DOOR_OPEN = Tile(2, 4);           // Open door

    // Town/Building tiles (Rows 3-5 typically have buildings)
    public static readonly int TOWN_TILE = Tile(4, 0);           // Small house
    public static readonly int TEMPLE_TILE = Tile(4, 6);         // Church/temple (with cross)
    public static readonly int SHOP_TILE = Tile(4, 3);           // Shop/market stall

    // ═══════════════════════════════════════════════════════════════
    // CHARACTER TILES (Rows 27-30 in Kenney packs)
    // ═══════════════════════════════════════════════════════════════

    // Player characters - Row 27 typically has armored humanoids
    public static readonly int PLAYER_TILE = Tile(27, 0);        // Knight (armored)
    public static readonly int PLAYER_MAGE = Tile(27, 12);       // Mage/wizard
    public static readonly int PLAYER_ROGUE = Tile(27, 6);       // Rogue/thief

    // ═══════════════════════════════════════════════════════════════
    // ENEMY/MOB TILES (Rows 16-26 in Kenney packs)
    // ═══════════════════════════════════════════════════════════════

    // Small creatures (Row 16-17 typically has small monsters)
    public static readonly int SLIME = Tile(16, 0);              // Green slime
    public static readonly int RAT = Tile(16, 24);               // Giant rat
    public static readonly int BAT = Tile(16, 30);               // Bat
    public static readonly int SPIDER = Tile(16, 36);            // Spider

    // Undead (Row 20-21 typically)
    public static readonly int SKELETON = Tile(20, 0);           // Skeleton warrior
    public static readonly int ZOMBIE = Tile(20, 6);             // Zombie
    public static readonly int GHOST = Tile(20, 12);             // Ghost/wraith

    // Humanoid monsters (Row 19-20)
    public static readonly int GOBLIN = Tile(19, 0);             // Goblin
    public static readonly int ORC = Tile(19, 6);                // Orc warrior
    public static readonly int WOLF = Tile(17, 12);              // Wolf

    // Large monsters (Row 22-24)
    public static readonly int DEMON = Tile(22, 0);              // Demon
    public static readonly int DRAGON = Tile(24, 0);             // Dragon
    public static readonly int SNAKE = Tile(17, 18);             // Snake

    // Boss monsters (larger versions)
    public static readonly int BOSS_DRAGON = Tile(24, 6);        // Large dragon
    public static readonly int BOSS_DEMON = Tile(22, 6);         // Demon lord

    // ═══════════════════════════════════════════════════════════════
    // ITEM TILES (Rows 10-15 in Kenney packs)
    // ═══════════════════════════════════════════════════════════════

    // Treasure/Items (Row 10)
    public static readonly int TREASURE_CHEST = Tile(10, 0);     // Closed chest
    public static readonly int TREASURE_OPEN = Tile(10, 1);      // Open chest
    public static readonly int GOLD_COIN = Tile(10, 12);         // Gold pile/coins
    public static readonly int POTION_RED = Tile(11, 0);         // Red potion (health)
    public static readonly int POTION_BLUE = Tile(11, 6);        // Blue potion (mana)
    public static readonly int POTION_GREEN = Tile(11, 12);      // Green potion (poison?)

    // Weapons (Row 12)
    public static readonly int SWORD = Tile(12, 0);              // Iron sword
    public static readonly int AXE = Tile(12, 6);                // Battle axe
    public static readonly int STAFF = Tile(12, 18);             // Magic staff
    public static readonly int BOW = Tile(12, 24);               // Longbow

    // Armor (Row 13)
    public static readonly int HELMET = Tile(13, 0);             // Iron helmet
    public static readonly int ARMOR = Tile(13, 6);              // Chest plate
    public static readonly int SHIELD = Tile(13, 12);            // Shield

    // Special items (Rows 14-15)
    public static readonly int KEY = Tile(14, 0);                // Golden key
    public static readonly int ARTIFACT = Tile(14, 12);          // Crystal/gem
    public static readonly int BOOK = Tile(14, 18);              // Spellbook/scroll
    public static readonly int PORTAL = Tile(0, 48);             // Portal/warp tile

    // ═══════════════════════════════════════════════════════════════
    // SPECIAL TILES
    // ═══════════════════════════════════════════════════════════════

    public static readonly int CORPSE = Tile(20, 18);            // Dead body/bones
    public static readonly int TRAP = Tile(9, 0);                // Spike trap

    // ═══════════════════════════════════════════════════════════════
    // MAPPING FUNCTIONS
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Get the tile ID for a terrain type
    /// </summary>
    public static int GetTerrainTileId(string terrain)
    {
        return terrain switch
        {
            "Grass" => GRASS_TILE,
            "Forest" => TREE_TILE,
            "Mountain" => MOUNTAIN_TILE,
            "Water" => WATER_TILE,
            "Town" => TOWN_TILE,
            "Dungeon" => DUNGEON_WALL,
            "Temple" => TEMPLE_TILE,
            _ => GRASS_TILE
        };
    }

    /// <summary>
    /// Get the tile ID for a dungeon tile
    /// </summary>
    public static int GetDungeonTileId(string tileType)
    {
        return tileType switch
        {
            "Floor" => DUNGEON_FLOOR,
            "Wall" => DUNGEON_WALL,
            "Stairs" => STAIRS_DOWN,
            "Monster" => SKELETON,           // Will be replaced by actual monster
            "Treasure" => TREASURE_CHEST,
            "Trap" => TRAP,
            "Boss" => BOSS_DRAGON,
            "Artifact" => ARTIFACT,
            "Portal" => PORTAL,
            _ => DUNGEON_FLOOR
        };
    }

    /// <summary>
    /// Get the tile ID for a mob based on enemy type
    /// </summary>
    public static int GetMobTileId(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.GoblinScout => SLIME,
            EnemyType.GoblinWarrior => ORC,
            EnemyType.GoblinArcher => GOBLIN,
            _ => GOBLIN
        };
    }

    /// <summary>
    /// Get the tile ID for a mob based on level (more variety)
    /// </summary>
    public static int GetMobTileByLevel(int level)
    {
        return level switch
        {
            <= 2 => SLIME,
            <= 4 => RAT,
            <= 6 => GOBLIN,
            <= 8 => SKELETON,
            <= 10 => ORC,
            <= 12 => ZOMBIE,
            <= 15 => DEMON,
            <= 20 => DRAGON,
            _ => BOSS_DRAGON
        };
    }

    /// <summary>
    /// Get random mob tile for variety
    /// </summary>
    public static int GetRandomMobTile(Random random, int level)
    {
        var lowLevel = new[] { SLIME, RAT, BAT, SPIDER };
        var midLevel = new[] { GOBLIN, SKELETON, ZOMBIE, ORC };
        var highLevel = new[] { DEMON, DRAGON, WOLF };

        if (level <= 5)
            return lowLevel[random.Next(lowLevel.Length)];
        else if (level <= 12)
            return midLevel[random.Next(midLevel.Length)];
        else
            return highLevel[random.Next(highLevel.Length)];
    }
}
