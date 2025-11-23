namespace ProjectEvolution.Game;

public enum SecretType
{
    HiddenDungeon,      // Secret entrance to bonus dungeon
    RareArtifact,       // Legendary item
    AncientShrine,      // Permanent stat boost
    MerchantCaravan,    // Rare items for sale
    WorldBoss           // Optional super-boss
}

public class Secret
{
    public SecretType Type { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsDiscovered { get; set; } = false;
    public string Name { get; set; }
    public string Description { get; set; }

    public Secret(SecretType type, string name, string description, int x, int y)
    {
        Type = type;
        Name = name;
        Description = description;
        X = x;
        Y = y;
    }

    public void Discover()
    {
        IsDiscovered = true;
    }
}

public class WorldSecrets
{
    private List<Secret> _secrets = new List<Secret>();
    public int TotalSecrets => _secrets.Count;
    public int DiscoveredSecrets => _secrets.Count(s => s.IsDiscovered);

    public WorldSecrets()
    {
        GenerateSecrets();
    }

    private void GenerateSecrets()
    {
        // Hidden dungeon
        _secrets.Add(new Secret(
            SecretType.HiddenDungeon,
            "The Forgotten Crypt",
            "An ancient tomb hidden beneath the mountains...",
            2, 18));

        // Ancient shrines
        _secrets.Add(new Secret(
            SecretType.AncientShrine,
            "Shrine of Strength",
            "A sacred altar that grants permanent power.",
            17, 3));

        _secrets.Add(new Secret(
            SecretType.AncientShrine,
            "Shrine of Defense",
            "A blessed monument that fortifies the soul.",
            3, 17));

        // Rare artifact
        _secrets.Add(new Secret(
            SecretType.RareArtifact,
            "Crown of the Ancients",
            "A legendary artifact of immense power.",
            18, 18));

        // Merchant caravan (moves around)
        _secrets.Add(new Secret(
            SecretType.MerchantCaravan,
            "Wandering Merchant",
            "A mysterious trader with exotic wares.",
            8, 12));

        // World boss
        _secrets.Add(new Secret(
            SecretType.WorldBoss,
            "The Ancient Dragon",
            "A primordial beast that predates civilization.",
            1, 1));
    }

    public Secret? GetSecretAt(int x, int y)
    {
        return _secrets.FirstOrDefault(s => s.X == x && s.Y == y && !s.IsDiscovered);
    }

    public List<Secret> GetAllSecrets()
    {
        return _secrets.ToList();
    }

    public List<Secret> GetDiscoveredSecrets()
    {
        return _secrets.Where(s => s.IsDiscovered).ToList();
    }

    public void DiscoverSecret(int x, int y)
    {
        var secret = GetSecretAt(x, y);
        secret?.Discover();
    }

    public int GetCompletionPercentage()
    {
        if (TotalSecrets == 0) return 0;
        return (DiscoveredSecrets * 100) / TotalSecrets;
    }
}

// Rare encounters
public class RareEncounter
{
    public string Name { get; set; }
    public string Description { get; set; }
    public double SpawnChance { get; set; } // 0.0 to 1.0
    public EnemyType? BossType { get; set; }
    public int BossLevel { get; set; }

    public RareEncounter(string name, string desc, double chance, EnemyType? bossType = null, int level = 10)
    {
        Name = name;
        Description = desc;
        SpawnChance = chance;
        BossType = bossType;
        BossLevel = level;
    }

    // Predefined rare encounters
    public static readonly RareEncounter GoblinKing = new RareEncounter(
        "Goblin King",
        "The ruler of all goblin tribes appears!",
        0.01, // 1% chance
        EnemyType.GoblinWarrior,
        15);

    public static readonly RareEncounter LichLord = new RareEncounter(
        "Lich Lord",
        "An ancient undead sorcerer rises from his tomb!",
        0.005, // 0.5% chance
        EnemyType.Wraith,
        18);

    public static readonly RareEncounter AlphaBeast = new RareEncounter(
        "Alpha Dire Wolf",
        "The pack leader emerges from the shadows!",
        0.01,
        EnemyType.Wolf,
        12);

    public static RareEncounter? RollForRareEncounter(Random random)
    {
        double roll = random.NextDouble();

        if (roll < GoblinKing.SpawnChance) return GoblinKing;
        if (roll < GoblinKing.SpawnChance + LichLord.SpawnChance) return LichLord;
        if (roll < GoblinKing.SpawnChance + LichLord.SpawnChance + AlphaBeast.SpawnChance) return AlphaBeast;

        return null;
    }
}
