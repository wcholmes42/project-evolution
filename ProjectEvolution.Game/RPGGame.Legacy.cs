namespace ProjectEvolution.Game;

using System;
using System.Collections.Generic;
using System.Linq;

public partial class RPGGame
{
    // ════════════════════════════════════════════════════════════════════
    // LEGACY GENERATIONS (0-12)
    // Kept for backward compatibility and regression testing
    // ════════════════════════════════════════════════════════════════════

    public void StartWithCoinFlip(bool heads)
    {
        IsWon = heads;
    }

    public void StartWithRandomCoinFlip()
    {
        bool heads = _random.Next(2) == 0;
        StartWithCoinFlip(heads);
    }

    public void StartCombat()
    {
        _combatStarted = true;
        _aiCombat = false;
        IsWon = false;
    }

    public void ChooseAction(CombatAction action)
    {
        if (!_combatStarted)
        {
            throw new InvalidOperationException("Combat has not been started");
        }

        // Simple combat (Gen 2): Attack wins, Defend loses
        IsWon = action == CombatAction.Attack;
    }

    public void StartCombatWithAI()
    {
        _combatStarted = true;
        _aiCombat = true;
        IsWon = false;
        CombatLog = string.Empty;
    }

    public void ChooseActionAgainstEnemy(CombatAction playerAction, CombatAction enemyAction, bool? coinFlipPlayerWins = null)
    {
        if (!_combatStarted || !_aiCombat)
        {
            throw new InvalidOperationException("AI combat has not been started");
        }

        IsWon = ResolveCombat(playerAction, enemyAction, coinFlipPlayerWins);
    }

    public void ChooseActionWithRandomEnemy(CombatAction playerAction)
    {
        if (!_combatStarted || !_aiCombat)
        {
            throw new InvalidOperationException("AI combat has not been started");
        }

        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        IsWon = ResolveCombat(playerAction, enemyAction, null);
    }

    private bool ResolveCombat(CombatAction playerAction, CombatAction enemyAction, bool? coinFlipPlayerWins)
    {
        // Rock-paper-scissors style combat
        if (playerAction == CombatAction.Attack && enemyAction == CombatAction.Defend)
        {
            CombatLog = "You attack! The goblin's defense crumbles!";
            return true; // Player wins
        }
        else if (playerAction == CombatAction.Defend && enemyAction == CombatAction.Attack)
        {
            CombatLog = "The goblin attacks! Your defense holds but you don't defeat it!";
            return false; // Enemy wins
        }
        else if (playerAction == CombatAction.Defend && enemyAction == CombatAction.Defend)
        {
            CombatLog = "Both fighters defend cautiously... stalemate!";
            return false; // Draw - player didn't defeat enemy
        }
        else // Both attack
        {
            bool playerWins = coinFlipPlayerWins ?? _random.Next(2) == 0;
            CombatLog = playerWins
                ? "Both strike at once! Your blade lands first!"
                : "Both strike at once! The goblin's attack overwhelms you!";
            return playerWins;
        }
    }

    public void StartCombatWithHP()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        EnemyHP = 3;
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void ExecuteHPCombatRoundWithRandomEnemy(CombatAction playerAction)
    {
        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        ExecuteHPCombatRound(playerAction, enemyAction);
    }

    public void ExecuteHPCombatRound(CombatAction playerAction, CombatAction enemyAction)
    {
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return; // Combat already over
        }

        CombatLog = string.Empty; // Clear previous log

        // Resolve damage
        bool playerAttacks = playerAction == CombatAction.Attack;
        bool playerDefends = playerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage if attacking and enemy not defending
        if (playerAttacks && !enemyDefends)
        {
            EnemyHP = Math.Max(0, EnemyHP - 1);
            CombatLog += "You strike the goblin! ";
        }

        // Enemy deals damage if attacking and player not defending
        if (enemyAttacks && !playerDefends)
        {
            PlayerHP = Math.Max(0, PlayerHP - 1);
            CombatLog += "The goblin strikes you! ";
        }

