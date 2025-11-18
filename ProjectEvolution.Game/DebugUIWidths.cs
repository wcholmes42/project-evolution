using ProjectEvolution.Game;

namespace ProjectEvolution.Game;

/// <summary>
/// Debug tool to verify display width calculations
/// </summary>
public static class DebugUIWidths
{
    public static void TestWidths()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.Clear();

        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("              DISPLAY WIDTH DEBUG TOOL");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        var testStrings = new[]
        {
            "[P] Play Game (Normal Mode)",
            "[X] X-MEN MUTATION MODE 🦄 (Find The Unicorn!)",
            "[V] PROGRESSION TUNER (Test Levels & Builds) 🆕",
            "[E] EVOLUTIONARY TUNER (Continuous Evolution) 🧬",
            "[M] PROGRESSION RESEARCH (Formula Discovery) 📊",
            "[B] CONTINUOUS RESEARCH → CODE GEN (Auto-evolve!) 🔄",
        };

        foreach (var str in testStrings)
        {
            int strLen = str.Length;
            int displayWidth = UIFramework.GetDisplayWidth(str);
            Console.WriteLine($"String Length: {strLen,2} | Display Width: {displayWidth,2} | Text: {str}");
        }

        Console.WriteLine("\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                    BOX ALIGNMENT TEST");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Draw a box with known width to verify alignment
        int boxWidth = 64;
        Console.WriteLine($"╔{new string('═', boxWidth)}╗");

        foreach (var str in testStrings)
        {
            string padded = UIFramework.PadToWidth("  " + str, boxWidth);
            int actualWidth = UIFramework.GetDisplayWidth(padded);
            Console.WriteLine($"║{padded}║ ({actualWidth})");
        }

        Console.WriteLine($"╚{new string('═', boxWidth)}╝");

        Console.WriteLine("\n\nPress any key to continue...");
        Console.ReadKey();
    }
}
