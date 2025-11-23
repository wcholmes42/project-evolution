using Raylib_CsLo;

namespace ProjectEvolution.Game;

/// <summary>
/// Dramatic death screen for graphics mode (Dark Souls inspired)
/// </summary>
public class GraphicsDeathScreen
{
    private int screenWidth;
    private int screenHeight;
    private float fadeAlpha = 0;
    private int countdownSeconds = 3;

    public GraphicsDeathScreen(int width, int height)
    {
        screenWidth = width;
        screenHeight = height;
    }

    /// <summary>
    /// Show the complete death sequence with fade effects
    /// </summary>
    public void ShowDeathSequence(RPGGame game, string killerName, int goldLost, List<string> droppedItems)
    {
        // PHASE 1: Fade to red (dramatic death)
        FadeToRed();

        // PHASE 2: Show death screen with stats
        ShowDeathStats(killerName, goldLost, droppedItems, game.TotalDeaths);

        // PHASE 3: Countdown to respawn
        ShowRespawnCountdown();

        // PHASE 4: Fade to black
        FadeToBlack();

        // PHASE 5: Fade back in at temple
        FadeIn();
    }

    private void FadeToRed()
    {
        for (float alpha = 0; alpha < 0.8f; alpha += 0.02f)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            // Red overlay
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight,
                new Color((byte)180, (byte)20, (byte)20, (byte)(alpha * 255)));

            Raylib.EndDrawing();
            System.Threading.Thread.Sleep(20); // ~50 FPS fade
        }
    }

    private void ShowDeathStats(string killerName, int goldLost, List<string> droppedItems, int totalDeaths)
    {
        fadeAlpha = 0;

        // Show death screen for 4 seconds with fade-in effect
        for (int frame = 0; frame < 240; frame++) // 4 seconds at 60 FPS
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(new Color((byte)20, (byte)10, (byte)10, (byte)255)); // Dark red background

            // Fade in text
            if (fadeAlpha < 1.0f)
                fadeAlpha += 0.02f;

            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            // Main "YOU DIED" banner (Dark Souls style)
            string deathText = "YOU DIED";
            int textWidth = MeasureTextWidth(deathText, 96);
            DrawTextWithAlpha(deathText, centerX - textWidth / 2, centerY - 150, 96, Raylib.WHITE);

            // Separator line
            Raylib.DrawRectangle(centerX - 300, centerY - 80, 600, 2,
                new Color((byte)255, (byte)255, (byte)255, (byte)(fadeAlpha * 255)));

            // Death statistics
            int statsY = centerY - 40;
            DrawTextWithAlpha($"Slain by: {killerName}", centerX - 200, statsY, 28, Raylib.LIGHTGRAY);

            statsY += 50;
            if (goldLost > 0)
            {
                DrawTextWithAlpha($"Gold lost: {goldLost}g", centerX - 200, statsY, 24, Raylib.GOLD);
                statsY += 40;
            }

            if (droppedItems.Count > 0)
            {
                DrawTextWithAlpha("Items dropped:", centerX - 200, statsY, 20, Raylib.RED);
                statsY += 30;
                foreach (var item in droppedItems)
                {
                    DrawTextWithAlpha($"  • {item}", centerX - 180, statsY, 18, Raylib.ORANGE);
                    statsY += 25;
                }
                statsY += 15;
            }

            // Death count
            DrawTextWithAlpha($"Deaths: {totalDeaths}", centerX - 200, statsY + 20, 20, Raylib.GRAY);

            // Hint
            DrawTextWithAlpha("Your corpse awaits at your death location...", centerX - 250, centerY + 150, 16, Raylib.DARKGRAY);

            Raylib.EndDrawing();
            System.Threading.Thread.Sleep(16); // ~60 FPS
        }
    }

    private void ShowRespawnCountdown()
    {
        for (int countdown = 3; countdown > 0; countdown--)
        {
            for (int frame = 0; frame < 60; frame++) // 1 second per count
            {
                Raylib.BeginDrawing();
                Raylib.ClearBackground(new Color((byte)20, (byte)10, (byte)10, (byte)255));

                int centerX = screenWidth / 2;
                int centerY = screenHeight / 2;

                // Pulsing countdown number
                float pulse = 1.0f + (float)Math.Sin(frame * 0.1f) * 0.1f;
                int fontSize = (int)(128 * pulse);
                string countText = countdown.ToString();
                int textWidth = MeasureTextWidth(countText, fontSize);

                Raylib.DrawText(countText, centerX - textWidth / 2, centerY - 100, fontSize,
                    new Color((byte)255, (byte)200, (byte)200, (byte)255));

                // Message
                string msg = "Respawning at Temple...";
                int msgWidth = MeasureTextWidth(msg, 24);
                Raylib.DrawText(msg, centerX - msgWidth / 2, centerY + 80, 24, Raylib.LIGHTGRAY);

                Raylib.EndDrawing();
                System.Threading.Thread.Sleep(16); // ~60 FPS
            }
        }
    }

    private void FadeToBlack()
    {
        for (float alpha = 0; alpha <= 1.0f; alpha += 0.05f)
        {
            Raylib.BeginDrawing();
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight,
                new Color((byte)0, (byte)0, (byte)0, (byte)(alpha * 255)));
            Raylib.EndDrawing();
            System.Threading.Thread.Sleep(30);
        }
    }

    private void FadeIn()
    {
        // Quick fade in at temple
        for (float alpha = 1.0f; alpha >= 0; alpha -= 0.1f)
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.BLACK);

            // Draw black overlay (fading out)
            Raylib.DrawRectangle(0, 0, screenWidth, screenHeight,
                new Color((byte)0, (byte)0, (byte)0, (byte)(alpha * 255)));

            Raylib.EndDrawing();
            System.Threading.Thread.Sleep(50);
        }
    }

    private void DrawTextWithAlpha(string text, int x, int y, int fontSize, Color color)
    {
        var fadedColor = new Color(color.r, color.g, color.b, (byte)(fadeAlpha * 255));
        Raylib.DrawText(text, x, y, fontSize, fadedColor);
    }

    private int MeasureTextWidth(string text, int fontSize)
    {
        // Approximate text width (Raylib's default font is roughly fontSize/2 per char)
        return text.Length * (fontSize / 2);
    }

    /// <summary>
    /// Quick death flash (for testing or minimal death feedback)
    /// </summary>
    public void ShowQuickDeath(string killerName)
    {
        for (int i = 0; i < 60; i++) // 1 second flash
        {
            Raylib.BeginDrawing();
            Raylib.ClearBackground(Raylib.RED);

            int centerX = screenWidth / 2;
            int centerY = screenHeight / 2;

            string text = $"KILLED BY {killerName.ToUpper()}";
            int textWidth = MeasureTextWidth(text, 48);
            Raylib.DrawText(text, centerX - textWidth / 2, centerY - 24, 48, Raylib.WHITE);

            Raylib.EndDrawing();
            System.Threading.Thread.Sleep(16);
        }
    }
}
