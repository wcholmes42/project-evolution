namespace ProjectEvolution.Game;

public enum ReputationLevel
{
    Evil,       // -100 to -50
    Bad,        // -49 to -20
    Neutral,    // -19 to +19
    Good,       // +20 to +49
    Hero        // +50 to +100
}

public class ReputationSystem
{
    public int ReputationScore { get; private set; } = 0; // -100 to +100

    public ReputationLevel GetLevel()
    {
        return ReputationScore switch
        {
            >= 50 => ReputationLevel.Hero,
            >= 20 => ReputationLevel.Good,
            >= -19 => ReputationLevel.Neutral,
            >= -49 => ReputationLevel.Bad,
            _ => ReputationLevel.Evil
        };
    }

    public void AdjustReputation(int amount)
    {
        ReputationScore = Math.Clamp(ReputationScore + amount, -100, 100);
    }

    public string GetReputationDescription()
    {
        return GetLevel() switch
        {
            ReputationLevel.Hero => "Legendary Hero",
            ReputationLevel.Good => "Honorable",
            ReputationLevel.Neutral => "Unknown",
            ReputationLevel.Bad => "Suspicious",
            ReputationLevel.Evil => "Villain",
            _ => "Unknown"
        };
    }
}

// Quest branch - represents a choice in a quest
public class QuestBranch
{
    public string ChoiceText { get; set; }
    public string Outcome { get; set; }
    public int ReputationChange { get; set; }
    public int? GoldChange { get; set; }
    public Action<RPGGame>? OnChoose { get; set; }

    public QuestBranch(string choiceText, string outcome, int reputationChange = 0)
    {
        ChoiceText = choiceText;
        Outcome = outcome;
        ReputationChange = reputationChange;
    }
}

// Extended quest with branching
public class BranchingQuest : Quest
{
    public List<QuestBranch> Branches { get; set; } = new List<QuestBranch>();
    public QuestBranch? ChosenBranch { get; set; } = null;

    public BranchingQuest(string id, string title, string description, QuestType type)
        : base(id, title, description, type)
    {
    }

    public void ChooseBranch(int branchIndex, RPGGame game)
    {
        if (branchIndex < 0 || branchIndex >= Branches.Count) return;

        ChosenBranch = Branches[branchIndex];

        // Apply consequences
        game.AdjustReputation(ChosenBranch.ReputationChange);

        if (ChosenBranch.GoldChange.HasValue)
        {
            game.SetGoldForTesting(game.PlayerGold + ChosenBranch.GoldChange.Value);
        }

        ChosenBranch.OnChoose?.Invoke(game);
    }

    // ════════════════════════════════════════════════════════════════════
    // PREDEFINED BRANCHING QUESTS
    // ════════════════════════════════════════════════════════════════════

    public static BranchingQuest CreateMercyOrJustice()
    {
        var quest = new BranchingQuest(
            "mercy_or_justice",
            "A Thief Caught",
            "The guards caught a thief stealing bread. What should be his fate?",
            QuestType.Talk)
        {
            GoldReward = 50,
            XPReward = 25,
            GivenByNPC = "Captain Aldric"
        };

        quest.Objectives.Add(new QuestObjective("Decide the thief's fate", 1));

        // Branch 1: Show mercy (Good)
        quest.Branches.Add(new QuestBranch(
            "Let him go - he's just hungry.",
            "The guards reluctantly release the thief. He thanks you with tears in his eyes.",
            reputationChange: +10)
        {
            GoldChange = -10 // You give him 10 gold
        });

        // Branch 2: Justice (Neutral)
        quest.Branches.Add(new QuestBranch(
            "Let the law decide.",
            "The thief is taken to the magistrate. Justice will be served.",
            reputationChange: 0));

        // Branch 3: Harsh punishment (Evil)
        quest.Branches.Add(new QuestBranch(
            "Make an example of him.",
            "The guards beat the thief as a warning to others. The crowd looks at you with fear.",
            reputationChange: -15)
        {
            GoldChange = +20 // Town rewards your 'strength'
        });

        return quest;
    }

    public static BranchingQuest CreateSacrificeOrGreed()
    {
        var quest = new BranchingQuest(
            "sacrifice_or_greed",
            "The Mysterious Artifact",
            "You found a powerful artifact. The temple wants it for protection, but a merchant offers a fortune.",
            QuestType.Fetch)
        {
            GoldReward = 0, // Reward depends on choice
            XPReward = 50
        };

        quest.Objectives.Add(new QuestObjective("Decide the artifact's fate", 1));

        // Branch 1: Give to temple (Good)
        quest.Branches.Add(new QuestBranch(
            "Donate it to the Temple.",
            "The priests bless you. The artifact will protect the realm.",
            reputationChange: +20)
        {
            GoldChange = 0 // No gold, but big reputation boost
        });

        // Branch 2: Sell to merchant (Evil)
        quest.Branches.Add(new QuestBranch(
            "Sell it for 500 gold.",
            "The merchant grins. The priests curse your name.",
            reputationChange: -20)
        {
            GoldChange = 500 // Huge gold reward
        });

        // Branch 3: Keep it (Neutral)
        quest.Branches.Add(new QuestBranch(
            "Keep it for yourself.",
            "The artifact pulses with power in your hands. For better or worse, it's yours now.",
            reputationChange: -5)
        {
            OnChoose = game =>
            {
                // Bonus: +2 permanent strength from artifact
                game.SetPlayerStats(game.PlayerStrength + 2, game.PlayerDefense);
            }
        });

        return quest;
    }
}
