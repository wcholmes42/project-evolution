using ProjectEvolution.Game;

Console.WriteLine("╔═══════════════════════════════════════╗");
Console.WriteLine("║  PROJECT EVOLUTION - GENERATION 13    ║");
Console.WriteLine("║    EXPERIENCE & LEVELING SYSTEM       ║");
Console.WriteLine("╚═══════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("CHARACTER PROGRESSION!");
Console.WriteLine("  ⭐  Defeat enemies → Earn 10 XP");
Console.WriteLine("  📈  Level up at 100/200/300 XP...");
Console.WriteLine("  🎯  Track your growth!");
Console.WriteLine();
Console.WriteLine("PLUS all previous systems:");
Console.WriteLine("  Variable enemies | Crits/Misses | Stamina");
Console.WriteLine();

var game = new RPGGame();
game.SetPlayerStats(strength: 2, defense: 1);
game.StartCombatWithXP();

Console.WriteLine($"⭐ LEVEL {game.PlayerLevel} | XP: {game.PlayerXP}/{game.XPForNextLevel}");
Console.WriteLine($"💰 Gold: {game.PlayerGold}g");
Console.WriteLine();
Console.WriteLine($"⚔️  A {game.EnemyName} appears!");
Console.WriteLine($"    Stats: {game.EnemyHP} HP, {game.EnemyDamage} damage");
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

    game.ExecuteXPCombatRoundWithRandomHits(action, CombatAction.Attack);

    Console.WriteLine();
    Console.WriteLine(game.CombatLog);

    if (game.CombatEnded)
    {
        game.ProcessXPGain();
        if (game.CombatLog.Contains("LEVEL UP"))
        {
            Console.WriteLine(game.CombatLog); // Show level up message
        }
    }

    Console.WriteLine();

    if (!game.CombatEnded)
    {
        Console.WriteLine("--- Fight continues ---");
        Console.WriteLine();
    }
}

Console.WriteLine();
Console.WriteLine("╔═══════════════════════════════════════╗");
if (game.IsWon)
{
    Console.WriteLine("║           ⭐ VICTORY! ⭐              ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine($"The {game.EnemyName} falls defeated!");
    Console.WriteLine();
    Console.WriteLine($"⭐ LEVEL: {game.PlayerLevel}");
    Console.WriteLine($"📊 XP: {game.PlayerXP}/{game.XPForNextLevel}");
    Console.WriteLine($"💰 Gold: {game.PlayerGold}g");
    Console.WriteLine($"💚 HP: {game.PlayerHP}/10");
    Console.WriteLine($"⚡ Stamina: {game.PlayerStamina}/12");
    Console.WriteLine();
    Console.WriteLine("You grow stronger with each victory!");
}
else
{
    Console.WriteLine("║           💀 DEATH 💀                 ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine($"The {game.EnemyName} has slain you!");
    Console.WriteLine();
    Console.WriteLine($"⭐ Reached Level: {game.PlayerLevel}");
    Console.WriteLine($"💰 Gold Earned: {game.PlayerGold}g");
    Console.WriteLine();
    Console.WriteLine("Experience is the best teacher... even in death.");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
