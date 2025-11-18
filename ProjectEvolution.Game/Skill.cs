namespace ProjectEvolution.Game;

// Skill effect types
public enum SkillEffect
{
    Damage,      // Direct damage multiplier
    Stun,        // Enemy skips turn
    Heal,        // Restore HP
    Buff,        // Apply buff to player
    Debuff       // Apply debuff to player (drawback)
}

// Buff/Debuff types
public enum BuffType
{
    None,
    BerserkerRage,      // +100% damage, +50% damage taken
    DefensiveStance,    // +5 defense
    Stunned             // Skip turn
}

// Active buff on player or enemy
public class ActiveBuff
{
    public BuffType Type { get; set; }
    public int TurnsRemaining { get; set; }
    public int Value { get; set; } // For buffs with numeric values (e.g., +5 defense)

    public ActiveBuff(BuffType type, int turns, int value = 0)
    {
        Type = type;
        TurnsRemaining = turns;
        Value = value;
    }
}

// Core Skill class
public class Skill
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int StaminaCost { get; set; }
    public int MinLevel { get; set; } // Level required to unlock
    public SkillEffect PrimaryEffect { get; set; }
    public double DamageMultiplier { get; set; } // For damage skills
    public int HealAmount { get; set; } // For healing skills
    public BuffType BuffApplied { get; set; } = BuffType.None;
    public int BuffDuration { get; set; } = 0;
    public int BuffValue { get; set; } = 0;
    public bool OncePerCombat { get; set; } = false; // Limited use skills

    public Skill(string name, string desc, int stamina, int minLevel)
    {
        Name = name;
        Description = desc;
        StaminaCost = stamina;
        MinLevel = minLevel;
    }

    // ALL AVAILABLE SKILLS
    public static readonly Skill PowerStrike = new Skill(
        "Power Strike",
        "A devastating blow dealing 1.5x damage (5 stamina)",
        5,
        1 // Available from level 1
    )
    {
        PrimaryEffect = SkillEffect.Damage,
        DamageMultiplier = 1.5
    };

    public static readonly Skill ShieldBash = new Skill(
        "Shield Bash",
        "Bash enemy, 1 turn stun (diminishes if repeated) (4 stam)",
        4,
        3 // Unlocks at level 3
    )
    {
        PrimaryEffect = SkillEffect.Stun,
        DamageMultiplier = 0.7, // Does some damage too
        BuffApplied = BuffType.Stunned,
        BuffDuration = 1
        // Note: Stun resistance builds each time enemy is stunned
    };

    public static readonly Skill SecondWind = new Skill(
        "Second Wind",
        "Catch your breath, restore 10 HP (6 stamina, once per combat)",
        6,
        2 // Unlocks at level 2
    )
    {
        PrimaryEffect = SkillEffect.Heal,
        HealAmount = 10,
        OncePerCombat = true
    };

    public static readonly Skill BerserkerRage = new Skill(
        "Berserker Rage",
        "Enter a rage! 2x damage for 3 turns, but take 50% more damage (7 stamina)",
        7,
        5 // Unlocks at level 5
    )
    {
        PrimaryEffect = SkillEffect.Buff,
        BuffApplied = BuffType.BerserkerRage,
        BuffDuration = 3,
        BuffValue = 100 // +100% damage
    };

    public static readonly Skill DefensiveStance = new Skill(
        "Defensive Stance",
        "Take a defensive posture, +5 defense for 3 turns, can't attack (5 stamina)",
        5,
        4 // Unlocks at level 4
    )
    {
        PrimaryEffect = SkillEffect.Buff,
        BuffApplied = BuffType.DefensiveStance,
        BuffDuration = 3,
        BuffValue = 5 // +5 defense
    };

    // Get all skills available to player at their level
    public static List<Skill> GetAvailableSkills(int playerLevel)
    {
        var allSkills = new List<Skill>
        {
            PowerStrike,
            SecondWind,
            ShieldBash,
            DefensiveStance,
            BerserkerRage
        };

        return allSkills.Where(s => playerLevel >= s.MinLevel).ToList();
    }

    // Get skill by name (for AI/simulation)
    public static Skill? GetSkillByName(string name)
    {
        return name switch
        {
            "Power Strike" => PowerStrike,
            "Shield Bash" => ShieldBash,
            "Second Wind" => SecondWind,
            "Berserker Rage" => BerserkerRage,
            "Defensive Stance" => DefensiveStance,
            _ => null
        };
    }
}
