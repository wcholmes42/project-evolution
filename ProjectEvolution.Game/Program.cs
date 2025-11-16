using ProjectEvolution.Game;

Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║  PROJECT EVOLUTION - GEN 13-17         ║");
Console.WriteLine("║    COMPLETE PROGRESSION SYSTEM         ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("📈 FULL RPG PROGRESSION:");
Console.WriteLine("  ⭐ Earn XP → Level up → Grow stronger");
Console.WriteLine("  💪 Spend stat points (STR/DEF)");
Console.WriteLine("  ❤️  Gain +2 Max HP per level + full heal!");
Console.WriteLine("  👹 Enemies scale with YOUR level");
Console.WriteLine();
Console.WriteLine("⚡ Scout:10xp | Warrior:25xp | Archer:15xp");
Console.WriteLine("🎲 Crits/Misses | Stamina | Variable Stats");
Console.WriteLine();

var game = new RPGGame();
game.SetPlayerStats(strength: 2, defense: 1);
game.StartCombatWithMaxHP();

Console.WriteLine($"YOU: Lvl {game.PlayerLevel} | HP:{game.PlayerHP}/{game.MaxPlayerHP} | STR:{game.PlayerStrength} DEF:{game.PlayerDefense}");
Console.WriteLine($"XP: {game.PlayerXP}/{game.XPForNextLevel} | Gold:{game.PlayerGold}g");
if (game.AvailableStatPoints > 0) Console.WriteLine($"⚡ UNSPENT STAT POINTS: {game.AvailableStatPoints}!");
Console.WriteLine();
Console.WriteLine($"ENEMY: {game.EnemyName} [Lvl {game.EnemyLevel}]");
Console.WriteLine($"Stats: {game.EnemyHP} HP, {game.EnemyDamage} damage");
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

    game.ExecuteMaxHPCombatRoundWithRandomHits(action, CombatAction.Attack);

    Console.WriteLine();
    Console.WriteLine(game.CombatLog);

    if (game.CombatEnded)
    {
        string logBefore = game.CombatLog;
        game.ProcessMaxHPGrowth();
        if (game.CombatLog != logBefore)
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
Console.WriteLine("╔════════════════════════════════════════╗");
if (game.IsWon)
{
    Console.WriteLine("║             ⭐ VICTORY! ⭐              ║");
    Console.WriteLine("╚════════════════════════════════════════╝");
    Console.WriteLine($"The {game.EnemyName} [Lvl {game.EnemyLevel}] falls!");
    Console.WriteLine();
    Console.WriteLine($"⭐ Level {game.PlayerLevel} | XP: {game.PlayerXP}/{game.XPForNextLevel}");
    Console.WriteLine($"❤️  HP: {game.PlayerHP}/{game.MaxPlayerHP} | ⚡ Stamina: {game.PlayerStamina}/12");
    Console.WriteLine($"💪 STR: {game.PlayerStrength} | DEF: {game.PlayerDefense}");
    if (game.AvailableStatPoints > 0)
    {
        Console.WriteLine($"📊 UNSPENT POINTS: {game.AvailableStatPoints} - Allocate them!");
    }
    Console.WriteLine($"💰 Gold: {game.PlayerGold}g");
    Console.WriteLine();
    Console.WriteLine("Your journey of growth continues!");
}
else
{
    Console.WriteLine("║            💀 DEFEATED 💀              ║");
    Console.WriteLine("╚════════════════════════════════════════╝");
    Console.WriteLine($"{game.EnemyName} [Lvl {game.EnemyLevel}] has slain you!");
    Console.WriteLine();
    Console.WriteLine($"⭐ Final Level: {game.PlayerLevel}");
    Console.WriteLine($"💰 Gold: {game.PlayerGold}g");
    Console.WriteLine();
    Console.WriteLine("The path to power is paved with defeats.");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
