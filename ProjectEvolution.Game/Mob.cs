namespace ProjectEvolution.Game;

public class Mob
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public EnemyType Type { get; set; }

    public Mob(int x, int y, string name, int level, EnemyType type = EnemyType.GoblinScout)
    {
        X = x;
        Y = y;
        Name = name;
        Level = level;
        Type = type;
    }
}
