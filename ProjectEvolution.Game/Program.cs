using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 7 ===");
Console.WriteLine("=== THE ULTIMATE RPG CHALLENGE ===");
Console.WriteLine();
Console.WriteLine("Your Character:");
Console.WriteLine("  Strength: 2 (damage per attack)");
Console.WriteLine("  Defense: 1 (damage reduction)");
Console.WriteLine("  HP: 10");
Console.WriteLine();
Console.WriteLine("A gang of 3 goblins blocks your path!");
Console.WriteLine("Defeat them all to prove your worth!");
Console.WriteLine();

var game = new RPGGame();
game.StartMultiEnemyCombatWithStats(enemyCount: 3);
game.SetPlayerStats(strength: 2, defense: 1);

while (!game.CombatEnded)
{
    Console.WriteLine($"STR:{game.PlayerStrength} DEF:{game.PlayerDefense}  |  Gold:{game.PlayerGold}g  |  Your HP:{game.PlayerHP}/10  |  Goblin HP:{game.EnemyHP}/3  |  Remaining:{game.RemainingEnemies}");
    Console.WriteLine();
    Console.WriteLine("What will you do?");
    Console.WriteLine("1. Attack");
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

    game.ExecuteStatsMultiEnemyRoundWithRandomEnemy(action);

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
    Console.WriteLine("       LEGENDARY VICTORY!");
    Console.WriteLine("==========================================");
    Console.WriteLine($"All {3} goblins have fallen before you!");
    Console.WriteLine($"HP Remaining: {game.PlayerHP}/10");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
    Console.WriteLine();
    Console.WriteLine("You have proven yourself a true hero!");
}
else
{
    Console.WriteLine("           DEFEAT");
    Console.WriteLine("==========================================");
    Console.WriteLine("The goblins have bested you in combat.");
    Console.WriteLine($"Goblins Defeated: {3 - game.RemainingEnemies}/3");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
}
Console.WriteLine("==========================================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
