namespace ProjectEvolution.Game;

public class RPGGame
{
    private static readonly Random _random = new Random();
    private bool _combatStarted;
    private bool _aiCombat;
    private bool _hpCombat;

    public bool IsWon { get; private set; }
    public string CombatLog { get; private set; } = string.Empty;
    public int PlayerHP { get; private set; }
    public int EnemyHP { get; private set; }
    public bool CombatEnded { get; private set; }
    public int PlayerGold { get; private set; }
    public int RemainingEnemies { get; private set; }
    public int PlayerStrength { get; private set; } = 1;
    public int PlayerDefense { get; private set; } = 0;
    public int PlayerStamina { get; private set; } = 12;
    public int EnemyDamage { get; private set; } = 1;
    public string EnemyName { get; private set; } = "Goblin";
    public int PermanentGold { get; private set; } = 0;
    public int DeathCount { get; private set; } = 0;
    public int PlayerXP { get; private set; } = 0;
    public int PlayerLevel { get; private set; } = 1;
    public int XPForNextLevel { get; private set; } = 100;
    public int AvailableStatPoints { get; private set; } = 0;
    public int EnemyLevel { get; private set; } = 1;
    public int MaxPlayerHP { get; private set; } = 10;

    public void Start()
    {
        IsWon = true;
    }

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

    public void SetPlayerStats(int strength, int defense)
    {
        PlayerStrength = strength;
        PlayerDefense = defense;
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
            int damage = PlayerStrength;
            EnemyHP = Math.Max(0, EnemyHP - damage);
            CombatLog += $"You strike for {damage} damage! ";
        }

