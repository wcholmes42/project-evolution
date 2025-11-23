namespace ProjectEvolution.Game;

/// <summary>
/// Tile connectivity rules for autotiling and procedural map generation
/// Defines which tiles can be adjacent to which other tiles (Ultima IV logic)
/// </summary>
public static class TileConnectivity
{
    /// <summary>
    /// Tile categories for connectivity rules
    /// </summary>
    public enum TileCategory
    {
        Water,          // Deep water, shallow water
        Land,           // Grass, forest, mountain
        Structure,      // Towns, temples, dungeons
        Dungeon         // Dungeon interiors
    }

    /// <summary>
    /// Get the category for a terrain type
    /// </summary>
    public static TileCategory GetCategory(string terrain)
    {
        return terrain switch
        {
            "Water" => TileCategory.Water,
            "Grass" or "Grassland" or "Forest" or "Mountain" => TileCategory.Land,
            "Town" or "Temple" or "Dungeon" => TileCategory.Structure,
            _ => TileCategory.Land
        };
    }

    /// <summary>
    /// Check if two terrain types can be adjacent (logical connectivity)
    /// </summary>
    public static bool CanBeAdjacent(string terrain1, string terrain2)
    {
        var cat1 = GetCategory(terrain1);
        var cat2 = GetCategory(terrain2);

        // Water can only touch water or land (not structures floating in water)
        if (cat1 == TileCategory.Water && cat2 == TileCategory.Structure)
            return false;
        if (cat2 == TileCategory.Water && cat1 == TileCategory.Structure)
            return false;

        // Everything else can be adjacent
        return true;
    }

    /// <summary>
    /// Get transition tile between two different terrain types
    /// Returns null if no transition needed (same terrain)
    /// </summary>
    public static string? GetTransitionTile(string fromTerrain, string toTerrain)
    {
        if (fromTerrain == toTerrain)
            return null;

        // Water to land transitions
        if (fromTerrain == "Water" && toTerrain == "Grass")
            return "ShallowWater"; // Beach/shore
        if (fromTerrain == "Grass" && toTerrain == "Water")
            return "ShallowWater";

        // Forest to mountain transitions
        if (fromTerrain == "Forest" && toTerrain == "Mountain")
            return "Grass"; // Hills between
        if (fromTerrain == "Mountain" && toTerrain == "Forest")
            return "Grass";

        // No transition needed
        return null;
    }

    /// <summary>
    /// Get the appropriate tile variant based on neighbors (for autotiling)
    /// This creates smooth transitions between terrain types
    /// </summary>
    public static int GetAutotileVariant(string terrain, bool hasNorth, bool hasSouth, bool hasEast, bool hasWest)
    {
        // Calculate bitmask: North=1, South=2, East=4, West=8
        int mask = 0;
        if (hasNorth) mask |= 1;
        if (hasSouth) mask |= 2;
        if (hasEast) mask |= 4;
        if (hasWest) mask |= 8;

        // For now, return 0 (base tile)
        // In future: return different tile variants for corners, edges, etc.
        return mask;
    }

    /// <summary>
    /// Ultima IV terrain generation weights (for procedural generation)
    /// Based on typical Ultima IV world composition
    /// </summary>
    public static class GenerationWeights
    {
        public static readonly Dictionary<string, int> TerrainWeights = new Dictionary<string, int>
        {
            { "Grass", 40 },      // Most common (40%)
            { "Forest", 25 },     // Common (25%)
            { "Mountain", 15 },   // Less common (15%)
            { "Water", 15 },      // Less common (15%)
            { "Town", 3 },        // Rare (3%)
            { "Temple", 1 },      // Very rare (1%)
            { "Dungeon", 1 }      // Very rare (1%)
        };

        /// <summary>
        /// Get a random terrain type based on Ultima IV-style weights
        /// </summary>
        public static string GetWeightedTerrain(Random random)
        {
            int total = TerrainWeights.Values.Sum();
            int roll = random.Next(total);
            int current = 0;

            foreach (var kvp in TerrainWeights)
            {
                current += kvp.Value;
                if (roll < current)
                    return kvp.Key;
            }

            return "Grass"; // Default
        }
    }

    /// <summary>
    /// Edge matching rules for Wang tiling
    /// Defines which terrain edges can connect to which other edges
    /// </summary>
    public static class EdgeRules
    {
        public enum EdgeType
        {
            Water,
            Land,
            Mountain,
            Structure
        }

        public static EdgeType GetEdgeType(string terrain)
        {
            return terrain switch
            {
                "Water" => EdgeType.Water,
                "Mountain" => EdgeType.Mountain,
                "Town" or "Temple" or "Dungeon" => EdgeType.Structure,
                _ => EdgeType.Land
            };
        }

        /// <summary>
        /// Check if two edge types can connect
        /// </summary>
        public static bool CanConnect(EdgeType edge1, EdgeType edge2)
        {
            // Same edges always connect
            if (edge1 == edge2)
                return true;

            // Land can connect to mountains
            if ((edge1 == EdgeType.Land && edge2 == EdgeType.Mountain) ||
                (edge1 == EdgeType.Mountain && edge2 == EdgeType.Land))
                return true;

            // Water and land need transition (shore)
            if ((edge1 == EdgeType.Water && edge2 == EdgeType.Land) ||
                (edge1 == EdgeType.Land && edge2 == EdgeType.Water))
                return true;

            // Structures can sit on land or mountains
            if (edge1 == EdgeType.Structure || edge2 == EdgeType.Structure)
                return true;

            return false;
        }
    }
}
