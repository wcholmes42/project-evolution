using ProjectEvolution.Game;

Console.WriteLine("=== Project Evolution - Generation 11 ===");
Console.WriteLine("=== VARIABLE ENEMY STATS ===");
Console.WriteLine();
Console.WriteLine("Every enemy is UNIQUE with random stats!");
Console.WriteLine();
Console.WriteLine("  SCOUT:   1-3 HP, 1 dmg");
Console.WriteLine("  WARRIOR: 4-6 HP, 1-2 dmg");
Console.WriteLine("  ARCHER:  2-4 HP, 1-3 dmg (!!)");
Console.WriteLine();
Console.WriteLine("Same enemy type = DIFFERENT stats each time!");
Console.WriteLine("+ Stamina + Crits/Misses = MAXIMUM CHAOS!");
Console.WriteLine();

var game = new RPGGame();
game.StartCombatWithRandomVariableEnemy();
game.SetPlayerStats(strength: 2, defense: 1);

Console.WriteLine($"A {game.EnemyName} appears!");
Console.WriteLine($"Stats: {game.EnemyHP} HP, {game.EnemyDamage} damage");
Console.WriteLine("(These stats are RANDOM - reload for different fight!)");
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

    game.ExecuteVariableStatsCombatRoundWithRandomHits(action, CombatAction.Attack);

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