        // Enemy deals damage, reduced by player Defense
        if (enemyAttacks && !playerDefends)
        {
            int enemyDamage = 1; // Enemy base damage
            int actualDamage = Math.Max(1, enemyDamage - PlayerDefense); // Minimum 1 damage
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
                CombatLog += "Both fighters guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks!";
            }
        }
    }

    public void StartMultiEnemyCombatWithStats(int enemyCount)
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        RemainingEnemies = enemyCount;
        EnemyHP = 3;
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Stats and gold persist
    }

    public void ExecuteStatsMultiEnemyRoundWithRandomEnemy(CombatAction playerAction)
    {
        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        ExecuteStatsMultiEnemyRound(playerAction, enemyAction);
    }

    public void ExecuteStatsMultiEnemyRound(CombatAction playerAction, CombatAction enemyAction)
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
            int damage = PlayerStrength;
            EnemyHP = Math.Max(0, EnemyHP - damage);
            CombatLog += $"You strike for {damage} damage! ";
        }

        // Enemy deals damage, reduced by player Defense
        if (enemyAttacks && !playerDefends)
        {
            int enemyDamage = 1;
            int actualDamage = Math.Max(1, enemyDamage - PlayerDefense);
            PlayerHP = Math.Max(0, PlayerHP - actualDamage);
            CombatLog += $"The goblin strikes for {actualDamage} damage! ";
        }

        // Check if current enemy defeated
        if (EnemyHP <= 0)
        {
            PlayerGold += 10;
            RemainingEnemies--;
            CombatLog += $"Goblin defeated! +10 gold! ";

            if (RemainingEnemies > 0)
            {
                EnemyHP = 3;
                CombatLog += $"Next goblin appears! ({RemainingEnemies} remaining)";
            }
            else
            {
                IsWon = true;
                CombatEnded = true;
                CombatLog += "All enemies defeated! Victory!";
            }
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
                CombatLog += "Both guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "Goblin blocks!";
            }
        }
    }

    public void StartCombatWithCrits()
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

    public void ExecuteCritCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteCritCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteCritCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
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

        // Player deals damage with hit/miss/crit mechanics
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "You swing and MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRITICAL HIT! {damage} damage! ";
                }
                else
                {
                    CombatLog += $"You strike for {damage} damage! ";
                }
            }
        }

        // Enemy deals damage with hit/miss/crit mechanics
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += "The goblin misses! ";
            }
            else
            {
                int baseDamage = 1; // Enemy base damage
                int damage = enemyHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"Goblin CRITICAL! {actualDamage} damage! ";
                }
                else
                {
                    CombatLog += $"Goblin hits for {actualDamage} damage! ";
                }
            }
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
                CombatLog += "Both guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks!";
            }
        }
    }

    private HitType DetermineHitType()
    {
        int roll = _random.Next(100);
        if (roll < 15) return HitType.Miss;      // 0-14: 15% miss
        if (roll < 30) return HitType.Critical;  // 15-29: 15% crit
        return HitType.Normal;                    // 30-99: 70% normal
    }

    public void StartCombatWithStamina()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        EnemyHP = 3;
        PlayerStamina = 12;
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Stats and gold persist
    }

    public void ExecuteStaminaCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteStaminaCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteStaminaCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
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

        // Check stamina and potentially force action
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "Not enough stamina to attack! Forced to defend. ";
        }

        // Deduct stamina based on action
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage with hit/miss/crit mechanics
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "You swing and MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRITICAL HIT! {damage} damage! ";
                }
                else
                {
                    CombatLog += $"You strike for {damage} damage! ";
                }
            }
        }

        // Enemy deals damage with hit/miss/crit mechanics
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += "The goblin misses! ";
            }
            else
            {
                int baseDamage = 1;
                int damage = enemyHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"Goblin CRITICAL! {actualDamage} damage! ";
                }
                else
                {
                    CombatLog += $"Goblin hits for {actualDamage} damage! ";
                }
            }
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
                CombatLog += "Both guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block the attack!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += "The goblin blocks!";
            }
        }
    }

    private void InitializeEnemy(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.GoblinScout:
                EnemyHP = 2;
                EnemyDamage = 1;
                EnemyName = "Goblin Scout";
                break;
            case EnemyType.GoblinWarrior:
                EnemyHP = 5;
                EnemyDamage = 1;
                EnemyName = "Goblin Warrior";
                break;
            case EnemyType.GoblinArcher:
                EnemyHP = 3;
                EnemyDamage = 2;
                EnemyName = "Goblin Archer";
                break;
        }
    }

    public void StartCombatWithEnemyType(EnemyType enemyType)
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemy(enemyType);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Stats and gold persist
    }

    public void StartCombatWithRandomEnemyType()
    {
        EnemyType randomType = (EnemyType)_random.Next(3); // 0=Scout, 1=Warrior, 2=Archer
        StartCombatWithEnemyType(randomType);
    }

    public void ExecuteEnemyTypeCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteEnemyTypeCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteEnemyTypeCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
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

        // Check stamina and potentially force action
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "Not enough stamina! Forced to defend. ";
        }

        // Deduct stamina
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player deals damage
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRIT! {damage} damage! ";
                }
                else
                {
                    CombatLog += $"Hit for {damage}! ";
                }
            }
        }

        // Enemy deals damage (using EnemyDamage property)
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += $"{EnemyName} misses! ";
            }
            else
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"{EnemyName} CRIT! {actualDamage} damage! ";
                }
                else
                {
                    CombatLog += $"{EnemyName} hits for {actualDamage}! ";
                }
            }
        }

        // Check combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10;
            CombatLog += $"{EnemyName} defeated! +10 gold!";
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
                CombatLog += "Both guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "You block!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += $"{EnemyName} blocks!";
            }
        }
    }

    private void InitializeEnemyWithVariableStats(EnemyType enemyType)
    {
        switch (enemyType)
        {
            case EnemyType.GoblinScout:
                EnemyHP = _random.Next(1, 4); // 1-3 HP
                EnemyDamage = 1;
                EnemyName = "Goblin Scout";
                break;
            case EnemyType.GoblinWarrior:
                EnemyHP = _random.Next(4, 7); // 4-6 HP
                EnemyDamage = _random.Next(1, 3); // 1-2 damage
                EnemyName = "Goblin Warrior";
                break;
            case EnemyType.GoblinArcher:
                EnemyHP = _random.Next(2, 5); // 2-4 HP
                EnemyDamage = _random.Next(1, 4); // 1-3 damage
                EnemyName = "Goblin Archer";
                break;
        }
    }

    public void StartCombatWithVariableStats(EnemyType enemyType)
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats(enemyType);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // Stats and gold persist
    }

    public void StartCombatWithRandomVariableEnemy()
    {
        EnemyType randomType = (EnemyType)_random.Next(3);
        StartCombatWithVariableStats(randomType);
    }

    public void ExecuteVariableStatsCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteVariableStatsCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteVariableStatsCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        // Same as ExecuteEnemyTypeCombatRound - variable stats just affect initialization
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return;
        }

        CombatLog = string.Empty;

        // Check stamina
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "Not enough stamina! ";
        }

        // Deduct stamina
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player damage
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRIT! {damage}! ";
                }
                else
                {
                    CombatLog += $"Hit {damage}! ";
                }
            }
        }

        // Enemy damage
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += $"{EnemyName} misses! ";
            }
            else
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"{EnemyName} CRIT {actualDamage}! ";
                }
                else
                {
                    CombatLog += $"{EnemyName} hits {actualDamage}! ";
                }
            }
        }

        // Combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10;
            CombatLog += "Victory! +10g!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "Defeated!";
        }
        else
        {
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Both guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "Block!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += $"{EnemyName} blocks!";
            }
        }
    }

    public void StartPermadeathMode()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        PlayerGold = 0; // Reset current gold for new run
        // PermanentGold, DeathCount, and Stats persist
    }

    public void StartNewPermadeathCombat()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // PlayerGold persists between victories until death
        // PermanentGold, DeathCount, and Stats persist
    }

    public void ExecutePermadeathRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecutePermadeathRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecutePermadeathRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        // Same as ExecuteVariableStatsCombatRound but without gold handling in combat end
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return;
        }

        CombatLog = string.Empty;

        // Check stamina
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        // Deduct stamina
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player damage
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRIT {damage}! ";
                }
                else
                {
                    CombatLog += $"Hit {damage}! ";
                }
            }
        }

        // Enemy damage
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += $"{EnemyName} misses! ";
            }
            else
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"{EnemyName} CRIT {actualDamage}! ";
                }
                else
                {
                    CombatLog += $"{EnemyName} {actualDamage}! ";
                }
            }
        }

        // Check combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10; // Add to current gold (not permanent yet)
            CombatLog += "Victory! +10g!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
        else
        {
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "Block!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += $"{EnemyName} blocks!";
            }
        }
    }

    public void CommitGoldOnVictory()
    {
        if (IsWon && CombatEnded)
        {
            PermanentGold = PlayerGold; // Make current gold permanent
        }
    }

    public void HandlePermadeath()
    {
        if (!IsWon && CombatEnded)
        {
            DeathCount++;
            PlayerGold = PermanentGold; // Reset to permanent gold (lose current run gold)
        }
    }

    public void StartCombatWithXP()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // XP, Level, Stats, Gold, PermanentGold, DeathCount all persist
    }

    public void StartNewXPCombat()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // XP, Level, Stats, Gold all persist
    }

    public void ExecuteXPCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteXPCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteXPCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
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

        // Check stamina
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        // Deduct stamina
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Player damage
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);

                if (playerHitType == HitType.Critical)
                {
                    CombatLog += $"CRIT {damage}! ";
                }
                else
                {
                    CombatLog += $"Hit {damage}! ";
                }
            }
        }

        // Enemy damage
        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += $"{EnemyName} misses! ";
            }
            else
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);

                if (enemyHitType == HitType.Critical)
                {
                    CombatLog += $"{EnemyName} CRIT {actualDamage}! ";
                }
                else
                {
                    CombatLog += $"{EnemyName} {actualDamage}! ";
                }
            }
        }

        // Check combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10;
            PlayerXP += 10; // Grant XP on victory
            CombatLog += "Victory! +10g +10xp!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
        else
        {
            if (playerDefends && enemyDefends)
            {
                CombatLog += "Guard.";
            }
            else if (playerDefends && enemyAttacks)
            {
                CombatLog += "Block!";
            }
            else if (enemyDefends && playerAttacks)
            {
                CombatLog += $"{EnemyName} blocks!";
            }
        }
    }

    public void ProcessXPGain()
    {
        // Check for level up
        while (PlayerXP >= XPForNextLevel)
        {
            PlayerLevel++;
            XPForNextLevel = PlayerLevel * 100; // Level 2 = 200 XP, Level 3 = 300 XP, etc.
            CombatLog += $" LEVEL UP! Now level {PlayerLevel}!";
        }
    }

    public void SpendStatPoint(StatType statType)
    {
        if (AvailableStatPoints <= 0)
        {
            throw new InvalidOperationException("No stat points available");
        }

        AvailableStatPoints--;

        switch (statType)
        {
            case StatType.Strength:
                PlayerStrength++;
                break;
            case StatType.Defense:
                PlayerDefense++;
                break;
        }
    }

    public void StartCombatWithStatPoints()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // XP, Level, AvailableStatPoints, Stats, Gold all persist
    }

    public void StartNewStatPointsCombat()
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats((EnemyType)_random.Next(3));
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
        // XP, Level, AvailableStatPoints, Stats, Gold persist
    }

    public void ExecuteStatPointsCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteStatPointsCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteStatPointsCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        // Same as XP combat
        if (!_combatStarted || !_hpCombat)
        {
            throw new InvalidOperationException("HP combat has not been started");
        }

        if (CombatEnded)
        {
            return;
        }

        CombatLog = string.Empty;

        // Stamina check
        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        // Deduct stamina
        if (actualPlayerAction == CombatAction.Attack)
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 3);
        }
        else
        {
            PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Combat resolution (same as XP)
        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType == HitType.Miss)
            {
                CombatLog += "MISS! ";
            }
            else
            {
                int baseDamage = PlayerStrength;
                int damage = playerHitType == HitType.Critical ? baseDamage * 2 : baseDamage;
                EnemyHP = Math.Max(0, EnemyHP - damage);
                CombatLog += playerHitType == HitType.Critical ? $"CRIT {damage}! " : $"Hit {damage}! ";
            }
        }

        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType == HitType.Miss)
            {
                CombatLog += $"{EnemyName} misses! ";
            }
            else
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);
                CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
            }
        }

        // Combat end
        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            PlayerGold += 10;
            PlayerXP += 10;
            CombatLog += "Victory! +10g +10xp!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
        else
        {
            if (playerDefends && enemyDefends) CombatLog += "Guard.";
            else if (playerDefends && enemyAttacks) CombatLog += "Block!";
            else if (enemyDefends && playerAttacks) CombatLog += $"{EnemyName} blocks!";
        }
    }

    public void ProcessStatPointGains()
    {
        // Process XP and grant stat points on level up
        int levelBefore = PlayerLevel;
        while (PlayerXP >= XPForNextLevel)
        {
            PlayerLevel++;
            XPForNextLevel = PlayerLevel * 100;
            AvailableStatPoints += 2; // Grant 2 stat points per level
            CombatLog += $" LEVEL UP {PlayerLevel}! +2 stat points!";
        }
    }

    private int GetEnemyXPValue(EnemyType enemyType)
    {
        return enemyType switch
        {
            EnemyType.GoblinScout => 10,
            EnemyType.GoblinWarrior => 25,
            EnemyType.GoblinArcher => 15,
            _ => 10
        };
    }

    public void StartCombatWithEnemyXP(EnemyType enemyType)
    {
        _combatStarted = true;
        _hpCombat = true;
        _aiCombat = false;
        PlayerHP = 10;
        PlayerStamina = 12;
        InitializeEnemyWithVariableStats(enemyType);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void ExecuteEnemyXPCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        if (!_combatStarted || !_hpCombat) throw new InvalidOperationException("Combat not started");
        if (CombatEnded) return;

        CombatLog = string.Empty;

        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        if (actualPlayerAction == CombatAction.Attack) PlayerStamina = Math.Max(0, PlayerStamina - 3);
        else PlayerStamina = Math.Max(0, PlayerStamina - 1);

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        if (playerAttacks && !enemyDefends)
        {
            if (playerHitType != HitType.Miss)
            {
                int damage = playerHitType == HitType.Critical ? PlayerStrength * 2 : PlayerStrength;
                EnemyHP = Math.Max(0, EnemyHP - damage);
                CombatLog += playerHitType == HitType.Critical ? $"CRIT {damage}! " : $"Hit {damage}! ";
            }
            else CombatLog += "MISS! ";
        }

        if (enemyAttacks && !playerDefends)
        {
            if (enemyHitType != HitType.Miss)
            {
                int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
                int actualDamage = Math.Max(1, damage - PlayerDefense);
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);
                CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
            }
            else CombatLog += $"{EnemyName} misses! ";
        }

        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            int xpGained = GetEnemyXPValue((EnemyType)Enum.Parse(typeof(EnemyType), EnemyName.Replace(" ", "")));
            PlayerXP += xpGained;
            PlayerGold += 10;
            CombatLog += $"Victory! +10g +{xpGained}xp!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
    }

    private void InitializeEnemyWithLevel(EnemyType enemyType, int level)
    {
        InitializeEnemyWithVariableStats(enemyType);
        EnemyLevel = level;
        // Scale stats with level: +2 HP and +1 damage per level above 1
        int bonusHP = (level - 1) * 2;
        int bonusDamage = (level - 1);
        EnemyHP += bonusHP;
        EnemyDamage += bonusDamage;
    }

    public void StartCombatAtEnemyLevel(EnemyType enemyType, int enemyLevel)
    {
        _combatStarted = true;
        _hpCombat = true;
        PlayerHP = MaxPlayerHP;
        PlayerStamina = 12;
        InitializeEnemyWithLevel(enemyType, enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void StartCombatWithLevelScaling()
    {
        _combatStarted = true;
        _hpCombat = true;
        PlayerHP = MaxPlayerHP;
        PlayerStamina = 12;
        // Enemy level near player level (player level +/- 1)
        int enemyLevel = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void StartNewLevelScalingCombat()
    {
        StartCombatWithLevelScaling();
    }

    public void ExecuteLevelScalingRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        // Same as enemy XP combat
        if (!_combatStarted || !_hpCombat) throw new InvalidOperationException("Combat not started");
        if (CombatEnded) return;

        CombatLog = string.Empty;

        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        if (actualPlayerAction == CombatAction.Attack) PlayerStamina = Math.Max(0, PlayerStamina - 3);
        else PlayerStamina = Math.Max(0, PlayerStamina - 1);

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        if (playerAttacks && !enemyDefends && playerHitType != HitType.Miss)
        {
            int damage = playerHitType == HitType.Critical ? PlayerStrength * 2 : PlayerStrength;
            EnemyHP = Math.Max(0, EnemyHP - damage);
            CombatLog += playerHitType == HitType.Critical ? $"CRIT {damage}! " : $"Hit {damage}! ";
        }
        else if (playerAttacks) CombatLog += "MISS! ";

        if (enemyAttacks && !playerDefends && enemyHitType != HitType.Miss)
        {
            int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
            int actualDamage = Math.Max(1, damage - PlayerDefense);
            PlayerHP = Math.Max(0, PlayerHP - actualDamage);
            CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
        }
        else if (enemyAttacks) CombatLog += $"{EnemyName} misses! ";

        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            int xpGained = GetEnemyXPValue((EnemyType)Enum.Parse(typeof(EnemyType), EnemyName.Replace(" ", "")));
            PlayerXP += xpGained;
            PlayerGold += 10;
            CombatLog += $"Victory! +10g +{xpGained}xp!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
    }

    public void ProcessLevelUp()
    {
        while (PlayerXP >= XPForNextLevel)
        {
            PlayerLevel++;
            XPForNextLevel = PlayerLevel * 100;
            AvailableStatPoints += 2;
            CombatLog += $" LEVEL UP {PlayerLevel}! +2 pts!";
        }
    }

    public void StartCombatWithMaxHP()
    {
        _combatStarted = true;
        _hpCombat = true;
        PlayerHP = MaxPlayerHP;
        PlayerStamina = 12;
        int enemyLevel = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void StartNewMaxHPCombat()
    {
        StartCombatWithMaxHP();
    }

    public void ExecuteMaxHPCombatRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteMaxHPCombatRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteMaxHPCombatRound(CombatAction playerAction, CombatAction enemyAction,
        HitType playerHitType, HitType enemyHitType)
    {
        if (!_combatStarted || !_hpCombat) throw new InvalidOperationException("Combat not started");
        if (CombatEnded) return;

        CombatLog = string.Empty;

        CombatAction actualPlayerAction = playerAction;
        if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
        {
            actualPlayerAction = CombatAction.Defend;
            CombatLog += "No stamina! ";
        }

        if (actualPlayerAction == CombatAction.Attack) PlayerStamina = Math.Max(0, PlayerStamina - 3);
        else PlayerStamina = Math.Max(0, PlayerStamina - 1);

        bool playerAttacks = actualPlayerAction == CombatAction.Attack;
        bool playerDefends = actualPlayerAction == CombatAction.Defend;
        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        if (playerAttacks && !enemyDefends && playerHitType != HitType.Miss)
        {
            int damage = playerHitType == HitType.Critical ? PlayerStrength * 2 : PlayerStrength;
            EnemyHP = Math.Max(0, EnemyHP - damage);
            CombatLog += playerHitType == HitType.Critical ? $"CRIT {damage}! " : $"Hit {damage}! ";
        }
        else if (playerAttacks) CombatLog += "MISS! ";

        if (enemyAttacks && !playerDefends && enemyHitType != HitType.Miss)
        {
            int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
            int actualDamage = Math.Max(1, damage - PlayerDefense);
            PlayerHP = Math.Max(0, PlayerHP - actualDamage);
            CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
        }
        else if (enemyAttacks) CombatLog += $"{EnemyName} misses! ";

        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            int xpGained = GetEnemyXPValue((EnemyType)Enum.Parse(typeof(EnemyType), EnemyName.Replace(" ", "")));
            PlayerXP += xpGained;
            PlayerGold += 10;
            CombatLog += $"Victory! +10g +{xpGained}xp!";
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            CombatLog += "DEATH!";
        }
    }

    public void ProcessMaxHPGrowth()
    {
        int levelBefore = PlayerLevel;
        while (PlayerXP >= XPForNextLevel)
        {
            PlayerLevel++;
            XPForNextLevel = PlayerLevel * 100;
            AvailableStatPoints += 2;
            MaxPlayerHP += 2; // Gain 2 max HP per level
            PlayerHP = MaxPlayerHP; // Restore to full HP on level up
            CombatLog += $" LEVEL UP {PlayerLevel}! +2 MaxHP! HP restored!";
        }
    }
}
