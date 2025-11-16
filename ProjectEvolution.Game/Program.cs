using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 2 ===");
Console.WriteLine();
Console.WriteLine("A weak goblin appears before you!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombat();

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
    Console.WriteLine("You raise your shield defensively...");
}
else
{
    Console.WriteLine();
    Console.WriteLine("Confused, you hesitate...");
    action = CombatAction.Defend; // Default to defend on invalid input
}

game.ChooseAction(action);

Console.WriteLine();
if (game.IsWon)
{
    Console.WriteLine("The goblin falls! Victory is yours!");
}
else
{
    Console.WriteLine("The goblin escapes! You failed to defeat it.");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
