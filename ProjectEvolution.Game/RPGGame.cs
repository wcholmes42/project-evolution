namespace ProjectEvolution.Game;

public class RPGGame
{
    private static readonly Random _random = new Random();
    private bool _combatStarted;
    private bool _aiCombat;

    public bool IsWon { get; private set; }
    public string CombatLog { get; private set; } = string.Empty;

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
        _aiCombat = false;
        IsWon = false;
    }

    public void ChooseAction(CombatAction action)
    {
        if (!_combatStarted)
        {
            throw new InvalidOperationException("Combat has not been started");
        }

        // Simple combat (Gen 2): Attack wins, Defend loses
        IsWon = action == CombatAction.Attack;
    }

    public void StartCombatWithAI()
    {
        _combatStarted = true;
        _aiCombat = true;
        IsWon = false;
        CombatLog = string.Empty;
    }

    public void ChooseActionAgainstEnemy(CombatAction playerAction, CombatAction enemyAction, bool? coinFlipPlayerWins = null)
    {
        if (!_combatStarted || !_aiCombat)
        {
            throw new InvalidOperationException("AI combat has not been started");
        }

        IsWon = ResolveCombat(playerAction, enemyAction, coinFlipPlayerWins);
    }

    public void ChooseActionWithRandomEnemy(CombatAction playerAction)
    {
        if (!_combatStarted || !_aiCombat)
        {
            throw new InvalidOperationException("AI combat has not been started");
        }

        CombatAction enemyAction = _random.Next(2) == 0 ? CombatAction.Attack : CombatAction.Defend;
        IsWon = ResolveCombat(playerAction, enemyAction, null);
    }

    private bool ResolveCombat(CombatAction playerAction, CombatAction enemyAction, bool? coinFlipPlayerWins)
    {
        // Rock-paper-scissors style combat
        if (playerAction == CombatAction.Attack && enemyAction == CombatAction.Defend)
        {
            CombatLog = "You attack! The goblin's defense crumbles!";
            return true; // Player wins
        }
        else if (playerAction == CombatAction.Defend && enemyAction == CombatAction.Attack)
        {
            CombatLog = "The goblin attacks! Your defense holds but you don't defeat it!";
            return false; // Enemy wins
        }
        else if (playerAction == CombatAction.Defend && enemyAction == CombatAction.Defend)
        {
            CombatLog = "Both fighters defend cautiously... stalemate!";
            return false; // Draw - player didn't defeat enemy
        }
        else // Both attack
        {
            bool playerWins = coinFlipPlayerWins ?? _random.Next(2) == 0;
            CombatLog = playerWins
                ? "Both strike at once! Your blade lands first!"
                : "Both strike at once! The goblin's attack overwhelms you!";
            return playerWins;
        }
    }
}
