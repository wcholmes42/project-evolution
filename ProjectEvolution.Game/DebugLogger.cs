namespace ProjectEvolution.Game;

/// <summary>
/// Simple debug logger that writes to file for self-review
/// </summary>
public static class DebugLogger
{
    private static string logPath = "debug_graphics.log";
    private static object lockObj = new object();

    static DebugLogger()
    {
        // Clear log on startup
        try
        {
            File.WriteAllText(logPath, $"=== Graphics Debug Log Started: {DateTime.Now} ===\n");
        }
        catch { }
    }

    public static void Log(string message)
    {
        lock (lockObj)
        {
            try
            {
                string timestamp = DateTime.Now.ToString("HH:mm:ss.fff");
                string logLine = $"[{timestamp}] {message}\n";

                // Write to file
                File.AppendAllText(logPath, logLine);

                // Also write to console
                Console.WriteLine(logLine.TrimEnd());
            }
            catch { }
        }
    }

    public static string GetLogPath()
    {
        return Path.GetFullPath(logPath);
    }
}
