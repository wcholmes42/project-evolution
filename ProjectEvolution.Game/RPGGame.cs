namespace ProjectEvolution.Game;

public class RPGGame
{
    private static readonly Random _random = new Random();
    private bool _combatStarted;

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

    public void StartCombat()
    {
        _combatStarted = true;
        IsWon = false; // Combat is ongoing, not won yet
    }

    public void ChooseAction(CombatAction action)
    {
        if (!_combatStarted)
        {
            throw new InvalidOperationException("Combat has not been started");
        }

        // Simple combat: Attack wins, Defend loses
        IsWon = action == CombatAction.Attack;
    }
}
