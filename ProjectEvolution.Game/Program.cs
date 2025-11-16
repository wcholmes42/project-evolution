using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 4 ===");
Console.WriteLine();
Console.WriteLine("A goblin warrior blocks your path!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithHP();

while (!game.CombatEnded)
{
    Console.WriteLine($"Your HP: {game.PlayerHP}/10  |  Goblin HP: {game.EnemyHP}/3");
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

    game.ExecuteHPCombatRoundWithRandomEnemy(action);

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
    Console.WriteLine("VICTORY! The goblin falls!");
    Console.WriteLine($"You survived with {game.PlayerHP} HP remaining.");
}
else
{
    Console.WriteLine("DEFEAT! You have fallen in battle!");
}
Console.WriteLine("======================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
