using Raylib_CsLo;
using System.Numerics;
using System.Linq;

namespace ProjectEvolution.Game;

/// <summary>
/// Graphical combat screen for turn-based battles (Ultima IV meets modern UI)
/// </summary>
public class GraphicsCombatScreen
{
    private const int LARGE_SPRITE_SIZE = 48; // 3x scaled tiles for combat
    private int screenWidth;
    private int screenHeight;
    private List<string> combatLog = new List<string>();
    private const int MAX_LOG_LINES = 6;

    // Animation state
    private float playerHealthAnimated = 0;
    private float enemyHealthAnimated = 0;
    private List<DamageNumber> damageNumbers = new List<DamageNumber>();
    private float screenShake = 0;

    public GraphicsCombatScreen(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
    }

    /// <summary>
    /// Render the full combat screen
    /// </summary>
    public void Render(RPGGame game, GraphicsRenderer renderer)
    {
        // Apply screen shake
        int shakeX = (int)(Math.Sin(screenShake * 20) * screenShake * 10);
        int shakeY = (int)(Math.Cos(screenShake * 15) * screenShake * 10);
        screenShake = Math.Max(0, screenShake - 0.05f);

        // Dark background
        Raylib.DrawRectangle(0, 0, screenWidth, screenHeight, new Color(20, 20, 30, 255));

        // Combat arena (centered)
        int arenaWidth = 1600;
        int arenaHeight = 1000;
        int arenaX = (screenWidth - arenaWidth) / 2 + shakeX;
        int arenaY = (screenHeight - arenaHeight) / 2 + shakeY;

        // Arena border
        Raylib.DrawRectangleLines(arenaX, arenaY, arenaWidth, arenaHeight, Raylib.GOLD);

        // Title: "COMBAT"
        DrawCenteredText("⚔️ COMBAT ⚔️", arenaY + 20, 32, Raylib.GOLD);

        // Player panel (left)
        RenderCharacterPanel(
            game,
            renderer,
            arenaX + 100,
            arenaY + 100,
            isPlayer: true
        );

        // VS text
        DrawCenteredText("VS", arenaY + 250, 48, Raylib.RED);

        // Enemy panel (right)
        RenderCharacterPanel(
            game,
            renderer,
            arenaX + arenaWidth - 400,
            arenaY + 100,
            isPlayer: false
        );

        // Combat log (bottom center)
        RenderCombatLog(arenaX + 50, arenaY + 450, arenaWidth - 100);

        // Action buttons (bottom)
        RenderActionButtons(arenaX + 50, arenaY + 650);

        // Damage numbers floating up
        RenderDamageNumbers();

        // Update animations
        UpdateAnimations(game);
    }

