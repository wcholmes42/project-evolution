# Generation 47: AI-Powered Procedural Graphics + Exclusive Fullscreen

## 🎯 VISION
Replace external tileset dependency with **AI-assisted procedural tile generation** while adding exclusive fullscreen mode and completing the graphics experience.

## 🤖 AI-POWERED FEATURES

### 1. ProceduralTileGenerator.cs
**Uses:**
- Raylib drawing primitives (DrawRectangle, DrawCircle, DrawPixel, etc.)
- Ollama Qwen2.5 32B for creative direction
- Seeded RNG for consistent tile generation
- Texture caching for performance

**Capabilities:**
- Generate 968x527px tileset (57x31 tiles @ 16px + 1px spacing)
- AI-suggested color palettes per theme
- Geometric base patterns (fast, reliable)
- AI-enhanced special tiles (bosses, artifacts, secrets)

### 2. AI Color Palette System
```csharp
public class AIColorPalette
{
    // Qwen2.5 generates JSON like:
    {
        "theme": "Classic Fantasy RPG",
        "grassland": ["#4a9a2a", "#3a7a1a", "#2a6a0a"],
        "forest": ["#1a4a0a", "#0a3a00", "#002a00"],
        "mountain": ["#8a8a8a", "#6a6a6a", "#4a4a4a"],
        "player": ["#ffa500", "#ff6500", "#ff4500"],
        "enemies": {
            "goblin": ["#4a7a1a", "#3a6a0a"],
            "undead": ["#8a8aaa", "#6a6a8a"],
            "demon": ["#aa2a2a", "#8a1a1a"]
        }
    }
}
```

### 3. Tile Generation Methods

#### Terrain Tiles (Geometric)
- **Grassland**: Base green + random dark pixels (grass texture)
- **Forest**: Dark green base + tree symbols (triangles)
- **Mountain**: Gray gradients + rocky texture (rectangles)
- **Town**: Brown/red buildings (rectangles + roofs)
- **Temple**: Gold/white columns (special pattern)
- **Dungeon**: Black walls, gray floors

#### Character Tiles (Geometric + AI)
- **Player**: Circle head + rectangle body + weapon
- **NPCs**: Similar structure, AI suggests clothing colors
- **Companions**: AI designs unique silhouettes

#### Enemy Tiles (AI-Enhanced)
- **Common Enemies**: Geometric base (circle + features)
- **Elite Enemies**: AI suggests special features
- **Bosses**: Full AI creative control
- **Rare Encounters**: AI-designed unique sprites

#### Item Tiles (Simple + AI)
- **Potions**: Circle + color (AI palette)
- **Weapons**: Triangle/rectangle (AI suggests angles)
- **Armor**: Shield shape (AI color scheme)
- **Artifacts**: AI designs special glows/auras

## 💾 CACHING SYSTEM

```csharp
public class TileCache
{
    // Save generated tileset to disk
    public void SaveTileset(Texture tileset, string seed)
    {
        // Assets/Generated/tileset_{seed}.png
        Raylib.ExportImage(tileset, $"Assets/Generated/tileset_{seed}.png");
    }

    // Load if exists, generate if not
    public Texture GetOrGenerateTileset(string seed)
    {
        if (File.Exists($"Assets/Generated/tileset_{seed}.png"))
            return Raylib.LoadTexture(...);
        else
            return ProceduralTileGenerator.Generate(seed);
    }
}
```

## 🖥️ EXCLUSIVE FULLSCREEN MODE

### GraphicsRenderer.cs Changes
```csharp
public void InitializeFullscreen(int width, int height)
{
    // NEW: Exclusive fullscreen flag
    Raylib.SetConfigFlags(
        ConfigFlags.FLAG_FULLSCREEN_MODE |  // <-- EXCLUSIVE FULLSCREEN
        ConfigFlags.FLAG_VSYNC_HINT
    );

    Raylib.InitWindow(width, height, "Project Evolution - Gen 47 (Fullscreen)");
    Raylib.SetTargetFPS(60);
}

public void ToggleFullscreen()
{
    Raylib.ToggleFullscreen();
}
```

### Resolution Selector
```csharp
public static (int, int) SelectResolution()
{
    var resolutions = new[]
    {
        (1920, 1080),
        (2560, 1440),
        (3840, 2160),
        (1280, 720)  // Fallback
    };

    Console.WriteLine("Select resolution:");
    // ... menu logic
}
```

## 🎮 GRAPHICAL COMBAT SCREEN

### GraphicsCombatScreen.cs (NEW)
```csharp
public class GraphicsCombatScreen
{
    public void Render(RPGGame game, GraphicsRenderer renderer)
    {
        // Layout:
        // [Player Sprite]  [VS]  [Enemy Sprite]
        // [Player HP Bar]        [Enemy HP Bar]
        // [Player Buffs]         [Enemy Buffs]
        //
        // [Combat Log - last 5 lines]
        //
        // [A]ttack [D]efend [S]kills [F]lee [P]otion

        DrawCharacterPanel(game, isPlayer: true, x: 100);
        DrawCharacterPanel(game, isPlayer: false, x: 700);
        DrawCombatLog(game.CombatLog);
        DrawActionButtons();
    }

    private void DrawCharacterPanel(RPGGame game, bool isPlayer, int x)
    {
        // Draw procedurally generated sprite (large 48x48)
        int tileId = isPlayer ? TileMapper.PLAYER_TILE : TileMapper.GetMobTileId(game.EnemyType);
        renderer.DrawTileAt(tileId, x, 100, scale: 3); // 3x larger for combat

        // HP bar with smooth animation
        DrawAnimatedHealthBar(x, 200, ...);

        // Buff icons
        if (game.HasBuff("RAGE"))
            DrawText("🔥 RAGE", x, 250, RED);
    }
}
```

