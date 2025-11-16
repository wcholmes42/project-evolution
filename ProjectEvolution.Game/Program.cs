using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 9 ===");
Console.WriteLine("=== STAMINA RESOURCE MANAGEMENT ===");
Console.WriteLine();
Console.WriteLine("You now have LIMITED STAMINA!");
Console.WriteLine("  Attack costs 3 stamina");
Console.WriteLine("  Defend costs 1 stamina");
Console.WriteLine("  Run out? Forced to defend!");
Console.WriteLine();
Console.WriteLine("PLUS: Crits & Misses still active!");
Console.WriteLine();
Console.WriteLine("Your Character:");
Console.WriteLine("  STR: 2 | DEF: 1 | HP: 10 | STAMINA: 12");
Console.WriteLine();
Console.WriteLine("A goblin warrior challenges you!");
Console.WriteLine("Manage your stamina wisely... or die!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithStamina();
game.SetPlayerStats(strength: 2, defense: 1);

while (!game.CombatEnded)
{
    Console.WriteLine($"STAMINA:{game.PlayerStamina}/12  |  HP:{game.PlayerHP}/10  |  Gold:{game.PlayerGold}g  |  Goblin HP:{game.EnemyHP}/3");
    Console.WriteLine();
    Console.WriteLine("What will you do?");
    Console.WriteLine("1. Attack (-3 stamina)");
    Console.WriteLine("2. Defend (-1 stamina)");
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

    game.ExecuteStaminaCombatRoundWithRandomHits(action, CombatAction.Attack);

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
