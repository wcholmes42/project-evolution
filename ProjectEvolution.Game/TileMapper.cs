namespace ProjectEvolution.Game;

/// <summary>
/// Maps game objects to AUTHENTIC Ultima IV tile IDs
/// Using Josh Steele's Ultima IV tileset (32x32, 16 tiles wide)
/// Based on: https://github.com/jahshuwaa/u4graphics
/// </summary>
public static class TileMapper
{
    // ═══════════════════════════════════════════════════════════════
    // TERRAIN TILES (Authentic Ultima IV - from SHAPES.EGA)
    // Reference: https://wiki.ultimacodex.com/wiki/Ultima_IV_internal_formats
    // ═══════════════════════════════════════════════════════════════

    public static readonly int WATER_TILE = 0;           // Deep water (tile 0)
    public static readonly int GRASS_TILE = 4;           // Grassland (tile 4)
    public static readonly int FOREST_TILE = 6;          // Forest (tile 6)
    public static readonly int MOUNTAIN_TILE = 8;        // Mountains (tile 8)

    // ═══════════════════════════════════════════════════════════════
    // STRUCTURE TILES (from Ultima IV)
    // ═══════════════════════════════════════════════════════════════

    public static readonly int TOWN_TILE = 18;           // Town/village
    public static readonly int CASTLE_TILE = 17;         // Castle
    public static readonly int TEMPLE_TILE = 22;         // Shrine/temple
    public static readonly int DUNGEON_ENTRANCE = 12;    // Dungeon entrance

    // ═══════════════════════════════════════════════════════════════
    // CHARACTER TILES (Ultima IV Avatar and NPCs)
    // ═══════════════════════════════════════════════════════════════

    public static readonly int PLAYER_TILE = 31;         // Avatar (tile 31)
    public static readonly int NPC_TILE = 32;            // Generic NPC

    // ═══════════════════════════════════════════════════════════════
    // ENEMY/MOB TILES (Ultima IV creatures - starting at tile 128+)
    // ═══════════════════════════════════════════════════════════════

    public static readonly int GOBLIN = 144;             // Orc/Goblin creature
    public static readonly int UNDEAD = 160;             // Skeleton
    public static readonly int DEMON = 192;              // Demon
    public static readonly int BEAST = 176;              // Wolf/beast

    // ═══════════════════════════════════════════════════════════════
    // ITEM TILES (Ultima IV)
    // ═══════════════════════════════════════════════════════════════

    public static readonly int POTION = 64;              // Potion/flask
    public static readonly int GOLD = 72;                // Treasure/gold
    public static readonly int TREASURE_CHEST = 72;      // Chest

    // ═══════════════════════════════════════════════════════════════
    // DUNGEON TILES (Ultima IV)
    // ═══════════════════════════════════════════════════════════════

    public static readonly int DUNGEON_WALL = 96;        // Dungeon brick wall
    public static readonly int DUNGEON_FLOOR = 112;      // Dungeon floor
    public static readonly int STAIRS_DOWN = 113;        // Ladder down

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
            "Grassland" => GRASS_TILE,      // tile 4
            "Forest" => FOREST_TILE,        // tile 6
            "Mountain" => MOUNTAIN_TILE,    // tile 8
            "Water" => WATER_TILE,          // tile 0
            "Town" => TOWN_TILE,            // tile 18
            "Dungeon" => DUNGEON_ENTRANCE,  // tile 12
            "Temple" => TEMPLE_TILE,        // tile 22
            _ => GRASS_TILE  // Default to grass (tile 4)
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
            "Monster" => GOBLIN,             // Will be replaced by actual monster
            "Treasure" => TREASURE_CHEST,
            "Trap" => DUNGEON_FLOOR,         // Trap looks like floor until triggered
            "Boss" => DEMON,                 // Boss tile
            "Artifact" => GOLD,              // Artifact (shiny)
            "Portal" => STAIRS_DOWN,         // Portal (use stairs for now)
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
            // Goblins (all use goblin tile)
            EnemyType.GoblinScout => GOBLIN,
            EnemyType.GoblinWarrior => GOBLIN,
            EnemyType.GoblinArcher => GOBLIN,

            // Undead
            EnemyType.Skeleton => UNDEAD,
            EnemyType.Zombie => UNDEAD,
            EnemyType.Wraith => UNDEAD,

            // Beasts
            EnemyType.Wolf => BEAST,
            EnemyType.Bear => BEAST,
            EnemyType.Serpent => BEAST,

            // Demons
            EnemyType.Imp => DEMON,
            EnemyType.Hellhound => DEMON,
            EnemyType.Demon => DEMON,

            _ => GOBLIN  // Default to goblin
        };
    }

    /// <summary>
    /// Get the tile ID for a mob based on level (for variety)
    /// </summary>
    public static int GetMobTileByLevel(int level)
    {
        return level switch
        {
            <= 5 => GOBLIN,
            <= 10 => UNDEAD,
            <= 15 => BEAST,
            _ => DEMON
        };
    }

    /// <summary>
    /// Get random mob tile for variety
    /// </summary>
    public static int GetRandomMobTile(Random random, int level)
    {
        if (level <= 5)
            return GOBLIN;
        else if (level <= 12)
            return random.Next(2) == 0 ? UNDEAD : BEAST;
        else
            return DEMON;
    }
}
