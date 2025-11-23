namespace ProjectEvolution.Game;

public enum CompanionClass
{
    Warrior,    // High HP, moderate damage
    Rogue,      // Low HP, high damage, evasion
    Cleric      // Moderate HP, heals party
}

public class Companion
{
    public string Name { get; set; }
    public string Description { get; set; }
    public CompanionClass Class { get; set; }
    public int HP { get; set; }
    public int MaxHP { get; set; }
    public int Strength { get; set; }
    public int Defense { get; set; }
    public int Level { get; set; } = 1;
    public int Loyalty { get; set; } = 50; // 0-100, affects performance
    public bool IsAlive { get; set; } = true;

    // Equipment
    public Weapon EquippedWeapon { get; set; }
    public Armor EquippedArmor { get; set; }

    // Location (for recruitment)
    public int X { get; set; }
    public int Y { get; set; }
    public bool IsRecruited { get; set; } = false;

    // Personal quest
    public string? PersonalQuestId { get; set; }

    public Companion(string name, string description, CompanionClass companionClass, int x, int y)
    {
        Name = name;
        Description = description;
        Class = companionClass;
        X = x;
        Y = y;

        // Class-based starting stats
        switch (companionClass)
        {
            case CompanionClass.Warrior:
                MaxHP = 20;
                HP = 20;
                Strength = 4;
                Defense = 3;
                EquippedWeapon = Weapon.AllWeapons[1]; // Iron Sword
                EquippedArmor = Armor.AllArmor[1]; // Leather Armor
                break;
            case CompanionClass.Rogue:
                MaxHP = 12;
                HP = 12;
                Strength = 6;
                Defense = 1;
                EquippedWeapon = Weapon.AllWeapons[0]; // Rusty Dagger
                EquippedArmor = Armor.AllArmor[0]; // Cloth Rags
                break;
            case CompanionClass.Cleric:
                MaxHP = 15;
                HP = 15;
                Strength = 2;
                Defense = 2;
                EquippedWeapon = Weapon.AllWeapons[0]; // Rusty Dagger
                EquippedArmor = Armor.AllArmor[2]; // Chain Mail
                break;
        }
    }

    public void TakeDamage(int damage)
    {
        HP = Math.Max(0, HP - damage);
        if (HP == 0) IsAlive = false;
    }

    public void Heal(int amount)
    {
        HP = Math.Min(MaxHP, HP + amount);
        if (HP > 0) IsAlive = true;
    }

    public int GetEffectiveStrength()
    {
        return Strength + EquippedWeapon.BonusStrength;
    }

    public int GetEffectiveDefense()
    {
        return Defense + EquippedArmor.BonusDefense;
    }

    public void AdjustLoyalty(int amount)
    {
        Loyalty = Math.Clamp(Loyalty + amount, 0, 100);
    }

    public string GetLoyaltyDescription()
    {
        return Loyalty switch
        {
            >= 80 => "Devoted",
            >= 60 => "Loyal",
            >= 40 => "Neutral",
            >= 20 => "Wavering",
            _ => "Distrustful"
        };
    }

    // ════════════════════════════════════════════════════════════════════
    // PREDEFINED COMPANIONS
    // ════════════════════════════════════════════════════════════════════

    public static Companion CreateThorin()
    {
        var companion = new Companion(
            "Thorin Ironheart",
            "A grizzled dwarf warrior seeking redemption",
            CompanionClass.Warrior,
            6, 6) // Near town 1
        {
            PersonalQuestId = "thorin_redemption"
        };

        return companion;
    }

    public static Companion CreateLyra()
    {
        var companion = new Companion(
            "Lyra Shadowstep",
            "A mysterious rogue with a dark past",
            CompanionClass.Rogue,
            14, 14) // Near town 2
        {
            PersonalQuestId = "lyra_revenge",
            Loyalty = 40 // Starts with lower loyalty
        };

        return companion;
    }

    public static Companion CreateElara()
    {
        var companion = new Companion(
            "Elara Lightbringer",
            "A devoted cleric of the Temple",
            CompanionClass.Cleric,
            10, 11) // At temple
        {
            PersonalQuestId = "elara_pilgrimage",
            Loyalty = 70 // High starting loyalty
        };

        return companion;
    }
}
