namespace ProjectEvolution.Game;

public class Weapon
{
    public string Name { get; set; }
    public int BonusStrength { get; set; }
    public int Value { get; set; }

    public Weapon(string name, int bonusStr, int value)
    {
        Name = name;
        BonusStrength = bonusStr;
        Value = value;
    }

    public static Weapon[] AllWeapons = new[]
    {
        new Weapon("Rusty Dagger", 0, 5),
        new Weapon("Iron Sword", 1, 25),
        new Weapon("Steel Blade", 2, 50),
        new Weapon("Silver Sword", 3, 100),
        new Weapon("Enchanted Blade", 5, 250)
    };
}

public class Armor
{
    public string Name { get; set; }
    public int BonusDefense { get; set; }
    public int Value { get; set; }

    public Armor(string name, int bonusDef, int value)
    {
        Name = name;
        BonusDefense = bonusDef;
        Value = value;
    }

    public static Armor[] AllArmor = new[]
    {
        new Armor("Cloth Rags", 0, 5),
        new Armor("Leather Armor", 1, 25),
        new Armor("Chain Mail", 2, 50),
        new Armor("Plate Armor", 3, 100),
        new Armor("Dragon Scale", 5, 250)
    };
}

public class Inventory
{
    public Weapon EquippedWeapon { get; set; }
    public Armor EquippedArmor { get; set; }
    public List<Weapon> Weapons { get; set; } = new List<Weapon>();
    public List<Armor> Armors { get; set; } = new List<Armor>();

    public Inventory()
    {
        EquippedWeapon = Weapon.AllWeapons[0]; // Start with rusty dagger
        EquippedArmor = Armor.AllArmor[0]; // Start with cloth rags
    }

    public int GetTotalStrength(int baseStr)
    {
        return baseStr + EquippedWeapon.BonusStrength;
    }

    public int GetTotalDefense(int baseDef)
    {
        return baseDef + EquippedArmor.BonusDefense;
    }
}
