# Tile Connectivity & Autotiling Guide

## What is Tile Connectivity?

Tile connectivity refers to understanding **which tiles are meant to be placed next to each other** to create seamless maps. For example:

- **Wall tiles** have variations for corners, edges, and interiors
- **Floor tiles** transition smoothly between grass, stone, dirt
- **Water tiles** have shores and edges that connect to land

## The Problem

When you randomly place tiles, they look disconnected:
```
🟫🟫🟩  ← Brown dirt suddenly turns to grass
🟫🟫🟩  ← No transition, looks wrong!
```

With proper connectivity:
```
🟫🟫🟤🟩  ← Dirt gradually transitions to grass
🟫🟤🟤🟩  ← Looks natural!
```

## Tile Connectivity Analyzer Tool

**Press [K]** from the main menu to run the Tile Connectivity Analyzer!

### What It Does:

1. **Examines Sample Maps** - Analyzes the professional TMX maps included with the tileset
2. **Identifies Adjacent Tiles** - Sees which tiles appear next to each other
3. **Groups Tile Sets** - Finds families of tiles that work together
4. **Creates Connectivity Rules** - Shows you which tiles connect in each direction

### Output:

Creates `tile-connectivity.txt` with:

```
Floor/Ground Tiles:
  Tile IDs: 125, 126, 127, 128, 129...
  Range: 125-145

  Tile 125 (0-based: 124 = row 2, col 11)
    North: 125, 126, 127  ← These tiles can go above it
    South: 125, 183, 184  ← These tiles can go below it
    East: 126, 127, 128   ← These tiles can go right
    West: 124, 125, 183   ← These tiles can go left

Wall/Structure Tiles:
  Tile IDs: 183, 184, 185, 186...
  Range: 183-240
```

## Understanding Tile Sets

### Floor Tiles (Rows 0-1)
Usually organized as:
```
[0,0] = Grass plain
[0,1] = Grass variant 1
[0,2] = Grass variant 2
[0,3] = Grass-to-dirt transition NW
[0,4] = Grass-to-dirt transition N
[0,5] = Grass-to-dirt transition NE
... etc
```

### Wall Tiles (Rows 2-5)
Typically organized as:
```
Row 2: Wall variations
  [2,0] = Wall top-left corner
  [2,1] = Wall top edge
  [2,2] = Wall top-right corner
  [2,3] = Wall left edge
  [2,4] = Wall center
  [2,5] = Wall right edge
  [2,6] = Wall bottom-left corner
  [2,7] = Wall bottom edge
  [2,8] = Wall bottom-right corner
```

## How to Use This Information

### Step 1: Run Connectivity Analyzer
```bash
dotnet run
Press [K]
```

This creates `tile-connectivity.txt` showing you all the tile relationships.

### Step 2: Identify Tile Families

Look for groups of tiles with similar IDs that connect to each other:

```
Tiles 125-145: Floor tiles (all connect to each other)
Tiles 183-240: Wall tiles (all connect to each other)
Tiles 456-489: Tree tiles (all connect to each other)
```

### Step 3: Update Your TileMapper

Instead of single tiles, define tile **sets**:

```csharp
// Floor tile set (16 tiles for all transitions)
public static readonly int[] GRASS_TILES = { 125, 126, 127, 128, 129, 130, 131, 132,
                                             133, 134, 135, 136, 137, 138, 139, 140 };

// Wall tile set (9 tiles for corners, edges, center)
public static readonly int[] WALL_TILES = { 183, 184, 185,  // Top-left, top, top-right
                                            186, 187, 188,  // Left, center, right
                                            189, 190, 191 };// Bottom-left, bottom, bottom-right
```

### Step 4: Implement Autotiling (Optional)

Create a function that picks the right tile based on neighbors:

```csharp
public static int GetFloorTile(bool hasNorth, bool hasSouth, bool hasEast, bool hasWest)
{
    // Pick the appropriate grass tile based on which neighbors are different terrain
    // This would use the connectivity data to select the right transition tile
}
```

## Common Tile Patterns

### 9-Tile Pattern (Walls, Cliffs)
```
┌─┬─┬─┐
│0│1│2│  Top row: Corners and top edge
├─┼─┼─┤
│3│4│5│  Middle row: Left edge, center, right edge
├─┼─┼─┤
│6│7│8│  Bottom row: Corners and bottom edge
└─┴─┴─┘
```

### 16-Tile Pattern (Terrain Transitions)
```
Full    = Both terrains match
Edges   = One side different (N, S, E, W)
Corners = Adjacent sides different (NW, NE, SW, SE)
Inner   = Opposite sides different
Diag    = Diagonal corners
```

### 47-Tile Pattern (Complex Autotiling)
Full autotiling with all possible neighbor combinations. Very complex!

## Practical Example

Let's say the analyzer finds:

```
Tile 125: Grass center
  North: 125, 126
  South: 125, 183
  East: 125, 127
  West: 125, 124

Tile 183: Stone floor
  North: 125, 183
  South: 183, 184
  East: 183, 185
  West: 183, 182
```

This tells you:
- **Tile 125** (grass) connects to **Tile 126** (probably grass edge/variant) to the north
- **Tile 125** (grass) connects to **Tile 183** (stone) to the south
  - So **183** is likely a grass-to-stone transition tile!
- **Tile 183** connects to itself and other 180s tiles
  - So **183-185** are probably the stone floor tile set

## Tools Summary

| Tool | Key | Purpose |
|------|-----|---------|
| **Tile Viewer** | Y | See individual tiles and their IDs |
| **TMX Analyzer** | U | Learn which tiles are used for what |
| **Connectivity Analyzer** | K | Understand which tiles connect together |

## Workflow

1. **[K]** Connectivity Analyzer → See tile families and connections
2. **[Y]** Tile Viewer → Visual confirmation of tiles
3. **[U]** TMX Analyzer → See tile usage frequencies
4. **Update TileMapper.cs** → Use tile sets instead of single tiles
5. **Build & Test** → See proper tile connectivity!

## Next Level: Autotiling System

Once you understand tile connectivity, you can build an autotiling system that:

1. Looks at the terrain type at a position
2. Checks the 4 (or 8) neighbors
3. Picks the correct tile from the tile set based on neighbors
4. Makes maps look professional automatically!

This is advanced but the connectivity analyzer gives you all the data you need!

---

**The connectivity analyzer reveals the "secret sauce" of how professional tilesets are organized!** 🔗
