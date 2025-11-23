namespace ProjectEvolution.Game;

public enum VirtueType
{
    Valor,      // Bravery in combat (fight vs flee)
    Honor,      // Fair play (mercy vs cruelty)
    Compassion, // Helping others (charity, healing)
    Honesty     // Truth (dialogue choices, quest outcomes)
}

public class VirtueScore
{
    public VirtueType Type { get; set; }
    public int Score { get; set; } = 50; // 0-100, starts neutral
    public string Name => Type.ToString();

    public VirtueScore(VirtueType type)
    {
        Type = type;
    }

    public void Adjust(int amount)
    {
        Score = Math.Clamp(Score + amount, 0, 100);
    }

    public string GetLevel()
    {
        return Score switch
        {
            >= 80 => "Exemplar",
            >= 60 => "Virtuous",
            >= 40 => "Average",
            >= 20 => "Lacking",
            _ => "Corrupt"
        };
    }

    public bool IsExemplar() => Score >= 80;
    public bool IsVirtuous() => Score >= 60;
}

public class VirtueSystem
{
    public Dictionary<VirtueType, VirtueScore> Virtues { get; private set; }

    public VirtueSystem()
    {
        Virtues = new Dictionary<VirtueType, VirtueScore>
        {
            { VirtueType.Valor, new VirtueScore(VirtueType.Valor) },
            { VirtueType.Honor, new VirtueScore(VirtueType.Honor) },
            { VirtueType.Compassion, new VirtueScore(VirtueType.Compassion) },
            { VirtueType.Honesty, new VirtueScore(VirtueType.Honesty) }
        };
    }

    public void AdjustVirtue(VirtueType type, int amount)
    {
        if (Virtues.ContainsKey(type))
        {
            Virtues[type].Adjust(amount);
        }
    }

    public int GetVirtueScore(VirtueType type)
    {
        return Virtues.ContainsKey(type) ? Virtues[type].Score : 0;
    }

    public bool IsExemplarInAll()
    {
        return Virtues.Values.All(v => v.IsExemplar());
    }

    public int GetTotalVirtueScore()
    {
        return Virtues.Values.Sum(v => v.Score);
    }

    public string GetVirtuePath()
    {
        int total = GetTotalVirtueScore();
        return total switch
        {
            >= 320 => "Avatar", // 80+ in all 4 virtues
            >= 280 => "Paragon",
            >= 240 => "Virtuous",
            >= 200 => "Balanced",
            >= 160 => "Flawed",
            >= 120 => "Corrupt",
            _ => "Fallen"
        };
    }

    // Check if player qualifies for virtue-based unlocks
    public bool CanUseVirtueAbility(VirtueType requiredVirtue, int minScore = 80)
    {
        return GetVirtueScore(requiredVirtue) >= minScore;
    }
}

// Virtue-based abilities (unlocked by exemplar status)
public class VirtueAbility
{
    public string Name { get; set; }
    public VirtueType RequiredVirtue { get; set; }
    public int MinVirtueScore { get; set; }
    public string Description { get; set; }
    public int StaminaCost { get; set; }

    public VirtueAbility(string name, VirtueType virtue, int minScore, string desc, int stamina)
    {
        Name = name;
        RequiredVirtue = virtue;
        MinVirtueScore = minScore;
        Description = desc;
        StaminaCost = stamina;
    }

    // Predefined virtue abilities
    public static readonly VirtueAbility CourageousStrike = new VirtueAbility(
        "Courageous Strike",
        VirtueType.Valor,
        80,
        "Your valor empowers you - 3x damage but no defense! (8 stamina)",
        8);

    public static readonly VirtueAbility HonorableDuel = new VirtueAbility(
        "Honorable Duel",
        VirtueType.Honor,
        80,
        "Challenge enemy to fair combat - both attacks deal true damage! (6 stamina)",
        6);

    public static readonly VirtueAbility HealingTouch = new VirtueAbility(
        "Healing Touch",
        VirtueType.Compassion,
        80,
        "Your compassion heals - restore 20 HP to self or companion! (10 stamina)",
        10);

    public static readonly VirtueAbility TruthSeeker = new VirtueAbility(
        "Truth Seeker",
        VirtueType.Honesty,
        80,
        "See enemy's true nature - reveals exact HP and abilities! (4 stamina)",
        4);

    public static List<VirtueAbility> GetAvailableAbilities(VirtueSystem virtues)
    {
        var abilities = new List<VirtueAbility>
        {
            CourageousStrike, HonorableDuel, HealingTouch, TruthSeeker
        };

        return abilities.Where(a => virtues.CanUseVirtueAbility(a.RequiredVirtue, a.MinVirtueScore)).ToList();
    }
}
