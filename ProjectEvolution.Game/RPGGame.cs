namespace ProjectEvolution.Game;

public class DungeonState
{
    public string[,] Map { get; set; }
    public bool[,] FogOfWar { get; set; }
    public int MaxDepth { get; set; }
    public int CurrentDepth { get; set; }
    public bool BossDefeated { get; set; }
    public bool ArtifactCollected { get; set; }
    public int PlayerStartX { get; set; }
    public int PlayerStartY { get; set; }
    public List<Mob> DungeonMobs { get; set; } = new List<Mob>();

    public DungeonState(int width, int height, int maxDepth)
    {
        Map = new string[width, height];
        FogOfWar = new bool[width, height]; // true = explored
        MaxDepth = maxDepth;
        CurrentDepth = 1;
        BossDefeated = false;
        ArtifactCollected = false;
    }
}

public partial class RPGGame
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
    public int PlayerStrength { get; private set; } = 10; // One-shot everything!
    public int PlayerDefense { get; private set; } = 0;
    public int PlayerStamina { get; private set; } = 12;
    public int EnemyDamage { get; private set; } = 1;
    public string EnemyName { get; private set; } = "Goblin";
    public EnemyType CurrentEnemyType { get; private set; } = EnemyType.GoblinScout;
    public int PermanentGold { get; private set; } = 0;
    public int DeathCount { get; private set; } = 0;
    public int PlayerXP { get; private set; } = 0;
    public int PlayerLevel { get; private set; } = 1;
    public int XPForNextLevel { get; private set; } = 100;
    public int AvailableStatPoints { get; private set; } = 0;
    public int EnemyLevel { get; private set; } = 1;
    public int MaxPlayerHP { get; private set; } = 25; // Balanced starting HP
    private int _configuredMaxMobs = 29; // Can be overridden by tuning results
    private int _configuredMobDetection = 3; // Can be overridden
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
    public Inventory PlayerInventory { get; private set; } = new Inventory();
    public bool InDungeon { get; private set; } = false;
    public int DungeonDepth { get; private set; } = 0;
    public int MaxDungeonDepth { get; private set; } = 3;
    public bool BossDefeated { get; private set; } = false;
    public bool ArtifactCollected { get; private set; } = false;
    public int DungeonsCompleted { get; private set; } = 0;
    public bool RunWon { get; private set; } = false;
    public int WorldTurn { get; private set; } = 0;
    private List<Mob> _activeMobs = new List<Mob>();
    private bool[,] _exploredTiles;
    private int _mobDetectionRange = 2; // Easy mode
    private const int PlayerVisionRange = 3; // Player can see 3 tiles around
    private int _maxMobsInWorld = 3; // TUTORIAL - almost empty!
    private const int MinMobsInWorld = 1;  // Bare minimum
    private string[,] _dungeonMap;
    private int _dungeonWidth = 30;
    private int _dungeonHeight = 30;
    private Dictionary<(int x, int y), Dictionary<int, DungeonState>> _persistentDungeons = new Dictionary<(int, int), Dictionary<int, DungeonState>>();
    private (int x, int y) _currentDungeonLocation;
    private DungeonState _currentDungeonState;

    // NEW: Death/Respawn System
    public int? DeathLocationX { get; private set; } = null;
    public int? DeathLocationY { get; private set; } = null;
    private Weapon? _droppedWeapon = null;
    private Armor? _droppedArmor = null;
    public int TotalDeaths { get; private set; } = 0;

    // NEW: Skill & Buff System (Generation 35)
    public List<ActiveBuff> PlayerBuffs { get; private set; } = new List<ActiveBuff>();
    public List<ActiveBuff> EnemyBuffs { get; private set; } = new List<ActiveBuff>();
    private HashSet<string> _usedOncePerCombatSkills = new HashSet<string>();
    public string LastSkillUsed { get; private set; } = "";
    public Skill? QueuedSkill { get; private set; } = null;
    private int _enemyStunResistance = 0; // Builds up each time enemy is stunned (anti-stun-lock)

    public RPGGame()
    {
        _worldMap = new string[WorldWidth, WorldHeight];
        _exploredTiles = new bool[WorldWidth, WorldHeight];
        _dungeonMap = new string[_dungeonWidth, _dungeonHeight];
        _currentDungeonState = new DungeonState(_dungeonWidth, _dungeonHeight, MaxDungeonDepth);
        
        GenerateWorld();
    }

    public void SetPlayerStats(int strength, int defense)
    {
        PlayerStrength = strength;
        PlayerDefense = defense;
    }

    public void SetOptimalConfig(SimulationConfig config)
    {
        // Apply tuning results to the game!
        MaxPlayerHP = config.PlayerStartHP;
        _maxMobsInWorld = config.MaxMobs;
        _mobDetectionRange = config.MobDetectionRange;
        PlayerStrength = config.PlayerStrength;
        PlayerDefense = config.PlayerDefense;
    }

    public void StartWorldExploration()
    {
        PlayerX = 10;
        PlayerY = 10;
        PlayerHP = MaxPlayerHP;
        PlayerStamina = 12;
        WorldTurn = 0;
        GenerateWorld();
        SpawnMobs();
        InitializeFogOfWar();
        RevealAreaAroundPlayer();
    }

    public int GetEffectiveStrength()
    {
        // Include equipment bonuses!
        return PlayerInventory.GetTotalStrength(PlayerStrength);
    }

    public int GetEffectiveDefense()
    {
        // Include equipment bonuses!
        return PlayerInventory.GetTotalDefense(PlayerDefense);
    }

    public bool BuyWeapon(Weapon weapon)
    {
        if (PlayerGold >= weapon.Value)
        {
            PlayerGold -= weapon.Value;
            
            // Move current weapon to inventory if it's not the default rusty dagger
            if (PlayerInventory.EquippedWeapon.Name != "Rusty Dagger")
            {
                PlayerInventory.Weapons.Add(PlayerInventory.EquippedWeapon);
            }
            
            PlayerInventory.EquippedWeapon = weapon;
            return true;
        }
        return false;
    }

    public bool BuyArmor(Armor armor)
    {
        if (PlayerGold >= armor.Value)
        {
            PlayerGold -= armor.Value;
            
            // Move current armor to inventory if it's not the default cloth rags
            if (PlayerInventory.EquippedArmor.Name != "Cloth Rags")
            {
                PlayerInventory.Armors.Add(PlayerInventory.EquippedArmor);
            }
            
            PlayerInventory.EquippedArmor = armor;
            return true;
        }
        return false;
    }

    public List<Weapon> GetShopWeapons()
    {
        // Return all weapons except the starter one
        return Weapon.AllWeapons.Skip(1).ToList();
    }

    public List<Armor> GetShopArmors()
    {
        // Return all armor except the starter one
        return Armor.AllArmor.Skip(1).ToList();
    }

    // ════════════════════════════════════════════════════════════════════
    // SKILL SYSTEM (Generation 35)
    // ════════════════════════════════════════════════════════════════════

    public bool CanUseSkill(Skill skill)
    {
        // Check level requirement
        if (PlayerLevel < skill.MinLevel) return false;

        // Check stamina
        if (PlayerStamina < skill.StaminaCost) return false;

        // Check once-per-combat restriction
        if (skill.OncePerCombat && _usedOncePerCombatSkills.Contains(skill.Name)) return false;

        return true;
    }

    public (bool success, string message) UseSkill(Skill skill)
    {
        if (!CanUseSkill(skill))
        {
            if (PlayerStamina < skill.StaminaCost)
                return (false, $"Not enough stamina! Need {skill.StaminaCost}, have {PlayerStamina}");
            if (skill.OncePerCombat && _usedOncePerCombatSkills.Contains(skill.Name))
                return (false, $"{skill.Name} already used this combat!");
            return (false, $"Cannot use {skill.Name}!");
        }

        // Deduct stamina
        PlayerStamina -= skill.StaminaCost;
        LastSkillUsed = skill.Name;

        // Mark as used if once per combat
        if (skill.OncePerCombat)
        {
            _usedOncePerCombatSkills.Add(skill.Name);
        }

        // Execute skill effect
        string message = "";
        switch (skill.PrimaryEffect)
        {
            case SkillEffect.Damage:
                // Damage skill - will be applied in ExecuteGameLoopRound
                message = $"Used {skill.Name}!";
                break;

            case SkillEffect.Heal:
                int healAmount = skill.HealAmount;
                int actualHeal = Math.Min(healAmount, MaxPlayerHP - PlayerHP);
                PlayerHP = Math.Min(MaxPlayerHP, PlayerHP + healAmount);
                message = $"Used {skill.Name}! Restored {actualHeal} HP!";
                break;

            case SkillEffect.Buff:
                var buff = new ActiveBuff(skill.BuffApplied, skill.BuffDuration, skill.BuffValue);
                PlayerBuffs.Add(buff);
                message = $"Used {skill.Name}! Active for {skill.BuffDuration} turns!";
                break;

            case SkillEffect.Stun:
                // Apply stun to enemy with resistance check (anti-stun-lock)
                var random = new Random();
                int stunChance = 100 - (_enemyStunResistance * 25); // 100%, 75%, 50%, 25%, 0%...
                if (random.Next(100) < stunChance)
                {
                    var stunBuff = new ActiveBuff(BuffType.Stunned, skill.BuffDuration);
                    EnemyBuffs.Add(stunBuff);
                    _enemyStunResistance++; // Build resistance
                    message = $"Used {skill.Name}! Enemy stunned!";
                }
                else
                {
                    message = $"Used {skill.Name}! Enemy resisted stun! (Resist: {_enemyStunResistance})";
                }
                break;
        }

        return (true, message);
    }

    public int CalculateSkillDamage(Skill skill, int baseStrength)
    {
        // Apply skill multiplier
        double damageMultiplier = skill.DamageMultiplier;

        // Apply Berserker Rage if active
        var rageBuff = PlayerBuffs.FirstOrDefault(b => b.Type == BuffType.BerserkerRage);
        if (rageBuff != null)
        {
            damageMultiplier *= 2.0; // Berserker doubles damage
        }

        return (int)(baseStrength * damageMultiplier);
    }

    public int ApplyDefenseBuffs(int baseDefense)
    {
        int totalDefense = baseDefense;

        // Apply Defensive Stance
        var defenseBuff = PlayerBuffs.FirstOrDefault(b => b.Type == BuffType.DefensiveStance);
        if (defenseBuff != null)
        {
            totalDefense += defenseBuff.Value; // +5 defense
        }

        return totalDefense;
    }

    public int ApplyIncomingDamageModifiers(int damage)
    {
        // Berserker Rage increases damage taken by 50%
        var rageBuff = PlayerBuffs.FirstOrDefault(b => b.Type == BuffType.BerserkerRage);
        if (rageBuff != null)
        {
            damage = (int)(damage * 1.5);
        }

        return damage;
    }

    public bool IsEnemyStunned()
    {
        return EnemyBuffs.Any(b => b.Type == BuffType.Stunned);
    }

    public bool IsInDefensiveStance()
    {
        return PlayerBuffs.Any(b => b.Type == BuffType.DefensiveStance);
    }

    public void TickBuffs()
    {
        // Decrement player buff durations
        for (int i = PlayerBuffs.Count - 1; i >= 0; i--)
        {
            PlayerBuffs[i].TurnsRemaining--;
            if (PlayerBuffs[i].TurnsRemaining <= 0)
            {
                PlayerBuffs.RemoveAt(i);
            }
        }

        // Decrement enemy buff durations
        for (int i = EnemyBuffs.Count - 1; i >= 0; i--)
        {
            EnemyBuffs[i].TurnsRemaining--;
            if (EnemyBuffs[i].TurnsRemaining <= 0)
            {
                EnemyBuffs.RemoveAt(i);
            }
        }
    }

    public void ClearCombatBuffsAndSkills()
    {
        PlayerBuffs.Clear();
        EnemyBuffs.Clear();
        _usedOncePerCombatSkills.Clear();
        LastSkillUsed = "";
        _enemyStunResistance = 0; // Reset stun resistance for new combat
    }

    public List<Skill> GetAvailableSkills()
    {
        return Skill.GetAvailableSkills(PlayerLevel);
    }

    public string GetActiveBuffsDisplay()
    {
        var buffs = new List<string>();
        foreach (var buff in PlayerBuffs)
        {
            string buffName = buff.Type switch
            {
                BuffType.BerserkerRage => "🔥RAGE",
                BuffType.DefensiveStance => "🛡️DEF",
                _ => buff.Type.ToString()
            };
            buffs.Add($"{buffName}({buff.TurnsRemaining})");
        }
        return buffs.Count > 0 ? string.Join(" ", buffs) : "";
    }

    public void QueueSkillForNextRound(Skill skill)
    {
        QueuedSkill = skill;
    }

    public void ClearQueuedSkill()
    {
        QueuedSkill = null;
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
        CurrentEnemyType = enemyType; // Store the enemy type!

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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int baseDamage = GetEffectiveStrength(); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
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
                int actualDamage = Math.Max(1, damage - GetEffectiveDefense()); // Include equipment bonuses!
                PlayerHP = Math.Max(0, PlayerHP - actualDamage);
                CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
            }
            else CombatLog += $"{EnemyName} misses! ";
        }

        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            int xpGained = GetEnemyXPValue(CurrentEnemyType);
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
            int xpGained = GetEnemyXPValue(CurrentEnemyType);
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
            int xpGained = GetEnemyXPValue(CurrentEnemyType);
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

        // === GENERATION 35: SKILL SYSTEM ===
        // Handle skill usage
        if (playerAction == CombatAction.UseSkill && QueuedSkill != null)
        {
            var (success, message) = UseSkill(QueuedSkill);
            CombatLog += message + " ";

            // Skills consume stamina via UseSkill, so we don't deduct again
            // For damage skills, we'll apply damage below
            ClearQueuedSkill();
        }
        else
        {
            // Normal action stamina costs
            CombatAction actualPlayerAction = playerAction;
            if (PlayerStamina < 3 && playerAction == CombatAction.Attack)
            {
                actualPlayerAction = CombatAction.Defend;
                CombatLog += "No stamina! ";
            }

            if (actualPlayerAction == CombatAction.Attack) PlayerStamina = Math.Max(0, PlayerStamina - 3);
            else PlayerStamina = Math.Max(0, PlayerStamina - 1);
        }

        bool playerAttacks = playerAction == CombatAction.Attack ||
                            (playerAction == CombatAction.UseSkill && LastSkillUsed == "Power Strike") ||
                            (playerAction == CombatAction.UseSkill && LastSkillUsed == "Shield Bash");
        bool playerDefends = playerAction == CombatAction.Defend;

        // Defensive Stance prevents attacking
        if (IsInDefensiveStance() && playerAttacks)
        {
            playerAttacks = false;
            CombatLog += "[Defensive Stance] ";
        }

        bool enemyAttacks = enemyAction == CombatAction.Attack;
        bool enemyDefends = enemyAction == CombatAction.Defend;

        // Enemy is stunned - skips turn
        bool enemyStunned = IsEnemyStunned();
        if (enemyStunned)
        {
            enemyAttacks = false;
            CombatLog += "[Enemy Stunned] ";
        }

        // Player attacks (with skill damage multipliers)
        if (playerAttacks && !enemyDefends && playerHitType != HitType.Miss)
        {
            int baseDamage = playerHitType == HitType.Critical ? GetEffectiveStrength() * 2 : GetEffectiveStrength();

            // Apply skill damage multiplier if using a damage skill
            if (LastSkillUsed == "Power Strike")
            {
                baseDamage = CalculateSkillDamage(Skill.PowerStrike, GetEffectiveStrength());
                if (playerHitType == HitType.Critical) baseDamage *= 2;
            }
            else if (LastSkillUsed == "Shield Bash")
            {
                baseDamage = CalculateSkillDamage(Skill.ShieldBash, GetEffectiveStrength());
                if (playerHitType == HitType.Critical) baseDamage *= 2;
            }

            // Apply Berserker Rage if active (for normal attacks)
            if (LastSkillUsed == "" && PlayerBuffs.Any(b => b.Type == BuffType.BerserkerRage))
            {
                baseDamage *= 2;
                CombatLog += "[RAGE] ";
            }

            EnemyHP = Math.Max(0, EnemyHP - baseDamage);
            CombatLog += playerHitType == HitType.Critical ? $"CRIT {baseDamage}! " : $"Hit {baseDamage}! ";
        }
        else if (playerAttacks) CombatLog += "MISS! ";

        // Enemy attacks (with defense buffs applied)
        if (enemyAttacks && !playerDefends && enemyHitType != HitType.Miss)
        {
            int damage = enemyHitType == HitType.Critical ? EnemyDamage * 2 : EnemyDamage;
            int effectiveDefense = ApplyDefenseBuffs(GetEffectiveDefense());
            int actualDamage = Math.Max(1, damage - effectiveDefense);

            // Apply damage modifiers (Berserker Rage increases damage taken)
            actualDamage = ApplyIncomingDamageModifiers(actualDamage);

            PlayerHP = Math.Max(0, PlayerHP - actualDamage);
            CombatLog += enemyHitType == HitType.Critical ? $"{EnemyName} CRIT {actualDamage}! " : $"{EnemyName} {actualDamage}! ";
        }
        else if (enemyAttacks && !enemyStunned) CombatLog += $"{EnemyName} misses! ";

        // Tick buffs at end of round
        TickBuffs();
        LastSkillUsed = ""; // Clear skill for next round

        if (EnemyHP <= 0)
        {
            IsWon = true;
            CombatEnded = true;
            int xpGained = GetEnemyXPValue(CurrentEnemyType);
            PlayerXP += xpGained;
            PlayerGold += 10;
            CombatLog += $"Victory! +10g +{xpGained}xp!";
            ClearCombatBuffsAndSkills(); // Clear buffs when combat ends
        }
        else if (PlayerHP <= 0)
        {
            IsWon = false;
            CombatEnded = true;
            RunEnded = true;
            CombatLog += "DEATH! Run ended!";
            ClearCombatBuffsAndSkills();
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
        _worldMap[10, 10] = "Temple"; // RESPAWN POINT - Center of world
        _worldMap[5, 5] = "Town";
        _worldMap[15, 15] = "Town";
        _worldMap[10, 5] = "Dungeon";
        _worldMap[10, 15] = "Dungeon";
    }


    private void InitializeFogOfWar()
    {
        _exploredTiles = new bool[WorldWidth, WorldHeight];
        // All tiles start unexplored
        for (int x = 0; x < WorldWidth; x++)
        {
            for (int y = 0; y < WorldHeight; y++)
            {
                _exploredTiles[x, y] = false;
            }
        }
    }

    private void RevealAreaAroundPlayer()
    {
        // Reveal area around player based on vision range
        for (int dx = -PlayerVisionRange; dx <= PlayerVisionRange; dx++)
        {
            for (int dy = -PlayerVisionRange; dy <= PlayerVisionRange; dy++)
            {
                int x = PlayerX + dx;
                int y = PlayerY + dy;

                if (x >= 0 && x < WorldWidth && y >= 0 && y < WorldHeight)
                {
                    _exploredTiles[x, y] = true;
                }
            }
        }
    }

    public bool MoveNorth()
    {
        if (InDungeon)
        {
            return MoveDungeon(0, -1);
        }

        if (PlayerY > 0)
        {
            PlayerY--;
            AdvanceTurnsByTerrain();
            RevealAreaAroundPlayer();
            TickWorld(); // Mobs move after player
            return true;
        }
        return false;
    }

    public bool MoveSouth()
    {
        if (InDungeon)
        {
            return MoveDungeon(0, 1);
        }

        if (PlayerY < WorldHeight - 1)
        {
            PlayerY++;
            AdvanceTurnsByTerrain();
            RevealAreaAroundPlayer();
            TickWorld();
            return true;
        }
        return false;
    }

    public bool MoveEast()
    {
        if (InDungeon)
        {
            return MoveDungeon(1, 0);
        }

        if (PlayerX < WorldWidth - 1)
        {
            PlayerX++;
            AdvanceTurnsByTerrain();
            RevealAreaAroundPlayer();
            TickWorld();
            return true;
        }
        return false;
    }

    public bool MoveWest()
    {
        if (InDungeon)
        {
            return MoveDungeon(-1, 0);
        }

        if (PlayerX > 0)
        {
            PlayerX--;
            AdvanceTurnsByTerrain();
            RevealAreaAroundPlayer();
            TickWorld();
            return true;
        }
        return false;
    }

    private bool MoveDungeon(int dx, int dy)
    {
        int newX = PlayerX + dx;
        int newY = PlayerY + dy;

        if (newX < 0 || newX >= _dungeonWidth || newY < 0 || newY >= _dungeonHeight)
            return false;

        string tile = _dungeonMap[newX, newY];
        if (tile == "Wall")
            return false;

        PlayerX = newX;
        PlayerY = newY;
        WorldTurn++;
        return true;
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
        // In dungeon, return "Dungeon" (don't access world map!)
        if (InDungeon) return "Dungeon";

        // Bounds check for world map
        if (PlayerX < 0 || PlayerX >= WorldWidth || PlayerY < 0 || PlayerY >= WorldHeight)
            return "OutOfBounds";

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
            int damage = Math.Max(1, EnemyDamage - GetEffectiveDefense());
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

    public void EndCombatStalemate()
    {
        // Force end combat due to excessive rounds (infinite miss loop)
        CombatEnded = true;
        IsWon = false;
        CombatLog = "Combat stalemate - both fighters disengage!";
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
        _currentDungeonLocation = (PlayerX, PlayerY);

        // Calculate max depth based on player level (scales with progression)
        MaxDungeonDepth = 3 + (PlayerLevel / 2); // Lvl 1 → 3 floors, Lvl 10 → 8 floors

        // Check if dungeon exists at this location
        if (!_persistentDungeons.ContainsKey(_currentDungeonLocation))
        {
            _persistentDungeons[_currentDungeonLocation] = new Dictionary<int, DungeonState>();
        }

        // Check if we have a saved state for depth 1
        if (!_persistentDungeons[_currentDungeonLocation].ContainsKey(1))
        {
            // Create new dungeon for depth 1
            _currentDungeonState = new DungeonState(_dungeonWidth, _dungeonHeight, MaxDungeonDepth);
            _persistentDungeons[_currentDungeonLocation][1] = _currentDungeonState;
            DungeonDepth = 1;
            GenerateDungeonMap(); // Generate new floor

            // Find starting position
            for (int x = 0; x < _dungeonWidth; x++)
            {
                for (int y = 0; y < _dungeonHeight; y++)
                {
                    if (_dungeonMap[x, y] == "Floor")
                    {
                        PlayerX = x;
                        PlayerY = y;
                        _currentDungeonState.PlayerStartX = x;
                        _currentDungeonState.PlayerStartY = y;
                        break;
                    }
                }
                if (_dungeonMap[PlayerX, PlayerY] == "Floor") break;
            }
        }
        else
        {
            // Load existing dungeon
            _currentDungeonState = _persistentDungeons[_currentDungeonLocation][1];
            DungeonDepth = _currentDungeonState.CurrentDepth;
            MaxDungeonDepth = _currentDungeonState.MaxDepth;
            BossDefeated = _currentDungeonState.BossDefeated;
            ArtifactCollected = _currentDungeonState.ArtifactCollected;
            _dungeonMap = _currentDungeonState.Map;

            // Restore player position
            PlayerX = _currentDungeonState.PlayerStartX;
            PlayerY = _currentDungeonState.PlayerStartY;
        }
    }

    private void GenerateDungeonMap()
    {
        _dungeonMap = new string[_dungeonWidth, _dungeonHeight];

        // Fill with walls
        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                _dungeonMap[x, y] = "Wall";
            }
        }

        // Carve out rooms and corridors
        CarveRooms();

        // Place visible features!
        PlaceDungeonFeatures();

        // Save to current dungeon state
        if (_currentDungeonState != null)
        {
            _currentDungeonState.Map = _dungeonMap;
            _currentDungeonState.CurrentDepth = DungeonDepth;
            _currentDungeonState.BossDefeated = BossDefeated;
            _currentDungeonState.ArtifactCollected = ArtifactCollected;
        }
    }

    private void PlaceDungeonFeatures()
    {
        // Find all floor tiles
        var floorTiles = new List<(int x, int y)>();
        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                if (_dungeonMap[x, y] == "Floor")
                {
                    floorTiles.Add((x, y));
                }
            }
        }

        if (floorTiles.Count == 0) return;

        // BOSS FLOOR: If on final depth, spawn boss instead of regular features
        bool isBossFloor = (DungeonDepth == MaxDungeonDepth);

        if (isBossFloor && !BossDefeated)
        {
            // Place boss in center of map
            var bossTile = floorTiles[floorTiles.Count / 2];
            _dungeonMap[bossTile.x, bossTile.y] = "Boss";
            floorTiles.Remove(bossTile);

            // Fewer features on boss floor (2-3 treasures, 2-4 traps, no monsters)
            int treasureCount = _random.Next(2, 4);
            for (int i = 0; i < treasureCount && floorTiles.Count > 0; i++)
            {
                var tile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[tile.x, tile.y] = "Treasure";
                floorTiles.Remove(tile);
            }

            int trapCount = _random.Next(2, 5);
            for (int i = 0; i < trapCount && floorTiles.Count > 0; i++)
            {
                var tile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[tile.x, tile.y] = "Trap";
                floorTiles.Remove(tile);
            }
        }
        else if (isBossFloor && BossDefeated)
        {
            // Boss defeated! Place artifact and exit portal
            if (!ArtifactCollected && floorTiles.Count > 0)
            {
                var artifactTile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[artifactTile.x, artifactTile.y] = "Artifact";
                floorTiles.Remove(artifactTile);
            }

            // Exit portal appears
            if (floorTiles.Count > 0)
            {
                var portalTile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[portalTile.x, portalTile.y] = "Portal";
                floorTiles.Remove(portalTile);
            }
        }
        else
        {
            // Normal floor features
            // Place 3-5 treasure chests
            int treasureCount = _random.Next(3, 6);
            for (int i = 0; i < treasureCount && floorTiles.Count > 0; i++)
            {
                var tile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[tile.x, tile.y] = "Treasure";
                floorTiles.Remove(tile);
            }

            // Place 5-8 traps (visible!)
            int trapCount = _random.Next(5, 9);
            for (int i = 0; i < trapCount && floorTiles.Count > 0; i++)
            {
                var tile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[tile.x, tile.y] = "Trap";
                floorTiles.Remove(tile);
            }

            // Place 3-5 monsters as actual mob entities (not tiles!)
            int monsterCount = _random.Next(3, 6);
            if (_currentDungeonState != null)
            {
                _currentDungeonState.DungeonMobs.Clear(); // Clear old mobs

                for (int i = 0; i < monsterCount && floorTiles.Count > 0; i++)
                {
                    var tile = floorTiles[_random.Next(floorTiles.Count)];

                    // Create dungeon mob at this location
                    int mobLevel = Math.Max(1, PlayerLevel + DungeonDepth);
                    var mob = new Mob(
                        tile.x, tile.y,
                        "Dungeon Monster",
                        mobLevel,
                        (EnemyType)_random.Next(3)
                    );
                    _currentDungeonState.DungeonMobs.Add(mob);

                    floorTiles.Remove(tile);
                }
            }

            // Place exit stairs (not on boss floor)
            if (floorTiles.Count > 0)
            {
                var exitTile = floorTiles[_random.Next(floorTiles.Count)];
                _dungeonMap[exitTile.x, exitTile.y] = "Stairs";
            }
        }
    }

    private void CarveRooms()
    {
        // Simple room generation: create 5-8 rectangular rooms
        int roomCount = _random.Next(5, 9);
        List<(int x, int y, int width, int height)> rooms = new List<(int, int, int, int)>();

        for (int i = 0; i < roomCount; i++)
        {
            int width = _random.Next(4, 8);
            int height = _random.Next(4, 8);
            int x = _random.Next(1, _dungeonWidth - width - 1);
            int y = _random.Next(1, _dungeonHeight - height - 1);

            // Carve room
            for (int rx = x; rx < x + width; rx++)
            {
                for (int ry = y; ry < y + height; ry++)
                {
                    _dungeonMap[rx, ry] = "Floor";
                }
            }

            rooms.Add((x, y, width, height));

            // Connect to previous room with corridor
            if (i > 0)
            {
                var prevRoom = rooms[i - 1];
                int prevCenterX = prevRoom.x + prevRoom.width / 2;
                int prevCenterY = prevRoom.y + prevRoom.height / 2;
                int currCenterX = x + width / 2;
                int currCenterY = y + height / 2;

                // Horizontal corridor
                int startX = Math.Min(prevCenterX, currCenterX);
                int endX = Math.Max(prevCenterX, currCenterX);
                for (int cx = startX; cx <= endX; cx++)
                {
                    _dungeonMap[cx, prevCenterY] = "Floor";
                }

                // Vertical corridor
                int startY = Math.Min(prevCenterY, currCenterY);
                int endY = Math.Max(prevCenterY, currCenterY);
                for (int cy = startY; cy <= endY; cy++)
                {
                    _dungeonMap[currCenterX, cy] = "Floor";
                }
            }
        }
    }

    public void ExitDungeon()
    {
        InDungeon = false;
        DungeonDepth = 0;
        _dungeonMap = null;
    }

    public void DescendDungeon()
    {
        if (!InDungeon) return;

        DungeonDepth++;

        // Check if we've been to this depth before
        if (_persistentDungeons[_currentDungeonLocation].ContainsKey(DungeonDepth))
        {
            // Load existing floor
            _currentDungeonState = _persistentDungeons[_currentDungeonLocation][DungeonDepth];
            _dungeonMap = _currentDungeonState.Map;
            BossDefeated = _currentDungeonState.BossDefeated;
            ArtifactCollected = _currentDungeonState.ArtifactCollected;

            // Restore player position
            PlayerX = _currentDungeonState.PlayerStartX;
            PlayerY = _currentDungeonState.PlayerStartY;
        }
        else
        {
            // Create new floor
            _currentDungeonState = new DungeonState(_dungeonWidth, _dungeonHeight, MaxDungeonDepth);
            _currentDungeonState.CurrentDepth = DungeonDepth;
            _persistentDungeons[_currentDungeonLocation][DungeonDepth] = _currentDungeonState;

            // Generate new floor layout
            GenerateDungeonMap();

            // Place player on first floor tile
            for (int x = 0; x < _dungeonWidth; x++)
            {
                for (int y = 0; y < _dungeonHeight; y++)
                {
                    if (_dungeonMap[x, y] == "Floor")
                    {
                        PlayerX = x;
                        PlayerY = y;
                        _currentDungeonState.PlayerStartX = x;
                        _currentDungeonState.PlayerStartY = y;
                        return;
                    }
                }
            }
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

    public void TriggerBossCombat()
    {
        // Boss combat! Much harder than regular monsters
        _combatStarted = true;
        _hpCombat = true;
        PlayerStamina = 12;

        // Boss level scales with player level and dungeon depth
        int bossLevel = PlayerLevel + MaxDungeonDepth;

        // Boss type depends on depth
        string bossName;
        int bonusHP;
        EnemyType bossBaseType;

        if (MaxDungeonDepth <= 3)
        {
            bossName = "Goblin King";
            bonusHP = 20;
            bossBaseType = EnemyType.GoblinWarrior; // Warrior-type boss
        }
        else if (MaxDungeonDepth <= 6)
        {
            bossName = "Troll Chieftain";
            bonusHP = 40;
            bossBaseType = EnemyType.GoblinWarrior; // Strong boss
        }
        else
        {
            bossName = "Ancient Dragon";
            bonusHP = 80;
            bossBaseType = EnemyType.GoblinWarrior; // Epic boss
        }

        // Initialize boss with enhanced stats
        InitializeEnemyWithLevel(bossBaseType, bossLevel);
        EnemyName = bossName; // Override display name
        EnemyHP += bonusHP; // Boss has extra HP!

        IsWon = false;
        CombatEnded = false;
        CombatLog = $"💀 {bossName} [Lvl{bossLevel}] - Final Boss!";
    }

    public void MarkBossDefeated()
    {
        BossDefeated = true;
        if (_currentDungeonState != null)
        {
            _currentDungeonState.BossDefeated = true;
        }

        // Regenerate map to spawn artifact and portal
        GenerateDungeonMap();

        // Place player on a floor tile (avoid spawning on artifact/portal)
        for (int x = 0; x < _dungeonWidth; x++)
        {
            for (int y = 0; y < _dungeonHeight; y++)
            {
                if (_dungeonMap[x, y] == "Floor")
                {
                    PlayerX = x;
                    PlayerY = y;
                    if (_currentDungeonState != null)
                    {
                        _currentDungeonState.PlayerStartX = x;
                        _currentDungeonState.PlayerStartY = y;
                    }
                    return;
                }
            }
        }
    }

    public void MarkDungeonTileExplored(int x, int y)
    {
        if (_currentDungeonState != null && x >= 0 && x < _dungeonWidth && y >= 0 && y < _dungeonHeight)
        {
            _currentDungeonState.FogOfWar[x, y] = true;
        }
    }

    public bool IsDungeonTileExplored(int x, int y)
    {
        if (_currentDungeonState == null || x < 0 || x >= _dungeonWidth || y < 0 || y >= _dungeonHeight)
            return false;
        return _currentDungeonState.FogOfWar[x, y];
    }

    public List<Mob> GetDungeonMobs()
    {
        if (_currentDungeonState == null)
            return new List<Mob>();
        return new List<Mob>(_currentDungeonState.DungeonMobs);
    }

    public Mob? GetDungeonMobAt(int x, int y)
    {
        if (_currentDungeonState == null)
            return null;
        return _currentDungeonState.DungeonMobs.FirstOrDefault(m => m.X == x && m.Y == y);
    }

    public bool IsDungeonMobAt(int x, int y)
    {
        return GetDungeonMobAt(x, y) != null;
    }

    public void RemoveDungeonMob(Mob mob)
    {
        if (_currentDungeonState != null)
        {
            _currentDungeonState.DungeonMobs.Remove(mob);
        }
    }

    public void MoveDungeonMobs()
    {
        if (_currentDungeonState == null || _currentDungeonState.DungeonMobs.Count == 0)
            return;

        foreach (var mob in _currentDungeonState.DungeonMobs.ToList())
        {
            // Simple random movement for dungeon mobs
            int direction = _random.Next(5); // 0-3 = move, 4 = stay

            int newX = mob.X;
            int newY = mob.Y;

            switch (direction)
            {
                case 0: newY--; break; // North
                case 1: newY++; break; // South
                case 2: newX++; break; // East
                case 3: newX--; break; // West
                case 4: continue; // Stay put
            }

            // Check if new position is valid (floor tile, no other mob there)
            if (newX >= 0 && newX < _dungeonWidth && newY >= 0 && newY < _dungeonHeight)
            {
                string tile = _dungeonMap[newX, newY];
                bool hasOtherMob = _currentDungeonState.DungeonMobs.Any(m => m != mob && m.X == newX && m.Y == newY);

                if (tile == "Floor" && !hasOtherMob && (newX != PlayerX || newY != PlayerY))
                {
                    mob.X = newX;
                    mob.Y = newY;
                }
            }
        }
    }

    public string CollectArtifact()
    {
        if (ArtifactCollected) return "Already collected!";

        ArtifactCollected = true;

        // Determine equipment tier based on player level
        int tierIndex = Math.Min(4, (PlayerLevel / 3) + 1); // Lvl 1-2 → tier 1, Lvl 3-5 → tier 2, etc.

        // 50/50 chance for weapon or armor
        bool giveWeapon = _random.Next(2) == 0;

        string rewardMsg;
        if (giveWeapon)
        {
            var weapon = Weapon.AllWeapons[tierIndex];
            PlayerInventory.EquippedWeapon = weapon;
            rewardMsg = $"⚔️  ARTIFACT: {weapon.Name} (+{weapon.BonusStrength} STR)!";
        }
        else
        {
            var armor = Armor.AllArmor[tierIndex];
            PlayerInventory.EquippedArmor = armor;
            rewardMsg = $"🛡️  ARTIFACT: {armor.Name} (+{armor.BonusDefense} DEF)!";
        }

        return rewardMsg;
    }

    public string UseDungeonPortal()
    {
        if (!ArtifactCollected)
        {
            return "The portal is sealed! Find the artifact first.";
        }

        // DUNGEON COMPLETED! Award bonuses
        DungeonsCompleted++;

        // Completion bonuses scale with dungeon depth
        int goldBonus = MaxDungeonDepth * 50;
        int xpBonus = MaxDungeonDepth * 30;

        PlayerGold += goldBonus;
        PlayerXP += xpBonus;

        // Check for level up
        ProcessLevelUp();

        // Exit dungeon
        ExitDungeon();

        // Check if player won the game!
        CheckVictoryCondition();

        return $"✅ DUNGEON CLEARED! +{goldBonus}g, +{xpBonus}XP | Total: {DungeonsCompleted} completed";
    }

    public void CheckVictoryCondition()
    {
        // Victory condition: reach target level, gold, and dungeons
        // These scale with player progression!
        int targetLevel = 5 + (PlayerLevel / 4);
        int targetGold = 100 * Math.Max(1, PlayerLevel / 2);
        int targetDungeons = 2 + (PlayerLevel / 5);

        bool levelGoal = PlayerLevel >= targetLevel;
        bool goldGoal = PlayerGold >= targetGold;
        bool dungeonGoal = DungeonsCompleted >= targetDungeons;

        if (levelGoal && goldGoal && dungeonGoal && !RunWon)
        {
            RunWon = true;
        }
    }

    public string GetVictoryProgress()
    {
        int targetLevel = 5 + (PlayerLevel / 4);
        int targetGold = 100 * Math.Max(1, PlayerLevel / 2);
        int targetDungeons = 2 + (PlayerLevel / 5);

        return $"Goals: Lvl {PlayerLevel}/{targetLevel} | {PlayerGold}/{targetGold}g | {DungeonsCompleted}/{targetDungeons} dungeons";
    }

    // NEW: Death & Respawn System
    public void HandlePlayerDeath()
    {
        TotalDeaths++;

        // Mark death location
        DeathLocationX = PlayerX;
        DeathLocationY = PlayerY;

        // Drop all equipment at death location
        if (PlayerInventory.EquippedWeapon.BonusStrength > 0)
        {
            _droppedWeapon = PlayerInventory.EquippedWeapon;
        }
        else
        {
            _droppedWeapon = null;
        }

        if (PlayerInventory.EquippedArmor.BonusDefense > 0)
        {
            _droppedArmor = PlayerInventory.EquippedArmor;
        }
        else
        {
            _droppedArmor = null;
        }

        // Reset equipment to rags (starter gear)
        PlayerInventory.EquippedWeapon = Weapon.AllWeapons[0]; // Rusty Dagger
        PlayerInventory.EquippedArmor = Armor.AllArmor[0]; // Cloth Rags

        // Lose some gold (50% penalty)
        int goldLost = PlayerGold / 2;
        PlayerGold -= goldLost;

        // Respawn at Temple (10, 10)
        PlayerX = 10;
        PlayerY = 10;
        PlayerHP = MaxPlayerHP; // Full heal on respawn

        // Exit dungeon if we were in one
        if (InDungeon)
        {
            ExitDungeon();
        }

        // Reset combat state
        IsWon = false;
        CombatEnded = false;
        RunEnded = false;
    }

    public bool CanRetrieveDroppedItems()
    {
        return DeathLocationX.HasValue &&
               DeathLocationX == PlayerX &&
               DeathLocationY == PlayerY &&
               (_droppedWeapon != null || _droppedArmor != null);
    }

    public string RetrieveDroppedItems()
    {
        if (!CanRetrieveDroppedItems())
            return "No items here.";

        var retrieved = new List<string>();

        if (_droppedWeapon != null)
        {
            PlayerInventory.EquippedWeapon = _droppedWeapon;
            retrieved.Add(_droppedWeapon.Name);
            _droppedWeapon = null;
        }

        if (_droppedArmor != null)
        {
            PlayerInventory.EquippedArmor = _droppedArmor;
            retrieved.Add(_droppedArmor.Name);
            _droppedArmor = null;
        }

        DeathLocationX = null;
        DeathLocationY = null;

        return "Retrieved: " + string.Join(", ", retrieved) + "!";
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
        int damage = _random.Next(1, 4); // REDUCED: 1-3 damage (was 1-5!)
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

            // Random enemy type and derive name from it
            EnemyType type = (EnemyType)_random.Next(3);
            string name = type switch
            {
                EnemyType.GoblinScout => "Goblin Scout",
                EnemyType.GoblinWarrior => "Goblin Warrior",
                EnemyType.GoblinArcher => "Goblin Archer",
                _ => "Goblin Scout"
            };

            int level = Math.Max(1, PlayerLevel + _random.Next(-1, 2)); // Level ±1 of player

            _activeMobs.Add(new Mob(x, y, name, level, type));
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

    public List<Mob> GetAllWorldMobs()
    {
        return _activeMobs;
    }

    // NEW: Active World - Mobs move toward player
    public void UpdateWorldMobs()
    {
        foreach (var mob in _activeMobs.ToList()) // ToList to avoid modification during iteration
        {
            // Calculate distance to player
            int dx = PlayerX - mob.X;
            int dy = PlayerY - mob.Y;
            int distance = Math.Abs(dx) + Math.Abs(dy); // Manhattan distance

            // Only move if within detection range
            if (distance <= _mobDetectionRange && distance > 0)
            {
                // Simple chase AI: Move one step toward player
                int newX = mob.X;
                int newY = mob.Y;

                // Prioritize horizontal or vertical based on distance
                if (Math.Abs(dx) > Math.Abs(dy))
                {
                    // Move horizontally toward player
                    newX += dx > 0 ? 1 : -1;
                }
                else if (Math.Abs(dy) > 0)
                {
                    // Move vertically toward player
                    newY += dy > 0 ? 1 : -1;
                }

                // Check bounds
                if (newX >= 0 && newX < WorldWidth && newY >= 0 && newY < WorldHeight)
                {
                    // Check if another mob is already at this position
                    if (!IsMobAt(newX, newY))
                    {
                        mob.X = newX;
                        mob.Y = newY;
                    }
                }
            }
        }
    }

    public void TriggerMobEncounter(Mob mob)
    {
        // CRITICAL: Set combat flags!
        _combatStarted = true;
        _hpCombat = true;

        // Initialize combat with the mob using its stored type
        InitializeEnemyWithVariableStats(mob.Type);

        // Override with mob's name and level (already set by InitializeEnemyWithVariableStats, but ensure level is correct)
        EnemyName = mob.Name;
        EnemyLevel = mob.Level;

        // Scale mob stats based on level
        int baseHP = EnemyHP;
        int baseDamage = EnemyDamage;

        EnemyHP = baseHP + ((mob.Level - 1) * 2);
        EnemyDamage = Math.Max(1, baseDamage + ((mob.Level - 1) / 2));

        CombatEnded = false;
        IsWon = false;
        PlayerStamina = 999; // Infinite stamina for encounters
        CombatLog = $"Encountered {mob.Name} [Level {mob.Level}]!";
    }

    // ===== GENERATION 31: MOB AI, FOG OF WAR, DUNGEON MAPS =====

    public void TickWorld()
    {
        // Mobs AI: move toward player if in detection range
        foreach (var mob in _activeMobs.ToList()) // ToList to avoid modification during iteration
        {
            int distance = Math.Abs(mob.X - PlayerX) + Math.Abs(mob.Y - PlayerY);

            if (distance <= _mobDetectionRange && distance > 0)
            {
                // Move toward player
                int dx = 0, dy = 0;

                if (mob.X < PlayerX) dx = 1;
                else if (mob.X > PlayerX) dx = -1;

                if (mob.Y < PlayerY) dy = 1;
                else if (mob.Y > PlayerY) dy = -1;

                // Try to move (prefer moving in one direction)
                int newX = mob.X + dx;
                int newY = mob.Y;

                // Check if target position is valid and empty
                if (newX >= 0 && newX < WorldWidth && newY >= 0 && newY < WorldHeight &&
                    !IsMobAt(newX, newY))
                {
                    mob.X = newX;
                }
                else
                {
                    // Try vertical movement instead
                    newX = mob.X;
                    newY = mob.Y + dy;

                    if (newX >= 0 && newX < WorldWidth && newY >= 0 && newY < WorldHeight &&
                        !IsMobAt(newX, newY))
                    {
                        mob.Y = newY;
                    }
                }
            }

            // Random despawn chance (2% per tick) - mobs can wander off
            if (_random.Next(100) < 2)
            {
                _activeMobs.Remove(mob);
            }
        }

        // Game of Life style population control
        ApplyMobPopulationRules();
    }

    private void ApplyMobPopulationRules()
    {
        // Check each mob for overpopulation/underpopulation
        var mobsToRemove = new List<Mob>();
        var spawnLocations = new List<(int x, int y)>();

        foreach (var mob in _activeMobs.ToList())
        {
            int neighborsCount = CountNearbyMobs(mob.X, mob.Y, 4); // 4-tile radius

            // Game of Life rules adapted for mobs:
            // Overpopulation: 4+ neighbors = 30% chance to despawn (flee crowding)
            if (neighborsCount >= 4 && _random.Next(100) < 30)
            {
                mobsToRemove.Add(mob);
            }
            // Isolation: 0 neighbors = 10% chance to despawn (no pack support)
            else if (neighborsCount == 0 && _random.Next(100) < 10)
            {
                mobsToRemove.Add(mob);
            }
            // Reproduction: 2-3 neighbors = ideal, 5% chance to spawn nearby
            else if (neighborsCount >= 2 && neighborsCount <= 3 && _random.Next(100) < 5)
            {
                // Try to spawn near this mob
                int spawnX = mob.X + _random.Next(-2, 3);
                int spawnY = mob.Y + _random.Next(-2, 3);

                if (spawnX >= 0 && spawnX < WorldWidth && spawnY >= 0 && spawnY < WorldHeight &&
                    !IsMobAt(spawnX, spawnY) && GetTerrainAt(spawnX, spawnY) != "Town")
                {
                    spawnLocations.Add((spawnX, spawnY));
                }
            }
        }

        // Apply removals
        foreach (var mob in mobsToRemove)
        {
            _activeMobs.Remove(mob);
        }

        // Apply spawns (respect population limits)
        foreach (var (x, y) in spawnLocations)
        {
            if (_activeMobs.Count < _maxMobsInWorld)
            {
                EnemyType type = (EnemyType)_random.Next(3);
                string name = type switch
                {
                    EnemyType.GoblinScout => "Goblin Scout",
                    EnemyType.GoblinWarrior => "Goblin Warrior",
                    EnemyType.GoblinArcher => "Goblin Archer",
                    _ => "Goblin Scout"
                };
                int level = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
                _activeMobs.Add(new Mob(x, y, name, level, type));
            }
        }

        // Minimum population maintenance
        if (_activeMobs.Count < MinMobsInWorld)
        {
            // Spawn a new mob somewhere random
            int x, y;
            string terrain;
            do
            {
                x = _random.Next(WorldWidth);
                y = _random.Next(WorldHeight);
                terrain = _worldMap[x, y];
            }
            while ((x == PlayerX && y == PlayerY) || terrain == "Town" || IsMobAt(x, y));

            EnemyType type = (EnemyType)_random.Next(3);
            string name = type switch
            {
                EnemyType.GoblinScout => "Goblin Scout",
                EnemyType.GoblinWarrior => "Goblin Warrior",
                EnemyType.GoblinArcher => "Goblin Archer",
                _ => "Goblin Scout"
            };
            int level = Math.Max(1, PlayerLevel + _random.Next(-1, 2));
            _activeMobs.Add(new Mob(x, y, name, level, type));
        }
    }

    private int CountNearbyMobs(int centerX, int centerY, int radius)
    {
        int count = 0;
        foreach (var mob in _activeMobs)
        {
            if (mob.X == centerX && mob.Y == centerY) continue; // Don't count self

            int distance = Math.Abs(mob.X - centerX) + Math.Abs(mob.Y - centerY);
            if (distance <= radius)
            {
                count++;
            }
        }
        return count;
    }

    public bool IsTileExplored(int x, int y)
    {
        if (x < 0 || x >= WorldWidth || y < 0 || y >= WorldHeight)
            return false;

        if (_exploredTiles == null)
            return false;

        return _exploredTiles[x, y];
    }

    public bool HasDungeonMap()
    {
        return _dungeonMap != null;
    }

    public string GetDungeonTile(int x, int y)
    {
        if (!InDungeon || _dungeonMap == null)
            return "None";

        if (x < 0 || x >= _dungeonWidth || y < 0 || y >= _dungeonHeight)
            return "OutOfBounds";

        return _dungeonMap[x, y];
    }


    // Testing helper methods
    public void SetTerrainForTesting(int x, int y, string terrain)
    {
        if (x >= 0 && x < WorldWidth && y >= 0 && y < WorldHeight)
        {
            _worldMap[x, y] = terrain;
        }
    }

    public void AddMobForTesting(Mob mob)
    {
        _activeMobs.Add(mob);
    }

    public Mob? GetNearestMob()
    {
        if (_activeMobs.Count == 0) return null;

        Mob? nearest = null;
        int minDistance = int.MaxValue;

        foreach (var mob in _activeMobs)
        {
            int distance = Math.Abs(mob.X - PlayerX) + Math.Abs(mob.Y - PlayerY);
            if (distance < minDistance)
            {
                minDistance = distance;
                nearest = mob;
            }
        }

        return nearest;
    }

    public void SpawnMobForTesting()
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
        while ((x == PlayerX && y == PlayerY) || terrain == "Town" || terrain == "Temple" || IsMobAt(x, y));

        EnemyType type = (EnemyType)_random.Next(3);
        string name = type switch
        {
            EnemyType.GoblinScout => "Goblin Scout",
            EnemyType.GoblinWarrior => "Goblin Warrior",
            EnemyType.GoblinArcher => "Goblin Archer",
            _ => "Goblin Scout"
        };
        int level = Math.Max(1, PlayerLevel + _random.Next(-1, 2));

        _activeMobs.Add(new Mob(x, y, name, level, type));
    }

    public void SetDungeonTileForTesting(int x, int y, string tile)
    {
        if (_dungeonMap != null && x >= 0 && x < _dungeonWidth && y >= 0 && y < _dungeonHeight)
        {
            _dungeonMap[x, y] = tile;
        }
    }
}
