using System.Text;

namespace ProjectEvolution.Game;

public class SafeFileWriter
{
    private static string _outputPath = "."; // Default to current directory
    private const int MAX_RETRIES = 5;
    private const int BASE_DELAY_MS = 100;

    // Configure output path (e.g., Unraid share mount point)
    public static void SetOutputPath(string path)
    {
        _outputPath = path;

        // Create directory if it doesn't exist
        try
        {
            if (!Directory.Exists(_outputPath))
            {
                Directory.CreateDirectory(_outputPath);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️  Warning: Could not create output directory {_outputPath}: {ex.Message}");
            Console.WriteLine("   Falling back to current directory.");
            _outputPath = ".";
        }
    }

    public static string GetOutputPath() => _outputPath;

    // Safe write with file lock protection and retry logic
    public static bool SafeWriteAllText(string filename, string content, bool silent = false)
    {
        string fullPath = Path.Combine(_outputPath, filename);
        string tempPath = Path.Combine(_outputPath, $"{filename}.tmp");

        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                // Write to temp file first (atomic operation)
                File.WriteAllText(tempPath, content, Encoding.UTF8);

                // Try to move/replace - this might fail if file is locked
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                File.Move(tempPath, fullPath);

                return true; // Success!
            }
            catch (IOException ex) when (IsFileLocked(ex))
            {
                // File is locked - retry with exponential backoff
                int delay = BASE_DELAY_MS * (int)Math.Pow(2, attempt);

                if (!silent)
                {
                    Console.SetCursorPosition(0, 35);
                    Console.ForegroundColor = ConsoleColor.DarkYellow;
                    Console.Write($"⏳ File locked: {filename} - Retry {attempt + 1}/{MAX_RETRIES} in {delay}ms...".PadRight(70));
                    Console.ResetColor();
                }

                Thread.Sleep(delay);
            }
            catch (UnauthorizedAccessException ex)
            {
                if (!silent)
                {
                    LogError($"Permission denied writing {filename}: {ex.Message}");
                }
                Thread.Sleep(BASE_DELAY_MS * 2);
            }
            catch (Exception ex)
            {
                if (!silent)
                {
                    LogError($"Error writing {filename}: {ex.Message}");
                }

                // For network errors, retry
                if (ex.Message.Contains("network") || ex.Message.Contains("share"))
                {
                    Thread.Sleep(BASE_DELAY_MS * 3);
                }
                else
                {
                    break; // Don't retry non-network errors
                }
            }
        }

        // All retries failed
        if (!silent)
        {
            LogError($"Failed to write {filename} after {MAX_RETRIES} attempts");
        }

        // Clean up temp file if it exists
        try
        {
            if (File.Exists(tempPath))
            {
                File.Delete(tempPath);
            }
        }
        catch { /* Ignore cleanup errors */ }

        return false;
    }

    // Safe append with lock protection
    public static bool SafeAppendAllText(string filename, string content, bool silent = true)
    {
        string fullPath = Path.Combine(_outputPath, filename);

        for (int attempt = 0; attempt < MAX_RETRIES; attempt++)
        {
            try
            {
                // Use FileStream with FileShare.Read to allow others to read while we append
                using (var stream = new FileStream(fullPath, FileMode.Append, FileAccess.Write, FileShare.Read))
                using (var writer = new StreamWriter(stream, Encoding.UTF8))
                {
                    writer.Write(content);
                }

                return true; // Success!
            }
            catch (IOException ex) when (IsFileLocked(ex))
            {
                int delay = BASE_DELAY_MS * (int)Math.Pow(2, attempt);
                Thread.Sleep(delay);
            }
            catch (Exception)
            {
                Thread.Sleep(BASE_DELAY_MS);
            }
        }

        return false; // Failed silently for appends (not critical)
    }

    private static bool IsFileLocked(IOException ex)
    {
        int errorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & 0xFFFF;
        // ERROR_SHARING_VIOLATION = 32, ERROR_LOCK_VIOLATION = 33
        return errorCode == 32 || errorCode == 33;
    }

    private static void LogError(string message)
    {
        try
        {
            Console.SetCursorPosition(0, 36);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.Write($"❌ {message}".PadRight(70));
            Console.ResetColor();
        }
        catch { /* Ignore if can't write error */ }
    }

    // Test if output path is accessible
    public static bool TestOutputPath()
    {
        try
        {
            string testFile = Path.Combine(_outputPath, ".write_test");
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"❌ Cannot write to {_outputPath}: {ex.Message}");
            Console.ResetColor();
            return false;
        }
    }

    // Get full path for a file
    public static string GetFullPath(string filename)
    {
        return Path.Combine(_outputPath, filename);
    }
}
