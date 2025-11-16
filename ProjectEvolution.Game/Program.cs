using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 8 ===");
Console.WriteLine("=== CRITICAL HITS & MISSES ===");
Console.WriteLine();
Console.WriteLine("Combat is now UNPREDICTABLE!");
Console.WriteLine("  15% chance to MISS (no damage)");
Console.WriteLine("  15% chance to CRIT (2x damage!)");
Console.WriteLine("  70% chance for normal hit");
Console.WriteLine();
Console.WriteLine("Your Character:");
Console.WriteLine("  Strength: 2 | Defense: 1 | HP: 10");
Console.WriteLine();
Console.WriteLine("A goblin warrior challenges you!");
Console.WriteLine("Every attack is a gamble... good luck!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithCrits();
game.SetPlayerStats(strength: 2, defense: 1);

while (!game.CombatEnded)
{
    Console.WriteLine($"STR:{game.PlayerStrength} DEF:{game.PlayerDefense}  |  Gold:{game.PlayerGold}g  |  Your HP:{game.PlayerHP}/10  |  Goblin HP:{game.EnemyHP}/3");
    Console.WriteLine();
    Console.WriteLine("What will you do?");
    Console.WriteLine("1. Attack (pray for crit!)");
    Console.WriteLine("2. Defend");
    Console.Write("> ");

    var choice = Console.ReadLine();

    CombatAction action;
    if (choice == "1")
    {
        action = CombatAction.Attack;
    }
    else if (choice == "2")
    {
        action = CombatAction.Defend;
    }
    else
    {
        Console.WriteLine("Invalid choice! Defaulting to Defend.");
        action = CombatAction.Defend;
    }

    game.ExecuteCritCombatRoundWithRandomHits(action, CombatAction.Attack);

    Console.WriteLine();
    Console.WriteLine(game.CombatLog);
    Console.WriteLine();

    if (!game.CombatEnded)
    {
        Console.WriteLine("--- Fight continues ---");
        Console.WriteLine();
    }
}

Console.WriteLine();
Console.WriteLine("==========================================");
if (game.IsWon)
{
    Console.WriteLine("       VICTORY!");
    Console.WriteLine("==========================================");
    Console.WriteLine("The goblin falls defeated!");
    Console.WriteLine($"HP Remaining: {game.PlayerHP}/10");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
    Console.WriteLine();
    Console.WriteLine("RNG was on your side today!");
}
else
{
    Console.WriteLine("           DEFEAT");
    Console.WriteLine("==========================================");
    Console.WriteLine("The goblin has bested you in combat.");
    Console.WriteLine("Maybe you missed too many attacks...");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
}
Console.WriteLine("==========================================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
