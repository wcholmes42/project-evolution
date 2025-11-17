namespace ProjectEvolution.Game;

public class Mob
{
    private static readonly Random _random = new Random();

    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public EnemyType Type { get; set; }
    public MobBehavior Behavior { get; set; }

    // Patrol path (for Patrol behavior)
    public List<(int x, int y)> PatrolPath { get; set; } = new List<(int, int)>();
    public int PatrolIndex { get; set; } = 0;

    // Ambush state (for Ambush behavior)
    public bool IsRevealed { get; set; } = false;

    // Guard anchor point (for Guard behavior)
    public int GuardX { get; set; }
    public int GuardY { get; set; }
    public int GuardRadius { get; set; } = 2;

    public Mob(int x, int y, string name, int level, EnemyType type = EnemyType.GoblinScout, MobBehavior behavior = MobBehavior.Chase)
    {
        X = x;
        Y = y;
        Name = name;
        Level = level;
        Type = type;
        Behavior = behavior;
        GuardX = x;
        GuardY = y;
    }

    public void AssignRandomBehavior()
    {
        // 40% Chase, 30% Wander, 20% Ambush, 10% Patrol
        int roll = _random.Next(100);
        if (roll < 40) Behavior = MobBehavior.Chase;
        else if (roll < 70) Behavior = MobBehavior.Wander;
        else if (roll < 90) Behavior = MobBehavior.Ambush;
        else Behavior = MobBehavior.Patrol;
    }

    public void GeneratePatrolPath(int worldWidth, int worldHeight)
    {
        // Generate simple square patrol path around spawn point
        int radius = 3;
        PatrolPath.Clear();
        PatrolPath.Add((X, Y));
        PatrolPath.Add((Math.Min(X + radius, worldWidth - 1), Y));
        PatrolPath.Add((Math.Min(X + radius, worldWidth - 1), Math.Min(Y + radius, worldHeight - 1)));
        PatrolPath.Add((X, Math.Min(Y + radius, worldHeight - 1)));
        PatrolPath.Add((X, Y));
    }
}
