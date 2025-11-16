namespace ProjectEvolution.Game;

public class GameLogger
{
    private List<string> _eventLog = new List<string>();
    private string _logFilePath;

    public GameLogger(string logFilePath = "game_log.txt")
    {
        _logFilePath = logFilePath;
    }

    public void LogEvent(string eventType, string message)
    {
        string timestamp = DateTime.Now.ToString("HH:mm:ss");
        string logEntry = $"[{timestamp}] {eventType}: {message}";
        _eventLog.Add(logEntry);

        // Also write to file
        try
        {
            File.AppendAllText(_logFilePath, logEntry + Environment.NewLine);
        }
        catch
        {
            // Silently fail if can't write to file
        }
    }

    public void LogStateChange(string property, object oldValue, object newValue)
    {
        LogEvent("STATE", $"{property}: {oldValue} → {newValue}");
    }

    public void DumpGameState(RPGGame game, string reason)
    {
        var dump = new List<string>
        {
            "",
            "═══════════════════════════════════════════════════════",
            "          GAME STATE DUMP",
            $"Reason: {reason}",
            "═══════════════════════════════════════════════════════",
            "",
            "PLAYER STATE:",
            $"  Level: {game.PlayerLevel}",
            $"  HP: {game.PlayerHP}/{game.MaxPlayerHP}",
            $"  XP: {game.PlayerXP}/{game.XPForNextLevel}",
            $"  Gold: {game.PlayerGold}g",
            $"  Strength: {game.PlayerStrength}",
            $"  Defense: {game.PlayerDefense}",
            $"  Stamina: {game.PlayerStamina}/12",
            $"  Potions: {game.PotionCount}",
            $"  Stat Points: {game.AvailableStatPoints}",
            "",
            "LOCATION:",
            $"  Position: ({game.PlayerX}, {game.PlayerY})",
            $"  Terrain: {game.GetCurrentTerrain()}",
            $"  In Location: {game.InLocation}",
            $"  In Dungeon: {game.InDungeon}",
            $"  Dungeon Depth: {game.DungeonDepth}",
            "",
            "PROGRESS:",
            $"  Combats Won: {game.CombatsWon}",
            $"  Deaths: {game.DeathCount}",
            $"  Run Ended: {game.RunEnded}",
            "",
            "COMBAT STATE:",
            $"  In Combat: {!game.CombatEnded}",
            $"  Combat Won: {game.IsWon}",
            $"  Enemy: {game.EnemyName} [Lvl {game.EnemyLevel}]",
            $"  Enemy HP: {game.EnemyHP}",
            $"  Enemy Damage: {game.EnemyDamage}",
            "",
            "═══════════════════════════════════════════════════════",
            ""
        };

        foreach (var line in dump)
        {
            Console.WriteLine(line);
            LogEvent("DUMP", line);
        }
    }

    public void ShowRecentEvents(int count = 20)
    {
        Console.WriteLine("\n═══ RECENT EVENTS ═══");
        int start = Math.Max(0, _eventLog.Count - count);
        for (int i = start; i < _eventLog.Count; i++)
        {
            Console.WriteLine(_eventLog[i]);
        }
    }

    public List<string> GetEventLog() => new List<string>(_eventLog);
}
