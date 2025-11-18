using ProjectEvolution.Game;

namespace ProjectEvolution.Game;

/// <summary>
/// Test program to demonstrate UIFramework capabilities
/// </summary>
public static class UIFrameworkTest
{
    public static void RunTests()
    {
        Console.Clear();
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("           UI FRAMEWORK TEST - Display Width Tests");
        Console.WriteLine("═══════════════════════════════════════════════════════════════\n");

        // Test 1: Display width calculation
        Console.WriteLine("TEST 1: Display Width Calculation");
        Console.WriteLine("─────────────────────────────────────────────");
        TestDisplayWidth("Hello World", 11);
        TestDisplayWidth("X-MEN MUTATION MODE 🦄", 23);
        TestDisplayWidth("EVOLUTIONARY 🧬", 16);
        TestDisplayWidth("RESEARCH 📊", 12);
        TestDisplayWidth("Arrow → Test", 13);
        Console.WriteLine();

        // Test 2: Simple Menu
        Console.WriteLine("TEST 2: Simple Menu with Emojis");
        Console.WriteLine("─────────────────────────────────────────────\n");

        var menuOptions = new Dictionary<string, string>
        {
            { "P", "Play Game (Normal Mode)" },
            { "T", "Manual Testing & Tuning (Interactive)" },
            { "X", "X-MEN MUTATION MODE 🦄 (Find The Unicorn!)" },
            { "V", "PROGRESSION TUNER (Test Levels & Builds) 🆕" },
            { "E", "EVOLUTIONARY TUNER (Continuous Evolution) 🧬" },
            { "M", "PROGRESSION RESEARCH (Formula Discovery) 📊" },
            { "B", "CONTINUOUS RESEARCH → CODE GEN (Auto-evolve!) 🔄" },
            { "Q", "Quit" }
        };

        string menu = UIFramework.CreateMenu(
            "PROJECT EVOLUTION - TEST MENU",
            "Emoji Alignment Test",
            menuOptions,
            width: 64
        );
        Console.WriteLine(menu);
        Console.WriteLine();

        // Test 3: Message Box
        Console.WriteLine("TEST 3: Message Box");
        Console.WriteLine("─────────────────────────────────────────────\n");
        Console.WriteLine(UIFramework.CreateMessageBox("Test Successful! 🎉", width: 50));
        Console.WriteLine();

        // Test 4: Status Panel
        Console.WriteLine("TEST 4: Status Panel");
        Console.WriteLine("─────────────────────────────────────────────\n");
        var stats = new Dictionary<string, string>
        {
            { "Health", "100/100 ❤️" },
            { "Mana", "50/50 🔵" },
            { "Level", "10 ⭐" },
            { "Gold", "1,234 💰" }
        };
        Console.WriteLine(UIFramework.CreateStatusPanel("Character Stats", stats, width: 50));
        Console.WriteLine();

        // Test 5: Custom Box with Builder
        Console.WriteLine("TEST 5: Custom Box Builder");
        Console.WriteLine("─────────────────────────────────────────────\n");
        var customBox = UIFramework.CreateBox(60)
            .WithTitle("ACHIEVEMENT UNLOCKED! 🏆")
            .WithSubtitle("Master of UI Frameworks")
            .WithDivider()
            .AddEmptyLine()
            .AddLine("You have successfully created a UI framework that:", indent: 2)
            .AddLine("✓ Handles emojis correctly", indent: 4)
            .AddLine("✓ Calculates display widths accurately", indent: 4)
            .AddLine("✓ Auto-pads content perfectly", indent: 4)
            .AddLine("✓ Works with Unicode characters →", indent: 4)
            .AddEmptyLine()
            .AddLine("Reward: +1000 XP 🎉", UIFramework.PadDirection.Center, indent: 0)
            .AddEmptyLine();

        Console.WriteLine(customBox.Build());
        Console.WriteLine();

        // Test 6: Table
        Console.WriteLine("TEST 6: Table with Data");
        Console.WriteLine("─────────────────────────────────────────────\n");
        var headers = new[] { "Class", "Level", "Status" };
        var rows = new List<string[]>
        {
            new[] { "Warrior ⚔️", "10", "Active" },
            new[] { "Mage 🧙", "8", "Active" },
            new[] { "Rogue 🗡️", "12", "Active" }
        };
        Console.WriteLine(UIFramework.CreateTable(headers, rows, width: 60));

        Console.WriteLine("\n\n═══════════════════════════════════════════════════════════════");
        Console.WriteLine("                  All Tests Complete!");
        Console.WriteLine("═══════════════════════════════════════════════════════════════");
        Console.WriteLine("\nPress any key to exit...");
        Console.ReadKey();
    }

    private static void TestDisplayWidth(string text, int expectedWidth)
    {
        int actualWidth = UIFramework.GetDisplayWidth(text);
        string status = actualWidth == expectedWidth ? "✓ PASS" : "✗ FAIL";
        Console.WriteLine($"{status} | \"{text}\" = {actualWidth} chars (expected: {expectedWidth})");
    }
}
