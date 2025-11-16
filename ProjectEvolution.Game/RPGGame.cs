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
    public int CombatsWon { get; private set; } = 0;
    public bool RunEnded { get; private set; } = false;
    public int PlayerX { get; private set; } = 10;
    public int PlayerY { get; private set; } = 10;
    public int WorldWidth { get; private set; } = 20;
    public int WorldHeight { get; private set; } = 20;
    private string[,] _worldMap;
    public bool InLocation { get; private set; } = false;
    public string CurrentLocation { get; private set; } = null;
    public int PotionCount { get; private set; } = 0;
    public bool InDungeon { get; private set; } = false;
    public int DungeonDepth { get; private set; } = 0;
    public int WorldTurn { get; private set; } = 0;
    private List<Mob> _activeMobs = new List<Mob>();

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

    public void StartGameLoop()
    {
        _combatStarted = true;
        _hpCombat = true;
        PlayerHP = MaxPlayerHP;
        PlayerStamina = 12;
        CombatsWon = 0;
        RunEnded = false;
        int enemyLevel = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void ContinueToNextCombat()
    {
        _combatStarted = true;
        _hpCombat = true;
        PlayerHP = MaxPlayerHP; // Restore HP between fights
        PlayerStamina = 12; // Restore stamina
        int enemyLevel = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = string.Empty;
    }

    public void ExecuteGameLoopRoundWithRandomHits(CombatAction playerAction, CombatAction enemyAction)
    {
        HitType playerHit = DetermineHitType();
        HitType enemyHit = DetermineHitType();
        ExecuteGameLoopRound(playerAction, enemyAction, playerHit, enemyHit);
    }

    public void ExecuteGameLoopRound(CombatAction playerAction, CombatAction enemyAction,
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
            RunEnded = true;
            CombatLog += "DEATH! Run ended!";
        }
    }

    public void ProcessGameLoopVictory()
    {
        if (IsWon && CombatEnded)
        {
            CombatsWon++;
            ProcessMaxHPGrowth(); // Check for level up
        }
    }

    private void GenerateWorld()
    {
        _worldMap = new string[WorldWidth, WorldHeight];
        string[] terrains = { "Grassland", "Forest", "Mountain", "Grassland", "Grassland", "Forest" }; // Weighted

        for (int x = 0; x < WorldWidth; x++)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                // Procedural generation with consistent seed
                int seed = x * 1000 + y;
                var localRandom = new Random(seed);
                _worldMap[x, y] = terrains[localRandom.Next(terrains.Length)];
            }
        }

        // Place special locations
        _worldMap[5, 5] = "Town";
        _worldMap[15, 15] = "Town";
        _worldMap[10, 5] = "Dungeon";
        _worldMap[10, 15] = "Dungeon";
    }

    public void StartWorldExploration()
    {
        PlayerX = 10;
        PlayerY = 10;
        PlayerHP = MaxPlayerHP; // CRITICAL FIX: Initialize HP!
        PlayerStamina = 12;
        WorldTurn = 0;
        GenerateWorld();
        SpawnMobs();
    }

    public bool MoveNorth()
    {
        if (PlayerY > 0)
        {
            PlayerY--;
            AdvanceTurnsByTerrain();
            return true;
        }
        return false;
    }

    public bool MoveSouth()
    {
        if (PlayerY < WorldHeight - 1)
        {
            PlayerY++;
            AdvanceTurnsByTerrain();
            return true;
        }
        return false;
    }

    public bool MoveEast()
    {
        if (PlayerX < WorldWidth - 1)
        {
            PlayerX++;
            AdvanceTurnsByTerrain();
            return true;
        }
        return false;
    }

    public bool MoveWest()
    {
        if (PlayerX > 0)
        {
            PlayerX--;
            AdvanceTurnsByTerrain();
            return true;
        }
        return false;
    }

    private void AdvanceTurnsByTerrain()
    {
        string terrain = GetCurrentTerrain();
        int turnCost = terrain switch
        {
            "Grassland" => 1,
            "Town" => 1,
            "Dungeon" => 1,
            "Forest" => 2,
            "Mountain" => 3,
            _ => 1
        };
        WorldTurn += turnCost;
    }

    public string GetCurrentTerrain()
    {
        return _worldMap[PlayerX, PlayerY];
    }

    public string GetTerrainAt(int x, int y)
    {
        if (x < 0 || x >= WorldWidth || y < 0 || y >= WorldHeight)
            return "OutOfBounds";
        return _worldMap[x, y];
    }

    public bool EnterLocation()
    {
        string terrain = GetCurrentTerrain();
        if (terrain == "Town" || terrain == "Dungeon")
        {
            InLocation = true;
            CurrentLocation = terrain;
            return true;
        }
        return false;
    }

    public void ExitLocation()
    {
        InLocation = false;
        CurrentLocation = null;
    }

    public bool RollForEncounter()
    {
        return RollForEncounterOnTerrain(GetCurrentTerrain());
    }

    public bool RollForEncounterOnTerrain(string terrain)
    {
        // Reduced random encounters now that mobs are visible
        // These are "ambush" encounters from hidden enemies
        int encounterChance = terrain switch
        {
            "Grassland" => 5,   // 5% chance (rare ambush)
            "Forest" => 15,     // 15% chance (enemies can hide)
            "Mountain" => 10,   // 10% chance (cave dwellers)
            "Town" => 0,        // Safe
            "Dungeon" => 0,     // Encounters handled differently in dungeons
            _ => 5
        };

        int roll = _random.Next(100);
        return roll < encounterChance;
    }

    public void TriggerEncounter()
    {
        // Start combat with level-scaled enemy
        _combatStarted = true;
        _hpCombat = true;
        PlayerStamina = 999; // ENCOUNTERS DON'T USE STAMINA (unlimited for random fights)
        int enemyLevel = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = "You are ambushed!";
    }

    public bool AttemptFlee()
    {
        // 50% chance to escape
        bool escaped = _random.Next(2) == 0;
        if (escaped)
        {
            CombatEnded = true;
            IsWon = false; // Didn't win, but didn't die either
            CombatLog = "You fled from combat!";
            return true;
        }
        else
        {
            // Failed to flee, enemy gets free hit!
            int damage = Math.Max(1, EnemyDamage - PlayerDefense);
            PlayerHP = Math.Max(0, PlayerHP - damage);
            CombatLog = $"Failed to flee! {EnemyName} hits you for {damage} damage!";

            if (PlayerHP <= 0)
            {
                CombatEnded = true;
                RunEnded = true;
                IsWon = false;
            }
            return false;
        }
    }

    public bool VisitInn()
    {
        const int innCost = 10;
        if (PlayerGold >= innCost)
        {
            PlayerGold -= innCost;
            PlayerHP = MaxPlayerHP;
            return true;
        }
        return false;
    }

    public bool BuyPotion()
    {
        const int potionCost = 5;
        if (PlayerGold >= potionCost)
        {
            PlayerGold -= potionCost;
            PotionCount++;
            return true;
        }
        return false;
    }

    public bool UsePotion()
    {
        if (PotionCount > 0)
        {
            PotionCount--;
            PlayerHP = Math.Min(MaxPlayerHP, PlayerHP + 5);
            return true;
        }
        return false;
    }

    // Test helpers
    public void SetGoldForTesting(int gold) => PlayerGold = gold;
    public void SetHPForTesting(int hp) => PlayerHP = hp;

    public void EnterDungeon()
    {
        InDungeon = true;
        DungeonDepth = 1;
    }

    public void ExitDungeon()
    {
        InDungeon = false;
        DungeonDepth = 0;
    }

    public void DescendDungeon()
    {
        if (InDungeon)
        {
            DungeonDepth++;
        }
    }

    public string RollForRoom()
    {
        // Warhammer Quest style room table
        int roll = _random.Next(100);
        if (roll < 50) return "Empty";      // 50% empty
        if (roll < 85) return "Monster";    // 35% monster
        return "Treasure";                   // 15% treasure
    }

    public void TriggerDungeonCombat()
    {
        // Combat in dungeons uses depth for enemy scaling
        _combatStarted = true;
        _hpCombat = true;
        PlayerStamina = 12;
        int enemyLevel = Math.Max(1, PlayerLevel + DungeonDepth); // Depth makes enemies harder!
        InitializeEnemyWithLevel((EnemyType)_random.Next(3), enemyLevel);
        IsWon = false;
        CombatEnded = false;
        CombatLog = $"A monster lurks in the darkness (Depth {DungeonDepth})!";
    }

    public int RollForTreasure(int dungeonDepth)
    {
        // Loot tables - deeper dungeons give more gold
        int baseGold = 10 + (_random.Next(3) * 10); // 10-30 base
        int depthBonus = dungeonDepth * 10; // +10 per depth
        int totalGold = baseGold + depthBonus;
        PlayerGold += totalGold;
        return totalGold;
    }

    public string RollForEvent()
    {
        // Event table
        int roll = _random.Next(100);
        if (roll < 60) return "Nothing";    // 60% nothing
        if (roll < 85) return "Trap";       // 25% trap
        return "Discovery";                  // 15% discovery
    }

    public int TriggerTrap()
    {
        int damage = _random.Next(1, 6); // 1-5 damage
        PlayerHP = Math.Max(0, PlayerHP - damage);
        return damage;
    }

    public string TriggerDiscovery()
    {
        // Discovery can give gold or XP
        int roll = _random.Next(2);
        if (roll == 0)
        {
            int goldFound = _random.Next(10, 31); // 10-30 gold
            PlayerGold += goldFound;
            return $"Found {goldFound} gold!";
        }
        else
        {
            int xpGained = _random.Next(10, 21); // 10-20 XP
            PlayerXP += xpGained;
            return $"Gained {xpGained} XP!";
        }
    }

    // ===== GENERATION 30: TURN-BASED WORLD & MOBS =====

    private void SpawnMobs()
    {
        _activeMobs.Clear();

        // Spawn 10-15 mobs across the world
        int mobCount = _random.Next(10, 16);
        string[] mobNames = { "Goblin Scout", "Orc Wanderer", "Wild Beast", "Bandit", "Skeleton" };

        for (int i = 0; i < mobCount; i++)
        {
            int x, y;
            string terrain;

            // Find valid spawn location (not on player, not in town)
            do
            {
                x = _random.Next(WorldWidth);
                y = _random.Next(WorldHeight);
                terrain = _worldMap[x, y];
            }
            while ((x == PlayerX && y == PlayerY) || terrain == "Town");

            string name = mobNames[_random.Next(mobNames.Length)];
            int level = Math.Max(1, PlayerLevel + _random.Next(-1, 2)); // Level ±1 of player

            _activeMobs.Add(new Mob(x, y, name, level));
        }
    }

    public int GetActiveMobCount()
    {
        return _activeMobs.Count;
    }

    public List<Mob> GetAllMobs()
    {
        return new List<Mob>(_activeMobs); // Return copy
    }

    public List<Mob> GetMobsInRange(int range)
    {
        var nearbyMobs = new List<Mob>();

        foreach (var mob in _activeMobs)
        {
            int distance = Math.Abs(mob.X - PlayerX) + Math.Abs(mob.Y - PlayerY);
            if (distance <= range)
            {
                nearbyMobs.Add(mob);
            }
        }

        return nearbyMobs;
    }

    public bool IsMobAt(int x, int y)
    {
        return _activeMobs.Any(m => m.X == x && m.Y == y);
    }

    public Mob? GetMobAt(int x, int y)
    {
        return _activeMobs.FirstOrDefault(m => m.X == x && m.Y == y);
    }

    public void RemoveMob(Mob mob)
    {
        _activeMobs.Remove(mob);
    }

    public void TriggerMobEncounter(Mob mob)
    {
        // Initialize combat with the mob (use random enemy type for stats)
        EnemyType randomType = (EnemyType)_random.Next(3);
        InitializeEnemyWithVariableStats(randomType);

        // Override with mob's name and level
        EnemyName = mob.Name;
        EnemyLevel = mob.Level;

        // Scale mob stats based on level
        int baseHP = EnemyHP;
        int baseDamage = EnemyDamage;

        EnemyHP = baseHP + ((mob.Level - 1) * 2);
        EnemyDamage = Math.Max(1, baseDamage + ((mob.Level - 1) / 2));

        CombatEnded = false;
        PlayerStamina = 999; // Infinite stamina for encounters
        CombatLog = $"Encountered {mob.Name} [Level {mob.Level}]!";
    }

    // Testing helper methods
    public void SetTerrainForTesting(int x, int y, string terrain)
    {
        if (x >= 0 && x < WorldWidth && y >= 0 && y < WorldHeight)
        {
            _worldMap[x, y] = terrain;
        }
    }
}
