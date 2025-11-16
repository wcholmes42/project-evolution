using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 3 ===");
Console.WriteLine();
Console.WriteLine("A goblin warrior appears, ready for battle!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithAI();

Console.WriteLine("What will you do?");
Console.WriteLine("1. Attack");
Console.WriteLine("2. Defend");
Console.Write("> ");

var choice = Console.ReadLine();

CombatAction action;
if (choice == "1")
{
    action = CombatAction.Attack;
    Console.WriteLine();
    Console.WriteLine("You swing your sword at the goblin!");
}
else if (choice == "2")
{
    action = CombatAction.Defend;
    Console.WriteLine();
    Console.WriteLine("You raise your shield...");
}
else
{
    Console.WriteLine();
    Console.WriteLine("Confused, you hesitate and raise your shield...");
    action = CombatAction.Defend;
}

game.ChooseActionWithRandomEnemy(action);

Console.WriteLine();
Console.WriteLine(game.CombatLog);
Console.WriteLine();

if (game.IsWon)
{
    Console.WriteLine("Victory! The goblin falls defeated!");
}
else
{
    Console.WriteLine("Defeat! The goblin bested you in combat!");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