        // Check for combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            CombatLog += "The goblin falls defeated!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "You have been defeated!";
        }
        else
        {
            // Combat continues
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Both fighters guard defensively.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the goblin's attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks your attack!";
            }
        }
    }

    public void StartCombatWithLoot()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        EnemyHP = 3;
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Gold persists across combats - don't reset it
    }

    public void ExecuteLootCombatRoundWithRandomEnemy(CombatAction playerAction)
    {
        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        ExecuteLootCombatRound(playerAction, enemyAction);
    }

    public void ExecuteLootCombatRound(CombatAction playerAction, CombatAction enemyAction)
    {
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return; // Combat already over
        }

        CombatLog = string.Empty; // Clear previous log

        // Resolve damage
        bool playerAttacks = playerAction == CombatAction.Attack;
        bool playerDefends = playerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage if attacking and enemy not defending
        if (playerAttacks && !enemyDefends)
        {
            EnemyHP = Math.Max(0, EnemyHP - 1);
            CombatLog += "You strike the goblin! ";
        }

        // Enemy deals damage if attacking and player not defending
        if (enemyAttacks && !playerDefends)
        {
            PlayerHP = Math.Max(0, PlayerHP - 1);
            CombatLog += "The goblin strikes you! ";
        }

        // Check for combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10; // Award gold for victory
            CombatLog += "The goblin falls defeated! You gain 10 gold!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "You have been defeated! No gold awarded.";
        }
        else
        {
            // Combat continues
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Both fighters guard defensively.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the goblin's attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks your attack!";
            }
        }
    }

    public void StartMultiEnemyCombat(int enemyCount)
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        RemainingEnemies = enemyCount;
        EnemyHP = 3; // Start first enemy
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Gold persists
    }

    public void ExecuteMultiEnemyRoundWithRandomEnemy(CombatAction playerAction)
    {
        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        ExecuteMultiEnemyRound(playerAction, enemyAction);
    }

    public void ExecuteMultiEnemyRound(CombatAction playerAction, CombatAction enemyAction)
    {
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return; // Combat already over
        }

        CombatLog = string.Empty;

        // Resolve damage
        bool playerAttacks = playerAction == CombatAction.Attack;
        bool playerDefends = playerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage if attacking and enemy not defending
        if (playerAttacks && !enemyDefends)
        {
            EnemyHP = Math.Max(0, EnemyHP - 1);
            CombatLog += "You strike the goblin! ";
        }

        // Enemy deals damage if attacking and player not defending
        if (enemyAttacks && !playerDefends)
        {
            PlayerHP = Math.Max(0, PlayerHP - 1);
            CombatLog += "The goblin strikes you! ";
        }

        // Check if current enemy is defeated
        if (EnemyHP <= 0)
        {
            PlayerGold += 10; // Award gold for this enemy
            RemainingEnemies--;
            CombatLog += $"Goblin defeated! +10 gold! ";

            if (RemainingEnemies > 0)
            {
                // Spawn next enemy
                EnemyHP = 3;
                CombatLog += $"Another goblin appears! ({RemainingEnemies} remaining)";
            }
            else
            {
                // All enemies defeated
                IsWon = true;
                CombatEnded = true;
                CombatLog += "All enemies defeated! Victory is yours!";
            }
        }
        else if (PlayerHP <= 0)
        {
            // Player dies
            IsWon = false;
            CombatEnded = true;
            CombatLog += "You have been defeated!";
        }
        else
        {
            // Combat continues with current enemy
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Both fighters guard defensively.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the goblin's attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks your attack!";
            }
        }
    }

    public void StartCombatWithStats()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        EnemyHP = 3;
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Stats and gold persist
    }

    public void ExecuteStatsCombatRoundWithRandomEnemy(CombatAction playerAction)
    {
        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        ExecuteStatsCombatRound(playerAction, enemyAction);
    }

    public void ExecuteStatsCombatRound(CombatAction playerAction, CombatAction enemyAction)
    {
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return;
        }

        CombatLog = string.Empty;

        bool playerAttacks = playerAction == CombatAction.Attack;
        bool playerDefends = playerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage based on Strength
        if (playerAttacks && !enemyDefends)
        {
            int damage = GetEffectiveStrength();
            EnemyHP = Math.Max(0, EnemyHP - damage);
            CombatLog += $"You strike for {damage} damage! ";
        }

        // Enemy deals damage, reduced by player Defense
        if (enemyAttacks && !playerDefends)
        {
            int enemyDamage = 1; // Enemy base damage
            int actualDamage = Math.Max(1, enemyDamage - GetEffectiveDefense()); // Minimum 1 damage
            PlayerHP = Math.Max(0, PlayerHP - actualDamage);
            CombatLog += $"The goblin strikes for {actualDamage} damage! ";
        }

        // Check combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10;
            CombatLog += "Victory! +10 gold!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "You have been defeated!";
        }
        else
        {
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Both fighters guard defensively.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the goblin's attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks your attack!";
            }
        }
    }
}
