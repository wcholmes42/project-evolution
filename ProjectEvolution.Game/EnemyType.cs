namespace ProjectEvolution.Game;

public enum EnemyType
{
    // GOBLIN FAMILY - Basic enemies
    GoblinScout,    // 2 HP, 1 damage - fast and weak
    GoblinWarrior,  // 5 HP, 1 damage - tank
    GoblinArcher,   // 3 HP, 2 damage - glass cannon

    // UNDEAD FAMILY - Regeneration & debuffs
    Skeleton,       // 4 HP, 1 damage - regenerates 1 HP/turn
    Zombie,         // 6 HP, 1 damage - inflicts poison (1 dmg/turn for 3 turns)
    Wraith,         // 3 HP, 2 damage - life drain (+1 HP on hit)

    // BEAST FAMILY - Pack tactics & counters
    Wolf,           // 4 HP, 1 damage - pack bonus (+1 dmg if ally nearby)
    Bear,           // 8 HP, 2 damage - counter-attacks when hit
    Serpent,        // 3 HP, 1 damage - poison bite (2 dmg/turn for 2 turns), high evasion

    // DEMON FAMILY - Fire & chaos
    Imp,            // 3 HP, 1 damage - teleports (can't flee easily)
    Hellhound,      // 5 HP, 2 damage - burning DoT (1 dmg/turn for 4 turns)
    Demon           // 7 HP, 3 damage - can cast skills!
}

public enum EnemyAbility
{
    None,
    Regeneration,   // Heal 1 HP per turn
    Poison,         // Inflict poison DoT
    LifeDrain,      // Heal when dealing damage
    PackTactics,    // Bonus damage with allies
    CounterAttack,  // Hit back when damaged
    HighEvasion,    // Increased miss chance for player
    Teleport,       // Harder to flee
    Burning,        // Fire DoT
    CastSkills      // Can use player skills
}
