namespace ProjectEvolution.Game;

public enum QuestType
{
    Kill,       // Kill X enemies
    Fetch,      // Collect X items (gold, artifacts)
    Explore,    // Visit X locations
    Talk        // Talk to X NPCs
}

public enum QuestStatus
{
    NotStarted,
    InProgress,
    Completed,
    Failed
}

public class QuestObjective
{
    public string Description { get; set; }
    public int Required { get; set; }
    public int Current { get; set; }
    public bool IsComplete => Current >= Required;

    public QuestObjective(string description, int required)
    {
        Description = description;
        Required = required;
        Current = 0;
    }

    public void Increment(int amount = 1)
    {
        Current = Math.Min(Current + amount, Required);
    }
}

public class Quest
{
    public string Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public QuestType Type { get; set; }
    public QuestStatus Status { get; set; } = QuestStatus.NotStarted;
    public List<QuestObjective> Objectives { get; set; } = new List<QuestObjective>();
    public int GoldReward { get; set; }
    public int XPReward { get; set; }
    public string? GivenByNPC { get; set; }

    public Quest(string id, string title, string description, QuestType type)
    {
        Id = id;
        Title = title;
        Description = description;
        Type = type;
    }

    public bool IsComplete()
    {
        return Objectives.All(o => o.IsComplete);
    }

    public void Start()
    {
        Status = QuestStatus.InProgress;
    }

    public void Complete()
    {
        Status = QuestStatus.Completed;
    }

    public void Fail()
    {
        Status = QuestStatus.Failed;
    }

    public string GetProgress()
    {
        if (Objectives.Count == 0) return "No objectives";

        var completed = Objectives.Count(o => o.IsComplete);
        return $"{completed}/{Objectives.Count} objectives complete";
    }

    // ════════════════════════════════════════════════════════════════════
    // PREDEFINED QUESTS
    // ════════════════════════════════════════════════════════════════════

    public static Quest CreateGoblinThreat()
    {
        var quest = new Quest(
            "goblin_threat",
            "Goblin Menace",
            "The town guard reports increased goblin activity. Clear them out!",
            QuestType.Kill)
        {
            GoldReward = 100,
            XPReward = 50,
            GivenByNPC = "Captain Aldric"
        };

        quest.Objectives.Add(new QuestObjective("Defeat 5 goblins", 5));
        return quest;
    }

    public static Quest CreateUndeadRising()
    {
        var quest = new Quest(
            "undead_rising",
            "The Undead Rise",
            "Skeletons and zombies have been spotted near the graveyard. Investigate!",
            QuestType.Kill)
        {
            GoldReward = 150,
            XPReward = 75,
            GivenByNPC = "Mara the Innkeeper"
        };

        quest.Objectives.Add(new QuestObjective("Defeat 3 undead creatures", 3));
        return quest;
    }

    public static Quest CreateBeastHunter()
    {
        var quest = new Quest(
            "beast_hunter",
            "The Beast Plague",
            "Wild beasts are terrorizing travelers. Hunt them down!",
            QuestType.Kill)
        {
            GoldReward = 200,
            XPReward = 100,
            GivenByNPC = "Gareth the Blacksmith"
        };

        quest.Objectives.Add(new QuestObjective("Hunt 3 beasts (Wolf, Bear, or Serpent)", 3));
        return quest;
    }

    public static Quest CreateDemonHunter()
    {
        var quest = new Quest(
            "demon_hunter",
            "Demonic Incursion",
            "Demons have been sighted! This is grave news indeed.",
            QuestType.Kill)
        {
            GoldReward = 300,
            XPReward = 150,
            GivenByNPC = "???"
        };

        quest.Objectives.Add(new QuestObjective("Slay 2 demons", 2));
        return quest;
    }

    public static Quest CreateExplorer()
    {
        var quest = new Quest(
            "explorer",
            "Explore the Realm",
            "Visit all the major locations in the world.",
            QuestType.Explore)
        {
            GoldReward = 100,
            XPReward = 50
        };

        quest.Objectives.Add(new QuestObjective("Visit both towns", 2));
        quest.Objectives.Add(new QuestObjective("Enter both dungeons", 2));
        quest.Objectives.Add(new QuestObjective("Pray at the Temple", 1));
        return quest;
    }

    public static Quest CreateDungeonDelver()
    {
        var quest = new Quest(
            "dungeon_delver",
            "Into the Depths",
            "Reach the bottom of any dungeon and defeat the boss.",
            QuestType.Explore)
        {
            GoldReward = 500,
            XPReward = 250,
            GivenByNPC = "Gareth the Blacksmith"
        };

        quest.Objectives.Add(new QuestObjective("Reach dungeon depth 3", 1));
        quest.Objectives.Add(new QuestObjective("Defeat a dungeon boss", 1));
        return quest;
    }
}
