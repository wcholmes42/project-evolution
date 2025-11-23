using Raylib_CsLo;

namespace ProjectEvolution.Game;

/// <summary>
/// Authentic Ultima IV 16-color EGA palette (matching CGA colors)
/// Based on https://wiki.ultimacodex.com/wiki/Ultima_IV_internal_formats
/// </summary>
public static class UltimaIVPalette
{
    // Standard EGA colors (0-15)
    public static readonly Color Black = new Color(0, 0, 0, 255);
    public static readonly Color Blue = new Color(0, 0, 170, 255);
    public static readonly Color Green = new Color(0, 170, 0, 255);
    public static readonly Color Cyan = new Color(0, 170, 170, 255);
    public static readonly Color Red = new Color(170, 0, 0, 255);
    public static readonly Color Magenta = new Color(170, 0, 170, 255);
    public static readonly Color Brown = new Color(170, 85, 0, 255);
    public static readonly Color LightGray = new Color(170, 170, 170, 255);
    public static readonly Color DarkGray = new Color(85, 85, 85, 255);
    public static readonly Color BrightBlue = new Color(85, 85, 255, 255);
    public static readonly Color BrightGreen = new Color(85, 255, 85, 255);
    public static readonly Color BrightCyan = new Color(85, 255, 255, 255);
    public static readonly Color BrightRed = new Color(255, 85, 85, 255);
    public static readonly Color BrightMagenta = new Color(255, 85, 255, 255);
    public static readonly Color Yellow = new Color(255, 255, 85, 255);
    public static readonly Color White = new Color(255, 255, 255, 255);

    // Themed color assignments (Ultima IV style)
    public static class Terrain
    {
        public static readonly Color DeepWater = Blue;
        public static readonly Color ShallowWater = Cyan;
        public static readonly Color Grass = BrightGreen;
        public static readonly Color GrassDark = Green;
        public static readonly Color Forest = Green;
        public static readonly Color ForestDark = DarkGray;
        public static readonly Color Mountain = LightGray;
        public static readonly Color MountainSnow = White;
        public static readonly Color Swamp = Brown;
        public static readonly Color Desert = Yellow;
    }

    public static class Structures
    {
        public static readonly Color TownWall = Brown;
        public static readonly Color TownRoof = Red;
        public static readonly Color TownDoor = DarkGray;
        public static readonly Color Castle = LightGray;
        public static readonly Color Temple = Yellow;
        public static readonly Color TempleAccent = White;
        public static readonly Color Dungeon = DarkGray;
        public static readonly Color DungeonDark = Black;
    }

    public static class Characters
    {
        public static readonly Color Avatar = White;
        public static readonly Color AvatarCloak = BrightRed;
        public static readonly Color NPC = LightGray;
        public static readonly Color NPCClothes = Cyan;
        public static readonly Color Guard = Blue;
    }

    public static class Monsters
    {
        public static readonly Color Goblin = Green;
        public static readonly Color Undead = LightGray;
        public static readonly Color Demon = BrightRed;
        public static readonly Color Dragon = Red;
        public static readonly Color Beast = Brown;
        public static readonly Color Slime = BrightGreen;
    }

    public static class Items
    {
        public static readonly Color Potion = BrightMagenta;
        public static readonly Color Gold = Yellow;
        public static readonly Color Chest = Brown;
        public static readonly Color ChestLock = Yellow;
        public static readonly Color Weapon = LightGray;
        public static readonly Color Armor = Cyan;
    }
}
