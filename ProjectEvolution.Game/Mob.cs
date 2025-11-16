namespace ProjectEvolution.Game;

public class Mob
{
    public int X { get; set; }
    public int Y { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }

    public Mob(int x, int y, string name, int level)
    {
        X = x;
        Y = y;
        Name = name;
        Level = level;
    }
}
