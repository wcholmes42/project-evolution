using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 5 ===");
Console.WriteLine();
Console.WriteLine("Welcome, adventurer! Your quest for gold begins...");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithLoot();

while (!game.CombatEnded)
{
    Console.WriteLine($"Gold: {game.PlayerGold}g  |  Your HP: {game.PlayerHP}/10  |  Goblin HP: {game.EnemyHP}/3");
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

    game.ExecuteLootCombatRoundWithRandomEnemy(action);

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
    Console.WriteLine($"Total Gold: {game.PlayerGold}g");
}
else
{
    Console.WriteLine("DEFEAT! You have fallen in battle!");
    Console.WriteLine($"Total Gold: {game.PlayerGold}g");
}
Console.WriteLine("======================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
