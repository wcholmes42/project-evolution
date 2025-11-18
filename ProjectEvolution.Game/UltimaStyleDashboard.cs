namespace ProjectEvolution.Game;

/// <summary>
/// Ultima 4-era AAA quality dashboard for progression research
/// Full-screen (240x67), rock-solid alignment, zero flicker
/// Uses entire 1920x1080 terminal space with retro perfection
/// </summary>
public class UltimaStyleDashboard
{
    private ScreenBuffer _screen;
    private int _width;
    private int _height;
    private bool _initialized = false;

    public void Initialize()
    {
        try
        {
            // Detect actual console size, fallback to 1920x1080 equivalent
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;
        }
        catch
        {
            _width = 240; // 1920px / 8px per char
            _height = 67;  // 1080px / 16px per char
        }

        // Minimum size for proper display
        if (_width < 120 || _height < 30)
        {
            _width = Math.Max(_width, 120);
            _height = Math.Max(_height, 30);
        }

        _screen = new ScreenBuffer(_width, _height);
        _initialized = true;

        Console.CursorVisible = false;
        Console.Clear();
    }

    public void RenderResearchDashboard(
        int generation,
        double fitness,
        double championFitness,
        int championGen,
        double genPerSec,
        int stuckGens,
        int resets,
        string phase,
        TimeSpan elapsed,
        ProgressionFrameworkData? framework,
        List<MetricResult>? metrics,
        Queue<(int gen, double fit)> fitnessHistory)
    {
        if (!_initialized) Initialize();

        _screen.SwapBuffers();
        _screen.Clear(' ', ConsoleColor.Black);

        // ═══════════════════════════════════════════════════════════════
        // LAYOUT: Full-screen 3-column design
        // ═══════════════════════════════════════════════════════════════

        int col1Width = _width / 3;           // Left: Status & Metrics
        int col2Width = _width / 3;           // Middle: Parameters
        int col3Width = _width - col1Width - col2Width; // Right: Trend & Guide

        // ═══════════════════════════════════════════════════════════════
        // HEADER: Full-width banner
        // ═══════════════════════════════════════════════════════════════

        DrawHeader(generation, fitness, genPerSec);

        // ═══════════════════════════════════════════════════════════════
        // COLUMN 1 (Left): Status, Metrics, Strategy
        // ═══════════════════════════════════════════════════════════════

        DrawColumn1_StatusAndMetrics(0, 4, col1Width, fitness, championFitness, championGen,
            stuckGens, resets, phase, elapsed, metrics);

        // ═══════════════════════════════════════════════════════════════
        // COLUMN 2 (Middle): All Parameters (12 params)
        // ═══════════════════════════════════════════════════════════════

        DrawColumn2_Parameters(col1Width, 4, col2Width, framework);

        // ═══════════════════════════════════════════════════════════════
        // COLUMN 3 (Right): Fitness Trend, Sparkline, Quality Guide
        // ═══════════════════════════════════════════════════════════════

        DrawColumn3_TrendAndGuide(col1Width + col2Width, 4, col3Width, fitnessHistory, fitness, stuckGens, generation, genPerSec);

        // ═══════════════════════════════════════════════════════════════
        // FOOTER: Controls and auto-reset status
        // ═══════════════════════════════════════════════════════════════

        DrawFooter(_height - 3, stuckGens, fitness, fitnessHistory);

        // Double-buffered render (only changed characters)
        _screen.Render();
    }

    private void DrawHeader(int generation, double fitness, double genPerSec)
    {
        // Top border
        _screen.WriteAt(0, 0, "╔" + new string('═', _width - 2) + "╗", ConsoleColor.Cyan);

        // Title
        string title = "🧬 PROGRESSION FRAMEWORK RESEARCH - DEMOSCENE EDITION 🧬";
        int titleX = (_width - title.Length) / 2;
        _screen.WriteAt(titleX, 1, title, ConsoleColor.Yellow);

        // Stats line
        string stats = $"Gen: {generation,7} | {genPerSec:F0} gen/s | Fitness: {fitness:F2}";
        int statsX = (_width - stats.Length) / 2;
        _screen.WriteAt(statsX, 2, stats, ConsoleColor.White);

        // Bottom border
        _screen.WriteAt(0, 3, "╚" + new string('═', _width - 2) + "╝", ConsoleColor.Cyan);
    }

