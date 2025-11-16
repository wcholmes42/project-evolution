namespace ProjectEvolution.Game;

public class RPGGame
{
    public bool IsWon { get; private set; }

    public void Start()
    {
        IsWon = true;
    }
}
