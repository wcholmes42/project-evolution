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
            // Auto-fit to current terminal size
            _width = Console.WindowWidth;
            _height = Console.WindowHeight;

            // Minimum size for proper display
            if (_width < 120 || _height < 30)
            {
                _width = Math.Max(_width, 120);
                _height = Math.Max(_height, 30);

                // Warn if terminal is too small
                if (!OperatingSystem.IsWindows())
                {
                    Console.WriteLine("⚠️  Terminal size warning:");
                    Console.WriteLine($"   Current: {Console.WindowWidth}x{Console.WindowHeight}");
                    Console.WriteLine($"   Recommended: 120x30 minimum for best display");
                    Console.WriteLine($"   Ideal: Use full screen (auto-fits to any size)");
                    System.Threading.Thread.Sleep(2000);
                }
            }

            // On Windows, ensure buffer matches window to prevent scrolling
            if (OperatingSystem.IsWindows())
            {
                try
                {
                    Console.SetBufferSize(_width, _height);
                }
                catch
                {
                    // Ignore if it fails
                }
            }
        }
        catch
        {
            // Fallback to reasonable defaults
            _width = 240; // 1920px / 8px per char
            _height = 67;  // 1080px / 16px per char
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
        ProgressionFrameworkData? currentCandidate,
        double currentCandidateFitness,
        ProgressionFrameworkData? bestFramework,
        ProgressionFrameworkData? previousBest,
        double previousFitness,
        List<MetricResult>? metrics,
        Queue<(int gen, double fit)> fitnessHistory,
        int populationSize = 0)
    {
        if (!_initialized) Initialize();

        // Validate all parameters before rendering
        if (bestFramework == null) return;

        // Use double-buffering properly (no Console.Clear to prevent flash)
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

        DrawColumn1_StatusAndMetrics(0, 4, col1Width, fitness, previousFitness, championFitness, championGen,
            stuckGens, resets, phase, elapsed, metrics, populationSize);

        // ═══════════════════════════════════════════════════════════════
        // COLUMN 2 (Middle): All Parameters (12 params)
        // ═══════════════════════════════════════════════════════════════

        DrawColumn2_Parameters(col1Width, 4, col2Width, currentCandidate, currentCandidateFitness, bestFramework, previousBest, previousFitness);

        // ═══════════════════════════════════════════════════════════════
        // COLUMN 3 (Right): Fitness Trend, Sparkline, Quality Guide
        // ═══════════════════════════════════════════════════════════════

        DrawColumn3_TrendAndGuide(col1Width + col2Width, 4, col3Width, fitnessHistory, fitness, stuckGens, generation, genPerSec);

        // ═══════════════════════════════════════════════════════════════
        // FOOTER: Controls and auto-reset status
        // ═══════════════════════════════════════════════════════════════

        DrawFooter(_height - 3, stuckGens, fitness, fitnessHistory);

        // Use differential rendering (no flash)
        _screen.Render();
    }

    private void DrawHeader(int generation, double fitness, double genPerSec)
    {
        // Top border
        _screen.WriteAt(0, 0, "╔" + new string('═', _width - 2) + "╗", ConsoleColor.Cyan);

        // Title with version
        string title = "🧬 PROGRESSION RESEARCH v2.0-DETERM-ADAPTIVE 🧬";
        int titleX = (_width - title.Length) / 2;
        _screen.WriteAt(titleX, 1, title, ConsoleColor.Yellow);

        // Stats line - clarify this is CURRENT BEST
        string stats = $"Gen: {generation,7} | {genPerSec:F0} gen/s | Current Best: {fitness:F2}";
        int statsX = (_width - stats.Length) / 2;
        _screen.WriteAt(statsX, 2, stats, ConsoleColor.White);

        // Bottom border
        _screen.WriteAt(0, 3, "╚" + new string('═', _width - 2) + "╝", ConsoleColor.Cyan);
    }

    private void DrawColumn1_StatusAndMetrics(int x, int y, int width, double fitness, double previousFitness,
        double championFitness, int championGen, int stuckGens, int resets,
        string phase, TimeSpan elapsed, List<MetricResult>? metrics, int populationSize = 0)
    {
        int row = y;
        int boxWidth = width - 2;
        int contentWidth = boxWidth - 4; // Account for padding and borders

        // Box: Status - expand to fit fitness comparison
        int statusHeight = previousFitness > 0 ? 11 : 9;
        _screen.WriteBox(x + 1, row, boxWidth, statusHeight, "STATUS", ConsoleColor.Cyan);
        row += 1;

        string quality = GetQualityBand(fitness);
        int boxX = x + 1;

        // Show current fitness with comparison to previous
        if (previousFitness > 0 && Math.Abs(fitness - previousFitness) > 0.01)
        {
            double delta = fitness - previousFitness;
            string deltaStr = delta > 0 ? $"+{delta:F2}" : $"{delta:F2}";
            ConsoleColor deltaColor = delta > 0 ? ConsoleColor.Green : ConsoleColor.Red;
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Current: {fitness:F2} ({deltaStr})", deltaColor);
        }
        else if (previousFitness > 0)
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Current: {fitness:F2} (unchanged)", ConsoleColor.DarkCyan);
        }
        else
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Current: {fitness:F2}", GetQualityColor(fitness));
        }

        // Show previous best for reference
        if (previousFitness > 0)
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Previous: {previousFitness:F2}", ConsoleColor.DarkGray);
        }

        WriteInBox(boxX, boxWidth, x + 3, row++, $"Runtime: {elapsed.Hours:D2}:{elapsed.Minutes:D2}:{elapsed.Seconds:D2}", ConsoleColor.Gray);
        WriteInBox(boxX, boxWidth, x + 3, row++, $"Phase: {phase}", ConsoleColor.Cyan);

        // Population diversity indicator (adaptive sizing!)
        if (populationSize > 0)
        {
            ConsoleColor popColor = populationSize >= 200 ? ConsoleColor.Magenta :
                                   populationSize >= 100 ? ConsoleColor.Cyan :
                                   populationSize >= 50 ? ConsoleColor.Green :
                                   populationSize >= 30 ? ConsoleColor.Yellow :
                                   populationSize >= 20 ? ConsoleColor.DarkYellow : ConsoleColor.Red;
            string sizeIndicator = populationSize >= 200 ? "HUGE" :
                                  populationSize >= 100 ? "XLRG" :
                                  populationSize >= 50 ? "LARG" :
                                  populationSize >= 30 ? "MED" :
                                  populationSize >= 20 ? "NORM" : "SMAL";
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Pop: {populationSize} ({sizeIndicator})", popColor);
        }

        // Stuck warning with color coding
        ConsoleColor stuckColor = stuckGens > 1000 ? ConsoleColor.Red :
                                 stuckGens > 500 ? ConsoleColor.DarkYellow :
                                 stuckGens > 200 ? ConsoleColor.Yellow : ConsoleColor.Gray;
        WriteInBox(boxX, boxWidth, x + 3, row++, $"Stuck: {stuckGens} gens", stuckColor);

        // Champion (all-time best across resets)
        if (championFitness > 0 && championFitness <= 100)
        {
            // Show if champion is outdated
            if (fitness > championFitness)
            {
                WriteInBox(boxX, boxWidth, x + 3, row++, $"Champion: {championFitness:F1} (Promoting...)", ConsoleColor.Yellow);
            }
            else
            {
                WriteInBox(boxX, boxWidth, x + 3, row++, $"Champion: {championFitness:F1} (Gen {championGen})", ConsoleColor.Magenta);
            }
        }
        else if (championFitness > 100)
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, "Champion: [Corrupted data]", ConsoleColor.DarkGray);
        }
        else
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, "Champion: None yet", ConsoleColor.DarkGray);
        }
        WriteInBox(boxX, boxWidth, x + 3, row++, $"Resets: {resets}", ConsoleColor.Gray);

        row = y + statusHeight + 1; // Start after status box

        // Box: Metrics - calculate safe height to not overflow screen
        if (metrics != null && metrics.Count > 0)
        {
            int availableHeight = _height - row - 5; // Space until footer
            int neededHeight = metrics.Count + 10; // Metrics + explanations + padding
            int boxHeight = Math.Min(availableHeight, neededHeight);

            if (boxHeight < 8) return; // Not enough room, skip metrics entirely

            _screen.WriteBox(x + 1, row, boxWidth, boxHeight, "METRICS", ConsoleColor.Yellow);
            int boxBottom = row + boxHeight - 1; // Bottom border line
            row += 2;

            int metricsRendered = 0;
            foreach (var metric in metrics)
            {
                // Stop if we'd go past the bottom border
                if (row >= boxBottom - 1) break;

                ConsoleColor color = metric.Score >= 80 ? ConsoleColor.Green :
                                    metric.Score >= 60 ? ConsoleColor.Yellow :
                                    ConsoleColor.Red;

                // Truncate metric name to fit
                int nameWidth = Math.Min(20, contentWidth - 30);
                string name = metric.MetricName.Length > nameWidth
                    ? metric.MetricName.Substring(0, nameWidth)
                    : metric.MetricName.PadRight(nameWidth);

                // Highlight Progression Strata if it's failing badly
                ConsoleColor nameColor = (metric.MetricName == "Progression Strata" && metric.Score < 20)
                    ? ConsoleColor.DarkRed
                    : ConsoleColor.White;
                _screen.WriteAt(x + 3, row, name, nameColor, contentWidth);

                // Progress bar - ensure it fits
                int barX = x + 3 + nameWidth + 1;
                int barWidth = Math.Min(20, contentWidth - nameWidth - 10);
                if (barWidth > 0)
                {
                    _screen.DrawProgressBar(barX, row, barWidth, metric.Score, 100, color, ConsoleColor.DarkGray);
                    _screen.WriteAt(barX + barWidth + 2, row, $"{metric.Score:F0}", color, 6);
                }

                row++;
                metricsRendered++;
            }

            // Metric explanations - only if we have room
            if (row < boxBottom - 7) // Need 7 lines for explanations
            {
                row++;
                WriteInBox(boxX, boxWidth, x + 3, row++, "What these mean:", ConsoleColor.DarkCyan);

                if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 3, row++, "Combat: 4-6 turn fights", ConsoleColor.DarkGray);
                if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 3, row++, "Economy: Can afford gear", ConsoleColor.DarkGray);

                // Find Progression Strata and show special warning if it's the problem
                var strataMetric = metrics?.FirstOrDefault(m => m.MetricName == "Progression Strata");
                if (strataMetric != null && strataMetric.Score < 20 && row < boxBottom - 2)
                {
                    WriteInBox(boxX, boxWidth, x + 3, row++, "Strata: 3 tier win rates", ConsoleColor.DarkRed);
                    if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 5, row++, "(HARD: needs 60/75/90%)", ConsoleColor.DarkGray);
                }
                else if (row < boxBottom - 1)
                {
                    WriteInBox(boxX, boxWidth, x + 3, row++, "Strata: Gear progression", ConsoleColor.DarkGray);
                }

                if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 3, row++, "Skills: Useful not OP", ConsoleColor.DarkGray);
                if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 3, row++, "Equipment: Meaningful ups", ConsoleColor.DarkGray);
                if (row < boxBottom - 1) WriteInBox(boxX, boxWidth, x + 3, row++, "Pacing: Smooth curve", ConsoleColor.DarkGray);
            }
        }
    }

    private void DrawColumn2_Parameters(int x, int y, int width,
        ProgressionFrameworkData? liveCandidate, double liveFitness,
        ProgressionFrameworkData? currentBest,
        ProgressionFrameworkData? previousBest,
        double previousFitness)
    {
        if (currentBest == null) return;

        int row = y;
        int boxWidth = width - 2;
        int boxX = x + 1;

        // Box: Show 4-column comparison: Live | Best | Prev | Δ
        _screen.WriteBox(boxX, row, boxWidth, 30, "PARAMS (Live|Best|Prev|Δ)", ConsoleColor.Green);
        row += 1;

        // Show fitness comparison and check if live is actually different
        bool liveIsDifferent = liveCandidate != null && !AreFrameworksEqual(liveCandidate, currentBest);
        ConsoleColor fitColor = liveIsDifferent ? ConsoleColor.Cyan : ConsoleColor.Red;
        string status = liveIsDifferent ? "EXPLORING" : "MONOCULTURE!";

        string fitLine = $"{status} Live={liveFitness:F1} Best={currentBest.Metadata.OverallFitness:F1}";
        if (previousFitness > 0)
        {
            fitLine += $" Δ{currentBest.Metadata.OverallFitness - previousFitness:+F1}";
        }
        WriteInBox(boxX, boxWidth, x + 3, row++, fitLine, fitColor);

        // Compact header with legend
        WriteInBox(boxX, boxWidth, x + 3, row++, "Live=trying Best=keeper Prev=old", ConsoleColor.DarkGray);
        WriteInBox(boxX, boxWidth, x + 3, row++, "Param    Live Best Prev   Δ", ConsoleColor.DarkCyan);
        row++;

        // Helper to format compact 4-column comparison row
        void RenderParamInt(string name, int? live, int best, int? prev)
        {
            int liveVal = live ?? best;
            string delta = prev.HasValue ? $"{best - prev.Value,+3}" : " - ";

            // Highlight live if different from best
            bool isDifferent = live.HasValue && live != best;
            ConsoleColor liveColor = isDifferent ? ConsoleColor.Cyan : ConsoleColor.Gray;
            // Highlight best if different from prev
            ConsoleColor bestColor = prev.HasValue && best != prev ? ConsoleColor.Yellow : ConsoleColor.White;

            // Show marker if live is exploring
            string liveStr = isDifferent ? $"{liveVal,3}*" : $"{liveVal,3} ";

            WriteInBox(boxX, boxWidth, x + 3, row, $"{name,-8} ", ConsoleColor.White);
            WriteInBox(boxX, boxWidth, x + 3 + 9, row, liveStr, liveColor);
            WriteInBox(boxX, boxWidth, x + 3 + 13, row, $"{best,3} ", bestColor);
            WriteInBox(boxX, boxWidth, x + 3 + 17, row, $"{(prev?.ToString() ?? "-"),3} {delta,4}", ConsoleColor.DarkGray);
            row++;
        }

        void RenderParamDouble(string name, double? live, double best, double? prev, string format = "F1")
        {
            double liveVal = live ?? best;
            string delta = prev.HasValue ? $"{best - prev.Value,+5:F1}" : "  -  ";

            bool isDifferent = live.HasValue && Math.Abs(live.Value - best) > 0.01;
            ConsoleColor liveColor = isDifferent ? ConsoleColor.Cyan : ConsoleColor.Gray;
            ConsoleColor bestColor = prev.HasValue && Math.Abs(best - prev.Value) > 0.01 ? ConsoleColor.Yellow : ConsoleColor.White;

            // Show marker if live is exploring
            string liveStr = isDifferent ? $"{liveVal.ToString(format),4}*" : $"{liveVal.ToString(format),4} ";

            WriteInBox(boxX, boxWidth, x + 3, row, $"{name,-8} ", ConsoleColor.White);
            WriteInBox(boxX, boxWidth, x + 3 + 9, row, liveStr, liveColor);
            WriteInBox(boxX, boxWidth, x + 3 + 14, row, $"{best.ToString(format),4} ", bestColor);
            WriteInBox(boxX, boxWidth, x + 3 + 19, row, $"{(prev?.ToString(format) ?? " - "),4} {delta}", ConsoleColor.DarkGray);
            row++;
        }

        // Render all 12 parameters in compact table format
        WriteInBox(boxX, boxWidth, x + 3, row++, "👤 PLAYER", ConsoleColor.Green);

        RenderParamInt("BaseHP", liveCandidate?.PlayerProgression.BaseHP, currentBest.PlayerProgression.BaseHP, previousBest?.PlayerProgression.BaseHP);
        RenderParamDouble("HP/Lvl", liveCandidate?.PlayerProgression.HPPerLevel, currentBest.PlayerProgression.HPPerLevel, previousBest?.PlayerProgression.HPPerLevel);
        RenderParamInt("BaseSTR", liveCandidate?.PlayerProgression.BaseSTR, currentBest.PlayerProgression.BaseSTR, previousBest?.PlayerProgression.BaseSTR);
        RenderParamInt("BaseDEF", liveCandidate?.PlayerProgression.BaseDEF, currentBest.PlayerProgression.BaseDEF, previousBest?.PlayerProgression.BaseDEF);
        RenderParamInt("StatPts", liveCandidate?.PlayerProgression.StatPointsPerLevel, currentBest.PlayerProgression.StatPointsPerLevel, previousBest?.PlayerProgression.StatPointsPerLevel);
        row++;

        WriteInBox(boxX, boxWidth, x + 3, row++, "👹 ENEMY", ConsoleColor.Red);
        RenderParamInt("BaseHP", liveCandidate?.EnemyProgression.BaseHP, currentBest.EnemyProgression.BaseHP, previousBest?.EnemyProgression.BaseHP);
        RenderParamDouble("HP×Lvl", liveCandidate?.EnemyProgression.HPScalingCoefficient, currentBest.EnemyProgression.HPScalingCoefficient, previousBest?.EnemyProgression.HPScalingCoefficient, "F2");
        RenderParamInt("BaseDMG", liveCandidate?.EnemyProgression.BaseDamage, currentBest.EnemyProgression.BaseDamage, previousBest?.EnemyProgression.BaseDamage);
        RenderParamDouble("DMG×Lvl", liveCandidate?.EnemyProgression.DamageScalingCoefficient, currentBest.EnemyProgression.DamageScalingCoefficient, previousBest?.EnemyProgression.DamageScalingCoefficient, "F2");
        row++;

        WriteInBox(boxX, boxWidth, x + 3, row++, "💰 ECONOMY", ConsoleColor.Yellow);
        RenderParamInt("BaseGold", liveCandidate?.Economy.BaseGoldPerCombat, currentBest.Economy.BaseGoldPerCombat, previousBest?.Economy.BaseGoldPerCombat);
        RenderParamDouble("Gold×Lvl", liveCandidate?.Economy.GoldScalingCoefficient, currentBest.Economy.GoldScalingCoefficient, previousBest?.Economy.GoldScalingCoefficient, "F2");
        row++;

        WriteInBox(boxX, boxWidth, x + 3, row++, "🎁 LOOT", ConsoleColor.Magenta);
        RenderParamInt("Treasure", liveCandidate?.Loot.BaseTreasureGold, currentBest.Loot.BaseTreasureGold, previousBest?.Loot.BaseTreasureGold);
        RenderParamInt("Loot×Dep", liveCandidate?.Loot.TreasurePerDungeonDepth, currentBest.Loot.TreasurePerDungeonDepth, previousBest?.Loot.TreasurePerDungeonDepth);
        RenderParamDouble("DropRate", liveCandidate?.Loot.EquipmentDropRate, currentBest.Loot.EquipmentDropRate, previousBest?.Loot.EquipmentDropRate, "F0");
    }

    private void DrawColumn3_TrendAndGuide(int x, int y, int width, Queue<(int gen, double fit)> history, double currentFitness, int stuckGens = 0, int currentGen = 0, double genPerSec = 600)
    {
        int row = y;
        int boxWidth = width - 2;
        int boxX = x + 1;

        // Box: Fitness Trend
        _screen.WriteBox(boxX, row, boxWidth, 18, "FITNESS TREND", ConsoleColor.Magenta);
        row += 2;

        if (history.Count >= 3)
        {
            var data = history.TakeLast(40).ToArray();
            double minFit = data.Length > 0 ? data.Min(d => d.fit) : 0;
            double maxFit = data.Length > 0 ? data.Max(d => d.fit) : 100;
            double range = maxFit - minFit;

            // Sparkline - truncate to fit
            string sparkline = "";
            int maxSparkWidth = boxWidth - 6;
            foreach (var point in data.Take(maxSparkWidth))
            {
                int level = range > 0.01 ? (int)((point.fit - minFit) / range * 7) : 4;
                char bar = level switch
                {
                    0 => '▁', 1 => '▂', 2 => '▃', 3 => '▄',
                    4 => '▅', 5 => '▆', 6 => '▇', _ => '█'
                };
                sparkline += bar;
            }

            WriteInBox(boxX, boxWidth, x + 3, row++, sparkline, ConsoleColor.Cyan);
            WriteInBox(boxX, boxWidth, x + 3, row++, $"Range: {minFit:F1} → {maxFit:F1}", ConsoleColor.Gray);

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
                    WriteInBox(boxX, boxWidth, x + 3, row++, $"Slope: +{slope:F3}/1k (STALE!)", ConsoleColor.Red);
                    WriteInBox(boxX, boxWidth, x + 3, row++, $"  Last improve: {gensSinceLastImprovement} gens ago", ConsoleColor.DarkGray);
                }
                else
                {
                    WriteInBox(boxX, boxWidth, x + 3, row++, $"Slope: +{slope:F3}/1k gens", slopeColor);
                }

                // ETA calculation with CORRECT MATH
                double[] targets = { 70, 75, 80, 85, 90 };
                double nextTarget = targets.FirstOrDefault(t => t > currentFitness);

                if (nextTarget > 0)
                {
                    // REALITY CHECK #1: Trend data is stale (no recent improvements)
                    if (trendStale || stuckGens > 100)
                    {
                        WriteInBox(boxX, boxWidth, x + 3, row++, $"ETA: STALE! Stuck {stuckGens} gens", ConsoleColor.Red);
                    }
                    // REALITY CHECK #2: Slope too low
                    else if (slope < 0.05)
                    {
                        WriteInBox(boxX, boxWidth, x + 3, row++, $"ETA: Plateau ({slope:F2}/1k too low)", ConsoleColor.DarkYellow);
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

                        WriteInBox(boxX, boxWidth, x + 3, row++, $"ETA to {nextTarget:F0}: ~{eta}", etaColor);
                        WriteInBox(boxX, boxWidth, x + 3, row++, $"  ({gensNeeded:F0} gens @ {genPerSec:F0}/s)", ConsoleColor.DarkGray);
                    }
                    else
                    {
                        WriteInBox(boxX, boxWidth, x + 3, row++, "ETA: Unknown (no trend)", ConsoleColor.Gray);
                    }
                }
            }
        }
        else
        {
            WriteInBox(boxX, boxWidth, x + 3, row++, "Collecting data...", ConsoleColor.DarkGray);
            WriteInBox(boxX, boxWidth, x + 3, row++, $"({history.Count}/3 improvements)", ConsoleColor.DarkGray);
        }

        row = y + 20; // Start AFTER FITNESS TREND box

        // HEALTH MONITOR instead of static quality guide
        DrawHealthMonitor(boxX, row, boxWidth, x + 3, currentFitness, stuckGens, currentGen, history, genPerSec);
    }

    private void DrawHealthMonitor(int boxX, int row, int boxWidth, int textX, double fitness, int stuckGens, int gen, Queue<(int gen, double fit)> history, double genPerSec)
    {
        _screen.WriteBox(boxX, row, boxWidth, 16, "TUNER HEALTH", ConsoleColor.Yellow);
        row += 2;

        // Quality band - no emojis for alignment
        string quality = fitness >= 90 ? "OPTIMAL" :
                        fitness >= 80 ? "EXCELLENT" :
                        fitness >= 70 ? "GOOD" :
                        fitness >= 60 ? "FAIR" : "POOR";
        ConsoleColor qualityColor = fitness >= 80 ? ConsoleColor.Green :
                                    fitness >= 70 ? ConsoleColor.Yellow :
                                    fitness >= 60 ? ConsoleColor.Cyan : ConsoleColor.Red;
        WriteInBox(boxX, boxWidth, textX, row++, $"Quality: {quality} ({fitness:F1})", qualityColor);

        // Throughput health
        ConsoleColor throughputColor = genPerSec > 100 ? ConsoleColor.Green :
                                       genPerSec > 10 ? ConsoleColor.Yellow : ConsoleColor.Red;
        WriteInBox(boxX, boxWidth, textX, row++, $"Throughput: {genPerSec:F0} gen/s", throughputColor);

        // Stuckness warning
        ConsoleColor stuckColor = stuckGens > 1000 ? ConsoleColor.Red :
                                  stuckGens > 500 ? ConsoleColor.DarkYellow :
                                  stuckGens > 200 ? ConsoleColor.Yellow : ConsoleColor.Green;
        WriteInBox(boxX, boxWidth, textX, row++, $"Stuckness: {stuckGens} gens", stuckColor);

        // Trend health
        if (history.Count >= 10)
        {
            var data = history.ToArray();
            double slope = CalculateTrend(history);
            ConsoleColor trendColor = slope > 1.0 ? ConsoleColor.Green :
                                     slope > 0.1 ? ConsoleColor.Yellow :
                                     slope > 0.01 ? ConsoleColor.DarkYellow : ConsoleColor.Red;
            WriteInBox(boxX, boxWidth, textX, row++, $"Trend: +{slope:F3}/1k", trendColor);
        }
        row++;

        row++; // Spacing

        // WARNINGS - show what auto-actions are happening (no emojis for alignment)
        // Note: "stuck" means gens since beating current best (may be at champion already)
        if (stuckGens > 5000)
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: At champion peak", ConsoleColor.Yellow);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Explore neighbors", ConsoleColor.DarkGray);
        }
        else if (stuckGens > 2000)
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: Near optimum", ConsoleColor.Yellow);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Fine-tune search", ConsoleColor.DarkGray);
        }
        else if (stuckGens > 1000)
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: SEVERELY STUCK", ConsoleColor.Red);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Champion guide", ConsoleColor.Yellow);
        }
        else if (stuckGens > 500)
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: Plateau", ConsoleColor.DarkYellow);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Diversity boost", ConsoleColor.Yellow);
        }
        else if (stuckGens > 200)
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: Slow progress", ConsoleColor.Yellow);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Random jumps", ConsoleColor.DarkGray);
        }
        else
        {
            WriteInBox(boxX, boxWidth, textX, row++, "STATUS: Healthy", ConsoleColor.Green);
            WriteInBox(boxX, boxWidth, textX, row++, "Action: Normal crossover", ConsoleColor.DarkGray);
        }
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
            // Truncate if too long
            if (msg.Length > _width - 4) msg = msg.Substring(0, _width - 7) + "...";
            _screen.WriteAt(2, y + 1, msg, ConsoleColor.Red, _width - 4);
        }
        else
        {
            string msg = "🔄 Auto-reset when: fitness ≥85 + stuck 150 OR trend <0.05/1k + stuck 100";
            // Truncate if too long
            if (msg.Length > _width - 4) msg = msg.Substring(0, _width - 7) + "...";
            _screen.WriteAt(2, y + 1, msg, ConsoleColor.DarkCyan, _width - 4);
        }

        // Controls - center and truncate if needed
        string controls = "[ESC] Stop & Save  |  [R] Manual Reset → Champion";
        if (controls.Length > _width - 4) controls = controls.Substring(0, _width - 7) + "...";
        int controlsX = Math.Max(2, (_width - controls.Length) / 2);
        _screen.WriteAt(controlsX, y + 2, controls, ConsoleColor.Yellow, _width - controlsX - 2);
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

    // Helper: Write text inside a box with automatic truncation
    // boxX = left edge of box, boxWidth = total width including borders, textX = where text starts
    private void WriteInBox(int boxX, int boxWidth, int textX, int y, string text, ConsoleColor color = ConsoleColor.White)
    {
        // Calculate space from text position to right border (║)
        int rightBorder = boxX + boxWidth - 1;
        int availableWidth = rightBorder - textX - 1; // -1 to not overwrite the right border

        if (availableWidth <= 0) return; // No space to write

        // Truncate if needed
        if (text.Length > availableWidth)
        {
            int truncLen = Math.Max(0, availableWidth - 3);
            text = text.Substring(0, truncLen) + "...";
        }

        _screen.WriteAt(textX, y, text, color, availableWidth);
    }

    // Helper: Check if two frameworks are equal (all 12 params match)
    private bool AreFrameworksEqual(ProgressionFrameworkData? a, ProgressionFrameworkData? b)
    {
        if (a == null || b == null) return false;

        return a.PlayerProgression.BaseHP == b.PlayerProgression.BaseHP &&
               Math.Abs(a.PlayerProgression.HPPerLevel - b.PlayerProgression.HPPerLevel) < 0.01 &&
               a.PlayerProgression.BaseSTR == b.PlayerProgression.BaseSTR &&
               a.PlayerProgression.BaseDEF == b.PlayerProgression.BaseDEF &&
               a.PlayerProgression.StatPointsPerLevel == b.PlayerProgression.StatPointsPerLevel &&
               a.EnemyProgression.BaseHP == b.EnemyProgression.BaseHP &&
               Math.Abs(a.EnemyProgression.HPScalingCoefficient - b.EnemyProgression.HPScalingCoefficient) < 0.01 &&
               a.EnemyProgression.BaseDamage == b.EnemyProgression.BaseDamage &&
               Math.Abs(a.EnemyProgression.DamageScalingCoefficient - b.EnemyProgression.DamageScalingCoefficient) < 0.01 &&
               a.Economy.BaseGoldPerCombat == b.Economy.BaseGoldPerCombat &&
               Math.Abs(a.Economy.GoldScalingCoefficient - b.Economy.GoldScalingCoefficient) < 0.01 &&
               a.Loot.BaseTreasureGold == b.Loot.BaseTreasureGold &&
               a.Loot.TreasurePerDungeonDepth == b.Loot.TreasurePerDungeonDepth &&
               Math.Abs(a.Loot.EquipmentDropRate - b.Loot.EquipmentDropRate) < 0.1;
    }
}
