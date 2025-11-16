using ProjectEvolution.Game;

Console.WriteLine("╔═══════════════════════════════════════╗");
Console.WriteLine("║  PROJECT EVOLUTION - GENERATION 12    ║");
Console.WriteLine("║        PERMADEATH MODE                ║");
Console.WriteLine("╚═══════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("THE ULTIMATE CHALLENGE:");
Console.WriteLine("  ⚔️  Random enemies with variable stats");
Console.WriteLine("  🎲  15% miss | 15% crit | 70% normal");
Console.WriteLine("  ⚡  12 stamina (Attack=3, Defend=1)");
Console.WriteLine("  💀  DIE = Lose current gold!");
Console.WriteLine("  💰  WIN = Gold becomes permanent!");
Console.WriteLine();

var game = new RPGGame();
game.SetPlayerStats(strength: 2, defense: 1);
game.StartPermadeathMode();

Console.WriteLine($"🏦 PERMANENT GOLD: {game.PermanentGold}g");
Console.WriteLine($"💀 DEATHS: {game.DeathCount}");
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

    game.ExecutePermadeathRoundWithRandomHits(action, CombatAction.Attack);

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
Console.WriteLine("╔═══════════════════════════════════════╗");
if (game.IsWon)
{
    game.CommitGoldOnVictory();
    Console.WriteLine("║           ⭐ VICTORY! ⭐              ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine($"The {game.EnemyName} falls defeated!");
    Console.WriteLine();
    Console.WriteLine($"💰 Gold Earned This Run: {game.PlayerGold}g");
    Console.WriteLine($"🏦 PERMANENT GOLD: {game.PermanentGold}g");
    Console.WriteLine($"💚 HP Remaining: {game.PlayerHP}/10");
    Console.WriteLine($"⚡ Stamina Left: {game.PlayerStamina}/12");
    Console.WriteLine();
    Console.WriteLine("Your gold is now SAFE! Play again or cash out.");
}
else
{
    game.HandlePermadeath();
    Console.WriteLine("║           💀 DEATH 💀                 ║");
    Console.WriteLine("╚═══════════════════════════════════════╝");
    Console.WriteLine($"The {game.EnemyName} has slain you!");
    Console.WriteLine();
    Console.WriteLine($"💀 Total Deaths: {game.DeathCount}");
    Console.WriteLine($"📉 Current Run Gold: LOST");
    Console.WriteLine($"🏦 Permanent Gold: {game.PermanentGold}g (safe)");
    Console.WriteLine();
    Console.WriteLine("Your permanent gold is safe. Try again?");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
