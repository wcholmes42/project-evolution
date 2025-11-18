# Project Evolution - Graphics Assets

## Required Tileset Download

To run Project Evolution in graphics mode, you need to download the tileset:

### 📦 Kenney's Roguelike/RPG Pack (1,700+ tiles)
- **License**: CC0 (Public Domain) - Free to use, no attribution required
- **Tile Size**: 16x16 pixels
- **Tiles**: 1,700+ tiles including floors, walls, vegetation, doors, furniture, UI elements
- **Download**: https://opengameart.org/content/roguelikerpg-pack-1700-tiles
  - Click "Roguelike pack.zip" (715.4 KB)

### Installation Instructions:

1. Download `Roguelike pack.zip` from the link above
2. Extract the ZIP file
3. Copy the **main spritesheet PNG file** to:
   ```
   ProjectEvolution.Game/Assets/Tilesets/roguelike-pack.png
   ```
4. The game will automatically load this tileset on startup

### File Structure:
```
Assets/
├── README.md (this file)
└── Tilesets/
    └── roguelike-pack.png (you need to download this)
```

### Credits (Optional):
While CC0 license doesn't require attribution, you may credit:
- **Creator**: Kenney (Kenney.nl)
- **16x16 Conversion**: Lynn Evers
- **Original**: https://www.kenney.nl

---

## Alternative Tilesets

You can also use other 16x16 tilesets by placing them in the `Assets/Tilesets/` folder and updating the `GraphicsRenderer.cs` to point to your preferred tileset.

### Other Options:
- **16x16 Puny World**: https://opengameart.org/content/16x16-puny-world-tileset
- **Dungeon Tileset**: https://opengameart.org/content/dungeon-tileset
- **Open RPG Fantasy**: https://finalbossblues.itch.io/openrtp-tiles

---

## Future Assets Needed:
- [ ] Character sprites (player, NPCs)
- [ ] Monster/enemy sprites
- [ ] UI elements (health bars, inventory)
- [ ] Particle effects
- [ ] Font files