    private void DrawColumn1_StatusAndMetrics(int x, int y, int width, double fitness,
        double championFitness, int championGen, int stuckGens, int resets,
        string phase, TimeSpan elapsed, List<MetricResult>? metrics)
    {
        int row = y;

        // Box: Status
        _screen.WriteBox(x + 1, row, width - 2, 8, "STATUS", ConsoleColor.Cyan);
        row += 1;

        string quality = GetQualityBand(fitness);
        _screen.WriteAt(x + 3, row++, $"Quality: {quality}", GetQualityColor(fitness));
        _screen.WriteAt(x + 3, row++, $"Runtime: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}", ConsoleColor.Gray);
        _screen.WriteAt(x + 3, row++, $"Phase: {phase}", ConsoleColor.Cyan);
        _screen.WriteAt(x + 3, row++, $"Stuck: {stuckGens} gens", ConsoleColor.Gray);

        if (championFitness > 0)
        {
            _screen.WriteAt(x + 3, row++, $"Champion: {championFitness:F1} (Gen {championGen})", ConsoleColor.Magenta);
            _screen.WriteAt(x + 3, row++, $"Resets: {resets}", ConsoleColor.Gray);
        }

        row = y + 9;

        // Box: Metrics
        if (metrics != null && metrics.Count > 0)
        {
            _screen.WriteBox(x + 1, row, width - 2, metrics.Count + 10, "METRICS", ConsoleColor.Yellow);
            row += 2;

            foreach (var metric in metrics)
            {
                ConsoleColor color = metric.Score >= 80 ? ConsoleColor.Green :
                                    metric.Score >= 60 ? ConsoleColor.Yellow :
                                    ConsoleColor.Red;

                string name = metric.MetricName.PadRight(20);
                _screen.WriteAt(x + 3, row, name, ConsoleColor.White);

                // Progress bar (20 chars wide)
                int barX = x + 24;
                _screen.DrawProgressBar(barX, row, 20, metric.Score, 100, color, ConsoleColor.DarkGray);

                // Score
                _screen.WriteAt(barX + 22, row, $"{metric.Score,5:F1}", color);

                row++;
            }

            // Metric explanations
            row++;
            _screen.WriteAt(x + 3, row++, "What these mean:", ConsoleColor.DarkCyan);
            _screen.WriteAt(x + 3, row++, "Combat: 4-6 turn fights", ConsoleColor.DarkGray);
            _screen.WriteAt(x + 3, row++, "Economy: Can afford gear", ConsoleColor.DarkGray);
            _screen.WriteAt(x + 3, row++, "Skills: Useful not OP", ConsoleColor.DarkGray);
            _screen.WriteAt(x + 3, row++, "Equipment: Meaningful ups", ConsoleColor.DarkGray);
            _screen.WriteAt(x + 3, row++, "Pacing: Smooth curve", ConsoleColor.DarkGray);
        }
    }

