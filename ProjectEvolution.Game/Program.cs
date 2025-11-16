using ProjectEvolution.Game;

Console.WriteLine("╔════════════════════════════════════════╗");
Console.WriteLine("║  PROJECT EVOLUTION - GENERATION 18     ║");
Console.WriteLine("║      ENDLESS GAME LOOP MODE            ║");
Console.WriteLine("╚════════════════════════════════════════╝");
Console.WriteLine();
Console.WriteLine("🔄 FIGHT UNTIL YOU DIE:");
Console.WriteLine("  ⭐ Chain victories → Level up → Get stronger");
Console.WriteLine("  💰 Accumulate gold → Build wealth");
Console.WriteLine("  💀 One death = Game Over");
Console.WriteLine("  🚪 Quit anytime after victory");
Console.WriteLine();
Console.WriteLine("All systems active: XP | Levels | Stat Points");
Console.WriteLine("Enemy Scaling | Max HP Growth | Variable Stats");
Console.WriteLine();

var game = new RPGGame();
game.SetPlayerStats(strength: 2, defense: 1);
game.StartGameLoop();

bool playing = true;

while (playing && !game.RunEnded)
{
    Console.WriteLine($"╔════════════════════════════════════════╗");
    Console.WriteLine($"║  COMBAT #{game.CombatsWon + 1}");
    Console.WriteLine($"╚════════════════════════════════════════╝");
    Console.WriteLine($"YOU: Lvl {game.PlayerLevel} | HP:{game.PlayerHP}/{game.MaxPlayerHP} | STR:{game.PlayerStrength} DEF:{game.PlayerDefense}");
    Console.WriteLine($"XP: {game.PlayerXP}/{game.XPForNextLevel} | Gold:{game.PlayerGold}g | Wins:{game.CombatsWon}");
    if (game.AvailableStatPoints > 0) Console.WriteLine($"⚡ UNSPENT POINTS: {game.AvailableStatPoints}!");
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

        game.ExecuteGameLoopRoundWithRandomHits(action, CombatAction.Attack);

        Console.WriteLine();
        Console.WriteLine(game.CombatLog);

        if (game.CombatEnded)
        {
            string logBefore = game.CombatLog;
            game.ProcessGameLoopVictory();
            if (game.CombatLog != logBefore)
            {
                Console.WriteLine(game.CombatLog); // Show level up
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
        Console.WriteLine($"❤️  HP: {game.MaxPlayerHP}/{game.MaxPlayerHP} (restored!) | Gold: {game.PlayerGold}g");
        Console.WriteLine($"💪 STR: {game.PlayerStrength} | DEF: {game.PlayerDefense}");
        if (game.AvailableStatPoints > 0)
        {
            Console.WriteLine($"📊 UNSPENT POINTS: {game.AvailableStatPoints}!");
        }
        Console.WriteLine($"🏆 Total Wins: {game.CombatsWon}");
        Console.WriteLine();
        Console.WriteLine("Continue fighting? (Y/N)");
        Console.Write("> ");

        var continueChoice = Console.ReadLine()?.ToUpper();
        if (continueChoice == "Y" || continueChoice == "YES" || continueChoice == "")
        {
            game.ContinueToNextCombat();
        }
        else
        {
            playing = false;
            Console.WriteLine();
            Console.WriteLine("You wisely retreat to fight another day!");
        }
    }
    else
    {
        Console.WriteLine("║            💀 GAME OVER 💀             ║");
        Console.WriteLine("╚════════════════════════════════════════╝");
        Console.WriteLine($"{game.EnemyName} [Lvl {game.EnemyLevel}] has slain you!");
        Console.WriteLine();
        Console.WriteLine($"═══════ FINAL STATS ═══════");
        Console.WriteLine($"⭐ Final Level: {game.PlayerLevel}");
        Console.WriteLine($"🏆 Combats Won: {game.CombatsWon}");
        Console.WriteLine($"💰 Gold Earned: {game.PlayerGold}g");
        Console.WriteLine($"📊 XP Gained: {game.PlayerXP}");
        Console.WriteLine();
        Console.WriteLine("You fought valiantly. The journey ends here.");
        playing = false;
    }
}

if (!game.RunEnded && !playing)
{
    Console.WriteLine();
    Console.WriteLine("═══════════════════════════════════════");
    Console.WriteLine("You survived to tell the tale!");
    Console.WriteLine($"🏆 Victories: {game.CombatsWon}");
    Console.WriteLine($"⭐ Level: {game.PlayerLevel}");
    Console.WriteLine($"💰 Gold: {game.PlayerGold}g");
    Console.WriteLine("═══════════════════════════════════════");
}

Console.WriteLine();
Console.WriteLine("Press any key to exit...");
Console.ReadKey();