    private void RenderCharacterPanel(RPGGame game, GraphicsRenderer renderer, int x, int y, bool isPlayer)
    {
        // Panel background
        Raylib.DrawRectangle(x - 10, y - 10, 320, 350, new Color(40, 40, 50, 200));
        Raylib.DrawRectangleLines(x - 10, y - 10, 320, 350, isPlayer ? Raylib.SKYBLUE : Raylib.RED);

        // Character sprite (large 3x scaled)
        int tileId = isPlayer ? TileMapper.PLAYER_TILE : TileMapper.GetMobTileId(game.CurrentEnemyType);
        renderer.DrawTileAt(tileId, x + 120, y, null);
        // Draw it even larger (3x the scaled size = 9x original)
        for (int i = 0; i < 3; i++)
        {
            for (int j = 0; j < 3; j++)
            {
                renderer.DrawTileAt(tileId, x + 100 + (i * 16), y + (j * 16), null);
            }
        }

        // Name
        string name = isPlayer ? "HERO" : game.EnemyName;
        int nameY = y + 60;
        Raylib.DrawText(name, x + 150 - (name.Length * 6), nameY, 24, Raylib.WHITE);

        // Level
        int level = isPlayer ? game.PlayerLevel : game.EnemyLevel;
        Raylib.DrawText($"Level {level}", x + 110, nameY + 30, 18, Raylib.GRAY);

        // HP (with animated bar)
        int hp = isPlayer ? game.PlayerHP : game.EnemyHP;
        int maxHp = isPlayer ? game.MaxPlayerHP : game.MaxEnemyHP;

        // Animate HP towards actual value
        ref float animatedHp = ref (isPlayer ? ref playerHealthAnimated : ref enemyHealthAnimated);
        if (animatedHp == 0) animatedHp = hp; // Initialize
        animatedHp = animatedHp + (hp - animatedHp) * 0.15f; // Smooth lerp

        int hpY = nameY + 60;
        Raylib.DrawText($"HP: {hp}/{maxHp}", x + 20, hpY, 20, Raylib.WHITE);

        // HP Bar
        int barWidth = 280;
        int barHeight = 24;
        int barX = x + 10;
        int barY = hpY + 25;

        // Background
        Raylib.DrawRectangle(barX, barY, barWidth, barHeight, Raylib.DARKGRAY);

        // Fill (animated)
        float fillPercent = Math.Clamp(animatedHp / maxHp, 0, 1);
        int fillWidth = (int)(barWidth * fillPercent);
        Color hpColor = fillPercent > 0.5f ? Raylib.GREEN :
                       fillPercent > 0.25f ? Raylib.YELLOW :
                       Raylib.RED;
        Raylib.DrawRectangle(barX, barY, fillWidth, barHeight, hpColor);

        // Border
        Raylib.DrawRectangleLines(barX, barY, barWidth, barHeight, Raylib.WHITE);

        // Stats
        int statsY = barY + 35;
        if (isPlayer)
        {
            Raylib.DrawText($"STR: {game.GetEffectiveStrength()}", x + 20, statsY, 18, Raylib.ORANGE);
            Raylib.DrawText($"DEF: {game.GetEffectiveDefense()}", x + 160, statsY, 18, Raylib.SKYBLUE);

            // Show buffs
            int buffY = statsY + 30;
            var buffs = game.PlayerBuffs;
            if (buffs.Any(b => b.Type == BuffType.BerserkerRage))
                Raylib.DrawText("🔥 RAGE", x + 20, buffY, 16, Raylib.RED);
            if (buffs.Any(b => b.Type == BuffType.DefensiveStance))
                Raylib.DrawText("🛡️ DEFENSE", x + 140, buffY, 16, Raylib.BLUE);
        }
        else
        {
            // Enemy abilities
            Raylib.DrawText($"DMG: {game.EnemyDamage}", x + 20, statsY, 18, Raylib.RED);

            // Show enemy abilities
            int abilityY = statsY + 30;
            if (game.CurrentEnemyAbility != EnemyAbility.None)
            {
                string abilityText = game.CurrentEnemyAbility.ToString();
                Raylib.DrawText(abilityText, x + 20, abilityY, 14, Raylib.YELLOW);
            }
        }
    }

    private void RenderCombatLog(int x, int y, int width)
    {
        // Log panel
        int height = 180;
        Raylib.DrawRectangle(x, y, width, height, new Color(30, 30, 40, 220));
        Raylib.DrawRectangleLines(x, y, width, height, Raylib.GRAY);

        // Title
        Raylib.DrawText("COMBAT LOG", x + 10, y + 5, 16, Raylib.LIGHTGRAY);

        // Log entries (show last 6 lines)
        int logY = y + 30;
        int startIndex = Math.Max(0, combatLog.Count - MAX_LOG_LINES);
        for (int i = startIndex; i < combatLog.Count; i++)
        {
            string line = combatLog[i];
            Color color = Raylib.WHITE;

            // Color code based on content
            if (line.Contains("CRITICAL") || line.Contains("defeated"))
                color = Raylib.RED;
            else if (line.Contains("healed") || line.Contains("Victory"))
                color = Raylib.GREEN;
            else if (line.Contains("missed"))
                color = Raylib.GRAY;

            Raylib.DrawText(line, x + 15, logY, 14, color);
            logY += 20;
        }
    }

