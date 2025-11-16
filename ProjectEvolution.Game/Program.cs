using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 10 ===");
Console.WriteLine("=== ENEMY VARIETY ===");
Console.WriteLine();
Console.WriteLine("Enemies now come in different types!");
Console.WriteLine();
Console.WriteLine("  GOBLIN SCOUT:   2 HP, 1 damage (fast & weak)");
Console.WriteLine("  GOBLIN WARRIOR: 5 HP, 1 damage (tank)");
Console.WriteLine("  GOBLIN ARCHER:  3 HP, 2 damage (glass cannon)");
Console.WriteLine();
Console.WriteLine("You never know which one you'll face!");
Console.WriteLine("Stamina + Crits/Misses + Enemy Types = CHAOS!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithRandomEnemyType();
game.SetPlayerStats(strength: 2, defense: 1);

Console.WriteLine($"A {game.EnemyName} appears!");
Console.WriteLine($"Enemy: {game.EnemyHP} HP, {game.EnemyDamage} damage");
Console.WriteLine();

while (!game.CombatEnded)
{
    Console.WriteLine($"STAMINA:{game.PlayerStamina}/12  |  HP:{game.PlayerHP}/10  |  {game.EnemyName} HP:{game.EnemyHP}  |  Gold:{game.PlayerGold}g");
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

    game.ExecuteEnemyTypeCombatRoundWithRandomHits(action, CombatAction.Attack);

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
    Console.WriteLine($"The {game.EnemyName} falls defeated!");
    Console.WriteLine($"HP Remaining: {game.PlayerHP}/10");
    Console.WriteLine($"Stamina Left: {game.PlayerStamina}/12");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
}
else
{
    Console.WriteLine("           DEFEAT");
    Console.WriteLine("==========================================");
    Console.WriteLine($"The {game.EnemyName} has bested you!");
    Console.WriteLine("Run it again - different enemy = different fight!");
    Console.WriteLine($"Gold Earned: {game.PlayerGold}g");
}
Console.WriteLine("==========================================");

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
