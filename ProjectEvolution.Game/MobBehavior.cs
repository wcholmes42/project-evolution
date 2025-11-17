namespace ProjectEvolution.Game;

public enum MobBehavior
{
    Chase,      // Actively hunt player (current behavior)
    Patrol,     // Follow predetermined path
    Wander,     // Random movement
    Ambush,     // Stay hidden in forests/mountains until player nearby
    Guard       // Stay near specific location
}
