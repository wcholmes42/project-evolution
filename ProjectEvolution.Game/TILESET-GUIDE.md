# Tileset Usage Guide - Kenney Roguelike Pack

## How Tilesets Work

### Tile Grid System
A tileset is a single image containing many small sprites (tiles) arranged in a grid. Each tile can be referenced by its **Tile ID** which is calculated from its position:

```
Tile ID = (Row × Tiles Per Row) + Column
```

**Example**: If a tile is in Row 3, Column 5, and there are 57 tiles per row:
```
Tile ID = (3 × 57) + 5 = 171 + 5 = 176
```

### Kenney Roguelike Pack Specifications

- **Image**: `roguelikeSheet_transparent.png`
- **Dimensions**: 968 × 526 pixels
- **Tile Size**: 16 × 16 pixels
- **Spacing**: 1px margin between tiles
- **Tiles Per Row**: 57
- **Total Rows**: 31
- **Total Tiles**: ~1,767 tiles

### Important: Tile Spacing Calculation

Because there's a 1px spacing between tiles, the formula to get a tile's pixel position is:
```
X Position = Column × (16 + 1) = Column × 17
Y Position = Row × (16 + 1) = Row × 17
```

This is **already handled** in `GraphicsRenderer.cs`.

## Tools Available

### 1. **Tile Viewer** [Press Y]
- Visual grid showing all tiles
- Hover over tiles to see:
  - Tile ID (0-based)
  - Row and Column
  - Pixel position
- Use this to identify which sprite you want

### 2. **TMX Analyzer** [Press U]
- Analyzes the sample maps included with the tileset
- Shows which tiles are used for what purpose
- Outputs to `tile-analysis.txt`
- Learn from professional examples

## How to Fix Tile Mappings

### Step 1: Run TMX Analyzer
```bash
dotnet run
# Press U for TMX Analyzer
```

This will create `tile-analysis.txt` showing you which tiles the official sample maps use for:
- Floors (Ground/terrain layer)
- Walls (Ground overlay layer)
- Objects (Objects layer)
- Roofs/decorations

### Step 2: Run Tile Viewer
```bash
dotnet run
# Press Y for Tile Viewer
# Hover over tiles to identify the ones you want
```

### Step 3: Update TileMapper.cs

Based on what you found, update the tile IDs in `TileMapper.cs`:

```csharp
// Example: Found that grass is actually tile 125
public static readonly int GRASS_TILE = Tile(2, 11);  // Row 2, Col 11 = Tile ID 125

// Helper shows the math:
private static int Tile(int row, int col) => row * 57 + col;
```

### Step 4: Rebuild and Test
```bash
dotnet build
dotnet run
# Press Z for Graphics Mode
```

## Common Tileset Organization

Typically in roguelike tilesets:

**Rows 0-1**: Floors, ground textures
**Rows 2-5**: Walls, doors, windows
**Rows 6-9**: Nature (trees, rocks, water)
**Rows 10-15**: Items (chests, potions, weapons, armor)
**Rows 16-26**: Monsters and creatures
**Rows 27-30**: Player characters, NPCs

## TMX File Structure (For Reference)

The `.tmx` files use **1-based** tile IDs (firstgid=1), so:
- TMX Tile ID 1 = Our Tile ID 0
- TMX Tile ID 125 = Our Tile ID 124

The sample maps have layers:
- **Ground/terrain**: Floor tiles (grass, dirt, stone)
- **Ground overlay**: Decorative patterns on floors
- **Objects**: Trees, furniture, items
- **Doors/windows/roof**: Building elements

## Quick Reference: Finding Specific Tiles

### For Grass/Dirt/Stone Floors
1. Run TMX Analyzer
2. Look at "Ground/terrain" layer
3. Note the most-used tile IDs
4. Those are your floor tiles!

### For Trees/Mountains
1. Run TMX Analyzer
2. Look at "Objects" layer
3. Find tiles used frequently
4. Use Tile Viewer to confirm they look right

### For Player Character
1. Run Tile Viewer
2. Look in rows 27-30
3. Find a character sprite you like
4. Note its Tile ID
5. Update `TileMapper.PLAYER_TILE`

## Example Workflow

1. Run `dotnet run` → Press **U** (TMX Analyzer)
2. Read `tile-analysis.txt`:
   ```
   Ground/terrain layer:
     Tile 125 (row 2, col 11) - used 3500 times  ← This is grass!
     Tile 183 (row 3, col 12) - used 1200 times  ← This is stone?
   ```
3. Run `dotnet run` → Press **Y** (Tile Viewer)
4. Hover over Row 2, Col 11 → Confirm it's grass ✓
5. Edit `TileMapper.cs`:
   ```csharp
   public static readonly int GRASS_TILE = Tile(2, 11);  // 0-based: 124
   ```
6. Rebuild and test!

## Need Help?

- **tile-analysis.txt**: Shows tile usage from sample maps
- **Tile Viewer [Y]**: Visual reference with IDs
- **TMX files**: `Assets/Tilesets/extracted/Map/*.tmx`
- **Spritesheet**: `Assets/Tilesets/roguelike-pack.png`

The tools are set up - you just need to run them and map the tiles you want!
