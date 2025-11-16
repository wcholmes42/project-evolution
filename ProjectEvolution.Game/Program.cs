using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 6 ===");
Console.WriteLine();
Console.WriteLine("A gang of goblins blocks your path!");
Console.WriteLine("Defeat all 3 to claim victory!");
Console.WriteLine();

var game = new RPGGame();
game.StartMultiEnemyCombat(enemyCount: 3);

while (!game.CombatEnded)
{
    Console.WriteLine($"Gold: {game.PlayerGold}g  |  Your HP: {game.PlayerHP}/10  |  Goblin HP: {game.EnemyHP}/3  |  Remaining: {game.RemainingEnemies}");
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

    game.ExecuteMultiEnemyRoundWithRandomEnemy(action);

    Console.WriteLine();
    Console.WriteLine(game.CombatLog);
    Console.WriteLine();

    if (!game.CombatEnded)
    {
        Console.WriteLine("--- Round continues ---");
        Console.WriteLine();
    }
}

Console.WriteLine("======================");
if (game.IsWon)
{
    Console.WriteLine("EPIC VICTORY! All goblins defeated!");
    Console.WriteLine($"You survived with {game.PlayerHP} HP remaining.");
    Console.WriteLine($"Total Gold Earned: {game.PlayerGold}g");
}
else
{
    Console.WriteLine("DEFEAT! The goblins overwhelmed you!");
    Console.WriteLine($"You managed to defeat {3 - game.RemainingEnemies} of 3 goblins.");
    Console.WriteLine($"Total Gold: {game.PlayerGold}g");
}
Console.WriteLine("======================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
