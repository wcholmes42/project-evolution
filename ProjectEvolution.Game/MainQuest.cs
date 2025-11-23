namespace ProjectEvolution.Game;

public enum GameEnding
{
    None,
    HeroicVictory,      // Avatar path, defeated demon lord with all virtues
    VirtuousVictory,    // Good reputation, most virtues high
    PragmaticVictory,   // Neutral, focused on power
    DarkVictory,        // Evil path, became powerful through corruption
    Defeat              // Player died before completing main quest
}

public class MainQuestline
{
    public bool IsStarted { get; set; } = false;
    public bool IsCompleted { get; set; } = false;
    public GameEnding Ending { get; set; } = GameEnding.None;
    public int DemonLordsDefeated { get; set; } = 0;
    public int ArtifactsCollected { get; set; } = 0;

    // Main quest stages
    public bool Stage1_LearnedAboutDemonLord { get; set; } = false;
    public bool Stage2_CollectedAllArtifacts { get; set; } = false;
    public bool Stage3_UnlockedFinalDungeon { get; set; } = false;
    public bool Stage4_DefeatedDemonLord { get; set; } = false;

    public void Start()
    {
        IsStarted = true;
        Stage1_LearnedAboutDemonLord = true;
    }

    public void CollectArtifact()
    {
        ArtifactsCollected++;
        if (ArtifactsCollected >= 3)
        {
            Stage2_CollectedAllArtifacts = true;
            Stage3_UnlockedFinalDungeon = true;
        }
    }

    public void DefeatDemonLord()
    {
        DemonLordsDefeated++;
        Stage4_DefeatedDemonLord = true;
    }

    public GameEnding DetermineEnding(VirtueSystem virtues, ReputationSystem reputation)
    {
        if (!Stage4_DefeatedDemonLord)
        {
            return GameEnding.Defeat;
        }

        // Avatar ending - perfect virtue path
        if (virtues.IsExemplarInAll())
        {
            return GameEnding.HeroicVictory;
        }

        // Check reputation-based endings
        int totalVirtue = virtues.GetTotalVirtueScore();

        if (reputation.ReputationScore >= 40 && totalVirtue >= 240)
        {
            return GameEnding.VirtuousVictory;
        }
        else if (reputation.ReputationScore <= -40 && totalVirtue <= 160)
        {
            return GameEnding.DarkVictory;
        }
        else
        {
            return GameEnding.PragmaticVictory;
        }
    }

    public void Complete(GameEnding ending)
    {
        IsCompleted = true;
        Ending = ending;
    }

    public string GetEndingDescription(GameEnding ending)
    {
        return ending switch
        {
            GameEnding.HeroicVictory =>
                "THE AVATAR ENDING\n\n" +
                "You have mastered all four virtues and defeated the demon lord with pure heart.\n" +
                "The realm celebrates you as a living legend. Your name will echo through eternity.\n" +
                "True heroes are not born - they are forged through trials, compassion, and unwavering honor.\n\n" +
                "The demon lord's essence is purified. Peace returns to the land.",

            GameEnding.VirtuousVictory =>
                "THE HERO'S ENDING\n\n" +
                "Through courage and virtue, you vanquished the demon lord.\n" +
                "Though your path was not perfect, your heart was true.\n" +
                "The people sing songs of your deeds. You have earned your place in history.\n\n" +
                "The darkness recedes, but your journey continues...",

            GameEnding.PragmaticVictory =>
                "THE SURVIVOR'S ENDING\n\n" +
                "You defeated the demon lord through strength and cunning.\n" +
                "The realm is saved, though the cost was high.\n" +
                "Some call you hero. Others call you mercenary. You simply call yourself... alive.\n\n" +
                "The demon lord falls, but you wonder if you've changed more than you realize.",

            GameEnding.DarkVictory =>
                "THE DARK LORD ENDING\n\n" +
                "You defeated the demon lord... but at what cost?\n" +
                "In your quest for power, you became what you sought to destroy.\n" +
                "The realm is 'saved', but now it fears a new tyrant: YOU.\n\n" +
                "As the demon lord dies, he smiles. 'You'll make a fine replacement,' he whispers.",

            GameEnding.Defeat =>
                "DEFEATED\n\n" +
                "Your journey ends here. The demon lord's corruption spreads unchecked.\n" +
                "But heroes are not defined by their victories - only by their willingness to stand.\n" +
                "Perhaps another will rise where you fell...",

            _ => "Unknown ending..."
        };
    }

    public int GetCompletionPercentage()
    {
        int stages = 0;
        if (Stage1_LearnedAboutDemonLord) stages++;
        if (Stage2_CollectedAllArtifacts) stages++;
        if (Stage3_UnlockedFinalDungeon) stages++;
        if (Stage4_DefeatedDemonLord) stages++;
        return (stages * 100) / 4;
    }
}

// The final boss
public class DemonLord
{
    public string Name => "Azathor the Eternal";
    public int HP { get; set; }
    public int MaxHP { get; set; } = 100;
    public int Damage { get; set; } = 5;
    public int Level { get; set; } = 20;

    // Boss phases
    public int CurrentPhase { get; set; } = 1; // Phases 1-3
    public bool CanCastSpells => CurrentPhase >= 2;
    public bool HasMinions => CurrentPhase >= 3;

    public DemonLord()
    {
        HP = MaxHP;
    }

    public void TakeDamage(int damage)
    {
        HP = Math.Max(0, HP - damage);

        // Phase transitions
        if (HP <= 66 && CurrentPhase == 1)
        {
            CurrentPhase = 2;
        }
        else if (HP <= 33 && CurrentPhase == 2)
        {
            CurrentPhase = 3;
        }
    }

    public string GetPhaseDescription()
    {
        return CurrentPhase switch
        {
            1 => "The demon lord towers before you, wreathed in shadow.",
            2 => "Wounded, the demon lord begins channeling dark magic!",
            3 => "In desperation, the demon lord summons his minions!",
            _ => "The demon lord has fallen."
        };
    }
}