## 💀 DEATH SCREEN

### GraphicsDeathScreen.cs (NEW)
```csharp
public class GraphicsDeathScreen
{
    public void Show(RPGGame game, string killer, int goldLost, List<string> items)
    {
        // Fade to red
        for (float alpha = 0; alpha < 0.8f; alpha += 0.05f)
        {
            DrawRectangle(0, 0, screenWidth, screenHeight, new Color(255, 0, 0, (int)(alpha * 255)));
            Thread.Sleep(50);
        }

        // "YOU DIED" banner (Dark Souls style)
        DrawText("YOU DIED", centerX, centerY - 100, 72, WHITE);

        // Stats
        DrawText($"Slain by: {killer}", centerX, centerY, 32, LIGHTGRAY);
        DrawText($"Gold lost: {goldLost}g", centerX, centerY + 40, 24, GOLD);

        if (items.Count > 0)
            DrawText($"Items dropped: {string.Join(", ", items)}", centerX, centerY + 70, 20, RED);

        // Respawn countdown
        DrawText("Respawning at Temple in 3...", centerX, centerY + 120, 20, WHITE);
    }
}
```

## 📋 IMPLEMENTATION CHECKLIST

### Phase 1: AI Tile Generation (3-4 hours)
- [ ] Create `ProceduralTileGenerator.cs`
- [ ] Implement `OllamaColorPalette.cs` (Qwen2.5 integration)
- [ ] Generate terrain tiles (geometric)
- [ ] Generate character tiles (geometric + AI palette)
- [ ] Generate enemy tiles (AI-enhanced)
- [ ] Create tile cache system
- [ ] Test: Verify tileset renders correctly

### Phase 2: Fullscreen Mode (30 min)
- [ ] Add `FLAG_FULLSCREEN_MODE` to GraphicsRenderer
- [ ] Implement resolution selector
- [ ] Add F11 toggle support
- [ ] Test: Verify exclusive fullscreen works

### Phase 3: Combat Screen (2 hours)
- [ ] Create `GraphicsCombatScreen.cs`
- [ ] Design combat UI layout
- [ ] Implement action buttons
- [ ] Add combat animations (damage numbers, screen shake)
- [ ] Integrate into `GraphicsGameLoop.cs` (remove auto-resolve)
- [ ] Test: Play through combat in graphics mode

### Phase 4: Death Screen (1 hour)
- [ ] Create `GraphicsDeathScreen.cs`
- [ ] Implement fade effects
- [ ] Add death statistics display
- [ ] Integrate into `GraphicsGameLoop.cs`
- [ ] Test: Die and verify respawn flow

### Phase 5: Polish (1 hour)
- [ ] Add visual effects (screen shake, particles)
- [ ] Smooth camera transitions
- [ ] Performance profiling
- [ ] Settings menu (resolution, effects quality)
- [ ] Test: Full playthrough in graphics mode

## 🧪 NEW TESTS

```csharp
// ProjectEvolution.Tests/ProceduralTileTests.cs
[Fact]
public void ProceduralGenerator_GenerateTileset_CreatesValidTexture()
{
    var generator = new ProceduralTileGenerator();
    var tileset = generator.GenerateTileset(seed: "test123");
    Assert.NotNull(tileset);
    Assert.Equal(968, tileset.width);
    Assert.Equal(527, tileset.height);
}

[Fact]
public async Task OllamaColorPalette_GetPalette_ReturnsValidColors()
{
    var palette = await OllamaColorPalette.GenerateAsync("fantasy");
    Assert.NotNull(palette.Grassland);
    Assert.True(palette.Grassland.Length >= 3);
}

[Fact]
public void GraphicsCombatScreen_Render_DisplaysPlayerAndEnemy()
{
    var game = TestHelpers.CreateGameInCombat();
    var screen = new GraphicsCombatScreen();
    screen.Render(game, mockRenderer);
    Assert.True(screen.IsPlayerVisible);
    Assert.True(screen.IsEnemyVisible);
}
```

## 🎯 EXPECTED OUTCOME

**Generation 47 delivers:**
- ✅ **Zero external dependencies** - All tiles generated procedurally
- ✅ **AI-powered creativity** - Qwen2.5 suggests palettes and designs
- ✅ **Exclusive fullscreen** - True fullscreen mode for performance
- ✅ **Complete graphics experience** - Combat, death, all playable
- ✅ **Persistent caching** - Fast startup after first generation
- ✅ **Customizable themes** - AI can generate different art styles

**Test Count:** ~225+ passing tests

## 🎨 BONUS: Multiple Art Styles

Because generation is procedural, we can offer:
- **Classic Fantasy** (current plan)
- **Cyberpunk** (AI suggests neon colors)
- **Horror** (dark palette, creepy sprites)
- **Retro** (CGA/EGA color schemes)
- **Minimalist** (simple shapes, muted colors)

Each style is just a different AI prompt + palette!

---

**Ready to implement Generation 47 with AI-powered procedural tiles?**
This will make the game truly self-contained and showcase AI integration beyond just tuning! 🚀