    private void DrawColumn2_Parameters(int x, int y, int width, ProgressionFrameworkData? framework)
    {
        if (framework == null) return;

        int row = y;

        // Box: All Parameters
        _screen.WriteBox(x + 1, row, width - 2, 28, "PARAMETERS (12 Total)", ConsoleColor.Green);
        row += 2;

        // Player params (5)
        _screen.WriteAt(x + 3, row++, "👤 PLAYER PROGRESSION", ConsoleColor.Green);
        _screen.WriteAt(x + 5, row++, $"BaseHP: {framework.PlayerProgression.BaseHP,3}  (Start health)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"HP/Lvl: {framework.PlayerProgression.HPPerLevel:F1}  (Growth rate)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"BaseSTR: {framework.PlayerProgression.BaseSTR,2}  (Start attack)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"BaseDEF: {framework.PlayerProgression.BaseDEF,2}  (Start defense)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"StatPts: {framework.PlayerProgression.StatPointsPerLevel,2}  (Per level)", ConsoleColor.White);
        row++;

        // Enemy params (4)
        _screen.WriteAt(x + 3, row++, "👹 ENEMY SCALING", ConsoleColor.Red);
        _screen.WriteAt(x + 5, row++, $"BaseHP: {framework.EnemyProgression.BaseHP,3}  (Goblin start)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"HP×Lvl: {framework.EnemyProgression.HPScalingCoefficient:F2}  (Growth factor)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"BaseDMG: {framework.EnemyProgression.BaseDamage,2}  (Start damage)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"DMG×Lvl: {framework.EnemyProgression.DamageScalingCoefficient:F2}  (Dmg growth)", ConsoleColor.White);
        row++;

        // Economy params (2)
        string ecoStatus = framework.Economy.CanAffordProgression ? "✅" : "❌";
        _screen.WriteAt(x + 3, row++, $"💰 ECONOMY {ecoStatus}", ConsoleColor.Yellow);
        _screen.WriteAt(x + 5, row++, $"BaseGold: {framework.Economy.BaseGoldPerCombat,3}g  (Per combat)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"Gold×Lvl: {framework.Economy.GoldScalingCoefficient:F2}  (Scaling)", ConsoleColor.White);
        row++;

        // Loot params (3)
        _screen.WriteAt(x + 3, row++, "🎁 LOOT & TREASURE", ConsoleColor.Magenta);
        _screen.WriteAt(x + 5, row++, $"BaseLoot: {framework.Loot.BaseTreasureGold,3}g  (Chest gold)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"Loot×Dep: {framework.Loot.TreasurePerDungeonDepth,3}g  (Per floor)", ConsoleColor.White);
        _screen.WriteAt(x + 5, row++, $"DropRate: {framework.Loot.EquipmentDropRate:F0}%  (Gear chance)", ConsoleColor.White);
    }

    private void DrawColumn3_TrendAndGuide(int x, int y, int width, Queue<(int gen, double fit)> history, double currentFitness, int stuckGens = 0, int currentGen = 0, double genPerSec = 600)
    {
        int row = y;

        // Box: Fitness Trend
        _screen.WriteBox(x + 1, row, width - 2, 18, "FITNESS TREND", ConsoleColor.Magenta);
        row += 2;

        if (history.Count >= 3)
        {
            var data = history.TakeLast(40).ToArray();
            double minFit = data.Length > 0 ? data.Min(d => d.fit) : 0;
            double maxFit = data.Length > 0 ? data.Max(d => d.fit) : 100;
            double range = maxFit - minFit;

            // Sparkline
            string sparkline = "";
            foreach (var point in data)
            {
                int level = range > 0.01 ? (int)((point.fit - minFit) / range * 7) : 4;
                char bar = level switch
                {
                    0 => '▁', 1 => '▂', 2 => '▃', 3 => '▄',
                    4 => '▅', 5 => '▆', 6 => '▇', _ => '█'
                };
                sparkline += bar;
            }

            _screen.WriteAt(x + 3, row++, sparkline, ConsoleColor.Cyan);
            _screen.WriteAt(x + 3, row++, $"Range: {minFit:F1} → {maxFit:F1}", ConsoleColor.Gray);

            // Calculate trend and check if data is STALE
            if (data.Length >= 10)
            {
                var oldest = data[0];
                var newest = data[data.Length - 1];
                double genDelta = newest.gen - oldest.gen;
                double fitDelta = newest.fit - oldest.fit;
                double slope = genDelta > 0 ? fitDelta / genDelta * 1000 : 0;

                // CRITICAL: Check if trend data is STALE!
                int gensSinceLastImprovement = currentGen - newest.gen;
                bool trendStale = gensSinceLastImprovement > 50; // Trend from >50 gens ago = stale!

                ConsoleColor slopeColor = slope > 0.5 ? ConsoleColor.Green :
                                         slope > 0.1 ? ConsoleColor.Yellow :
                                         slope > 0 ? ConsoleColor.DarkYellow :
                                         ConsoleColor.Red;

                if (trendStale)
                {
                    _screen.WriteAt(x + 3, row++, $"Slope: +{slope:F3}/1k (STALE!)", ConsoleColor.Red);
                    _screen.WriteAt(x + 3, row++, $"  Last improve: {gensSinceLastImprovement} gens ago", ConsoleColor.DarkGray);
                }
                else
                {
                    _screen.WriteAt(x + 3, row++, $"Slope: +{slope:F3}/1k gens", slopeColor);
                }

                // ETA calculation with CORRECT MATH
                double[] targets = { 70, 75, 80, 85, 90 };
                double nextTarget = targets.FirstOrDefault(t => t > currentFitness);

                if (nextTarget > 0)
                {
                    // REALITY CHECK #1: Trend data is stale (no recent improvements)
                    if (trendStale || stuckGens > 100)
                    {
                        _screen.WriteAt(x + 3, row++, $"ETA: STALE! Stuck {stuckGens} gens", ConsoleColor.Red);
                    }
                    // REALITY CHECK #2: Slope too low
                    else if (slope < 0.05)
                    {
                        _screen.WriteAt(x + 3, row++, $"ETA: Plateau ({slope:F2}/1k too low)", ConsoleColor.DarkYellow);
                    }
                    // Calculate ETA with ACTUAL throughput
                    else if (slope > 0.01)
                    {
                        double fitnessNeeded = nextTarget - currentFitness;
                        double gensNeeded = fitnessNeeded / (slope / 1000.0); // How many gens to gain that fitness
                        double secondsNeeded = gensNeeded / Math.Max(1, genPerSec); // Use ACTUAL throughput!

                        string eta;
                        ConsoleColor etaColor;

                        if (secondsNeeded < 60)
                        {
                            eta = $"{secondsNeeded:F0}sec";
                            etaColor = ConsoleColor.Green;
                        }
                        else if (secondsNeeded < 3600)
                        {
                            eta = $"{secondsNeeded / 60:F1}min";
                            etaColor = secondsNeeded < 1800 ? ConsoleColor.Green : ConsoleColor.Yellow;
                        }
                        else if (secondsNeeded < 86400)
                        {
                            eta = $"{secondsNeeded / 3600:F1}hrs";
                            etaColor = ConsoleColor.Yellow;
                        }
                        else
                        {
                            eta = $"{secondsNeeded / 86400:F1}days";
                            etaColor = ConsoleColor.DarkYellow;
                        }

                        _screen.WriteAt(x + 3, row++, $"ETA to {nextTarget:F0}: ~{eta}", etaColor);
                        _screen.WriteAt(x + 3, row++, $"  ({gensNeeded:F0} gens @ {genPerSec:F0}/s)", ConsoleColor.DarkGray);
                    }
                    else
                    {
                        _screen.WriteAt(x + 3, row++, "ETA: Unknown (no trend)", ConsoleColor.Gray);
                    }
                }
            }
        }
        else
        {
            _screen.WriteAt(x + 3, row++, "Collecting data...", ConsoleColor.DarkGray);
            _screen.WriteAt(x + 3, row++, $"({history.Count}/3 improvements)", ConsoleColor.DarkGray);
        }

        row = y + 16;

        // Box: Quality Guide
        _screen.WriteBox(x + 1, row, width - 2, 20, "QUALITY GUIDE", ConsoleColor.Yellow);
        row += 2;

        _screen.WriteAt(x + 3, row++, "50-60: ⚠️  POOR", ConsoleColor.Red);
        _screen.WriteAt(x + 5, row++, "Broken economy/combat", ConsoleColor.DarkGray);
        _screen.WriteAt(x + 5, row++, "1-2 hours to fix", ConsoleColor.DarkGray);
        row++;

        _screen.WriteAt(x + 3, row++, "60-70: 📊 FAIR", ConsoleColor.DarkYellow);
        _screen.WriteAt(x + 5, row++, "Functional, rough edges", ConsoleColor.DarkGray);
        _screen.WriteAt(x + 5, row++, "2-4 hours to polish", ConsoleColor.DarkGray);
        row++;

        _screen.WriteAt(x + 3, row++, "70-80: ✅ GOOD", ConsoleColor.Yellow);
        _screen.WriteAt(x + 5, row++, "Playable and fun!", ConsoleColor.DarkGray);
        _screen.WriteAt(x + 5, row++, "4-8 hours to achieve", ConsoleColor.DarkGray);
        row++;

        _screen.WriteAt(x + 3, row++, "80-90: ⭐ EXCELLENT", ConsoleColor.Green);
        _screen.WriteAt(x + 5, row++, "Production ready!", ConsoleColor.DarkGray);
        _screen.WriteAt(x + 5, row++, "8-24 hours", ConsoleColor.DarkGray);
        row++;

        _screen.WriteAt(x + 3, row++, "90+: 🏆 OPTIMAL", ConsoleColor.Cyan);
        _screen.WriteAt(x + 5, row++, "Near perfect balance", ConsoleColor.DarkGray);
        _screen.WriteAt(x + 5, row++, "Days/weeks", ConsoleColor.DarkGray);
    }

    private void DrawFooter(int y, int stuckGens, double fitness, Queue<(int gen, double fit)> history)
    {
        _screen.DrawHorizontalLine(0, y, _width, ConsoleColor.DarkGray);

        // Check auto-reset triggers
        double trend = CalculateTrend(history);
        bool trigger1 = fitness >= 85 && stuckGens >= 150;
        bool trigger2 = history.Count >= 20 && trend < 0.05 && stuckGens >= 100;

        if (trigger1 || trigger2)
        {
            string msg = "🔄 AUTO-RESET IMMINENT! ";
            if (trigger1) msg += "[High fitness stuck] ";
            if (trigger2) msg += $"[Plateau {trend:F2}/1k] ";
            _screen.WriteAt(2, y + 1, msg, ConsoleColor.Red);
        }
        else
        {
            string msg = "🔄 Auto-reset when: fitness ≥85 + stuck 150 OR trend <0.05/1k + stuck 100";
            _screen.WriteAt(2, y + 1, msg, ConsoleColor.DarkCyan);
        }

        // Controls
        string controls = "[ESC] Stop & Save  |  [R] Manual Reset → Champion";
        _screen.WriteAt((_width - controls.Length) / 2, y + 2, controls, ConsoleColor.Yellow);
    }

    private double CalculateTrend(Queue<(int gen, double fit)> history)
    {
        if (history.Count < 10) return 10.0;

        var data = history.ToArray();
        var oldest = data[0];
        var newest = data[data.Length - 1];

        double genDelta = newest.gen - oldest.gen;
        double fitDelta = newest.fit - oldest.fit;

        return genDelta > 0 ? fitDelta / genDelta * 1000 : 0;
    }

    private string GetQualityBand(double fitness)
    {
        if (fitness >= 90) return "🏆 OPTIMAL";
        if (fitness >= 80) return "⭐ EXCELLENT";
        if (fitness >= 70) return "✅ GOOD";
        if (fitness >= 60) return "📊 FAIR";
        if (fitness >= 50) return "⚠️  POOR";
        return "❌ BROKEN";
    }

    private ConsoleColor GetQualityColor(double fitness)
    {
        if (fitness >= 80) return ConsoleColor.Green;
        if (fitness >= 70) return ConsoleColor.Yellow;
        if (fitness >= 60) return ConsoleColor.Cyan;
        return ConsoleColor.Red;
    }
}
