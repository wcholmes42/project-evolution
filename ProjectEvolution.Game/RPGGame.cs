namespace ProjectEvolution.Game;

public class RPGGame
{
    private static readonly Random _random = new Random();

    public bool IsWon { get; private set; }

    public void Start()
    {
        IsWon = true;
    }

    public void StartWithCoinFlip(bool heads)
    {
        IsWon = heads;
    }

    public void StartWithRandomCoinFlip()
    {
        bool heads = _random.Next(2) == 0;
        StartWithCoinFlip(heads);
    }
}