    private void RenderActionButtons(int x, int y)
    {
        // Button layout
        var buttons = new[]
        {
            ("[A] Attack", Raylib.RED),
            ("[D] Defend", Raylib.BLUE),
            ("[S] Skills", Raylib.PURPLE),
            ("[F] Flee", Raylib.YELLOW),
            ("[P] Potion", Raylib.GREEN)
        };

        int buttonWidth = 200;
        int buttonSpacing = 20;
        int totalWidth = (buttonWidth * buttons.Length) + (buttonSpacing * (buttons.Length - 1));
        int startX = x + (1100 - totalWidth) / 2; // Center buttons

        for (int i = 0; i < buttons.Length; i++)
        {
            var (text, color) = buttons[i];
            int bx = startX + (i * (buttonWidth + buttonSpacing));

            // Button background
            Raylib.DrawRectangle(bx, y, buttonWidth, 40, new Color(color.r / 3, color.g / 3, color.b / 3, 255));
            Raylib.DrawRectangleLines(bx, y, buttonWidth, 40, color);

            // Button text (centered)
            int textWidth = text.Length * 10;
            Raylib.DrawText(text, bx + (buttonWidth - textWidth) / 2, y + 10, 18, color);
        }
    }

    private void RenderDamageNumbers()
    {
        for (int i = damageNumbers.Count - 1; i >= 0; i--)
        {
            var dmg = damageNumbers[i];
            dmg.Update();

            if (dmg.IsExpired())
            {
                damageNumbers.RemoveAt(i);
            }
            else
            {
                dmg.Render();
            }
        }
    }

    private void UpdateAnimations(RPGGame game)
    {
        // Smooth HP animation happens in RenderCharacterPanel
        // This is called each frame to keep things moving
    }

    /// <summary>
    /// Add a message to the combat log
    /// </summary>
    public void LogMessage(string message)
    {
        combatLog.Add(message);

        // Parse damage and create floating numbers
        if (message.Contains("dealt") || message.Contains("takes"))
        {
            // Extract damage number (simple parsing)
            var words = message.Split(' ');
            foreach (var word in words)
            {
                if (int.TryParse(word, out int damage))
                {
                    bool isPlayerDamage = message.Contains("Hero") || message.Contains("You");
                    ShowDamageNumber(damage, isPlayerDamage);
                    break;
                }
            }
        }

        // Critical hits cause screen shake
        if (message.Contains("CRITICAL"))
        {
            ScreenShake(0.5f);
        }
    }

    /// <summary>
    /// Clear the combat log
    /// </summary>
    public void ClearLog()
    {
        combatLog.Clear();
    }

    /// <summary>
    /// Show a damage number floating up
    /// </summary>
    public void ShowDamageNumber(int damage, bool isPlayer)
    {
        int x = isPlayer ? screenWidth / 2 + 300 : screenWidth / 2 - 300;
        int y = screenHeight / 2 - 100;
        damageNumbers.Add(new DamageNumber(damage, x, y, isPlayer ? Raylib.RED : Raylib.ORANGE));
    }

    /// <summary>
    /// Trigger screen shake effect
    /// </summary>
    public void ScreenShake(float intensity)
    {
        screenShake = Math.Max(screenShake, intensity);
    }

    /// <summary>
    /// Reset combat screen state for new battle
    /// </summary>
    public void Reset()
    {
        combatLog.Clear();
        playerHealthAnimated = 0;
        enemyHealthAnimated = 0;
        damageNumbers.Clear();
        screenShake = 0;
    }

    private void DrawCenteredText(string text, int y, int fontSize, Color color)
    {
        int textWidth = text.Length * (fontSize / 2);
        Raylib.DrawText(text, (screenWidth - textWidth) / 2, y, fontSize, color);
    }
}

/// <summary>
/// Floating damage number animation
/// </summary>
class DamageNumber
{
    private int damage;
    private float x;
    private float y;
    private Color color;
    private float alpha = 1.0f;
    private float age = 0;
    private const float LIFETIME = 1.5f; // 1.5 seconds

    public DamageNumber(int damage, int x, int y, Color color)
    {
        this.damage = damage;
        this.x = x;
        this.y = y;
        this.color = color;
    }

    public void Update()
    {
        age += 0.016f; // ~60 FPS
        y -= 1.5f; // Float upward
        alpha = 1.0f - (age / LIFETIME); // Fade out
    }

    public void Render()
    {
        var fadeColor = new Color(color.r, color.g, color.b, (byte)(alpha * 255));
        int fontSize = 32 + (int)(age * 10); // Grow slightly
        Raylib.DrawText($"-{damage}", (int)x, (int)y, fontSize, fadeColor);
    }

    public bool IsExpired()
    {
        return age >= LIFETIME;
    }
}
