namespace ProjectEvolution.Game;

/// <summary>
/// Represents a dialogue option that the player can choose
/// </summary>
public class DialogueChoice
{
    public string Text { get; set; }
    public string Response { get; set; }
    public string? NextNodeId { get; set; } // null = ends conversation
    public Action<RPGGame>? OnChoose { get; set; } // Optional side effect

    public DialogueChoice(string text, string response, string? nextNodeId = null, Action<RPGGame>? onChoose = null)
    {
        Text = text;
        Response = response;
        NextNodeId = nextNodeId;
        OnChoose = onChoose;
    }
}

/// <summary>
/// A node in a dialogue tree
/// </summary>
public class DialogueNode
{
    public string Id { get; set; }
    public string NPCText { get; set; }
    public List<DialogueChoice> Choices { get; set; } = new List<DialogueChoice>();

    public DialogueNode(string id, string npcText)
    {
        Id = id;
        NPCText = npcText;
    }
}

/// <summary>
/// A complete dialogue tree for an NPC
/// </summary>
public class DialogueTree
{
    public string RootNodeId { get; set; } = "start";
    public Dictionary<string, DialogueNode> Nodes { get; set; } = new Dictionary<string, DialogueNode>();

    public DialogueNode? GetNode(string nodeId)
    {
        return Nodes.ContainsKey(nodeId) ? Nodes[nodeId] : null;
    }

    public void AddNode(DialogueNode node)
    {
        Nodes[node.Id] = node;
    }
}

/// <summary>
/// Non-Player Character with dialogue and location
/// </summary>
public class NPC
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public DialogueTree Dialogue { get; set; }
    public string CurrentNodeId { get; set; } = "start";

    public NPC(string name, string description, int x, int y)
    {
        Name = name;
        Description = description;
        X = x;
        Y = y;
        Dialogue = new DialogueTree();
    }

    /// <summary>
    /// Get the current dialogue node
    /// </summary>
    public DialogueNode? GetCurrentNode()
    {
        return Dialogue.GetNode(CurrentNodeId);
    }

    /// <summary>
    /// Choose a dialogue option and advance the conversation
    /// </summary>
    public string Choose(int choiceIndex, RPGGame game)
    {
        var currentNode = GetCurrentNode();
        if (currentNode == null || choiceIndex < 0 || choiceIndex >= currentNode.Choices.Count)
        {
            return "Invalid choice.";
        }

        var choice = currentNode.Choices[choiceIndex];

        // Execute side effect if any
        choice.OnChoose?.Invoke(game);

        // Move to next node or end conversation
        if (choice.NextNodeId != null)
        {
            CurrentNodeId = choice.NextNodeId;
        }
        else
        {
            CurrentNodeId = "start"; // Reset for next conversation
        }

        return choice.Response;
    }

    /// <summary>
    /// Reset conversation to the beginning
    /// </summary>
    public void ResetDialogue()
    {
        CurrentNodeId = "start";
    }

    // ════════════════════════════════════════════════════════════════════
    // PREDEFINED NPCs
    // ════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Create the Innkeeper NPC
    /// </summary>
    public static NPC CreateInnkeeper(int x, int y)
    {
        var npc = new NPC("Mara the Innkeeper", "A warm, welcoming innkeeper", x, y);

        // Start node
        var start = new DialogueNode("start",
            "Welcome to the Restful Inn! What can I do for you, traveler?");
        start.Choices.Add(new DialogueChoice(
            "Tell me about this place.",
            "This inn has been here for generations. We provide rest and healing for weary adventurers.",
            "about_inn"));
        start.Choices.Add(new DialogueChoice(
            "Any rumors?",
            "I hear strange creatures have been spotted near the old temple... undead, beasts, even demons!",
            "rumors"));
        start.Choices.Add(new DialogueChoice(
            "Goodbye.",
            "Safe travels, friend!"));

        // About inn node
        var aboutInn = new DialogueNode("about_inn",
            "My family has run this inn since before the great war. We've seen many heroes pass through.");
        aboutInn.Choices.Add(new DialogueChoice(
            "What happened in the war?",
            "Dark times... The demon lord nearly destroyed everything. But heroes like you stopped him!",
            "war_story"));
        aboutInn.Choices.Add(new DialogueChoice(
            "Thanks for the history.",
            "Anytime! Come back soon."));

        // War story node
        var warStory = new DialogueNode("war_story",
            "They say the demon lord's essence still lingers in the deepest dungeons...");
        warStory.Choices.Add(new DialogueChoice(
            "I'll put an end to it!",
            "Brave words! May the gods protect you."));

        // Rumors node
        var rumors = new DialogueNode("rumors",
            "The old hermit outside town might know more. He studies ancient texts.");
        rumors.Choices.Add(new DialogueChoice(
            "Where can I find him?",
            "Just west of here, near the forest edge. Look for smoke from his campfire.",
            "hermit_location"));
        rumors.Choices.Add(new DialogueChoice(
            "Thanks for the tip.",
            "Good luck out there!"));

        // Hermit location
        var hermitLoc = new DialogueNode("hermit_location",
            "Be careful around him though - he's... eccentric.");
        hermitLoc.Choices.Add(new DialogueChoice(
            "I'll keep that in mind.",
            "Smart choice!"));

        npc.Dialogue.AddNode(start);
        npc.Dialogue.AddNode(aboutInn);
        npc.Dialogue.AddNode(warStory);
        npc.Dialogue.AddNode(rumors);
        npc.Dialogue.AddNode(hermitLoc);

        return npc;
    }

    /// <summary>
    /// Create the Blacksmith NPC
    /// </summary>
    public static NPC CreateBlacksmith(int x, int y)
    {
        var npc = new NPC("Gareth the Blacksmith", "A burly smith with calloused hands", x, y);

        var start = new DialogueNode("start",
            "Need a weapon forged? Or just here to chat?");
        start.Choices.Add(new DialogueChoice(
            "What's your finest work?",
            "That enchanted blade over there - took me a month to forge! But it's not for sale... yet.",
            "finest_work"));
        start.Choices.Add(new DialogueChoice(
            "Tips for fighting demons?",
            "Silver weapons work best! Fire won't hurt 'em much, but cold iron does the trick.",
            "demon_tips"));
        start.Choices.Add(new DialogueChoice(
            "See you later.",
            "Come back when you need proper steel!"));

        var finestWork = new DialogueNode("finest_work",
            "If you can bring me dragon scales, I'll make you something legendary!");
        finestWork.Choices.Add(new DialogueChoice(
            "I'll find those scales!",
            "Hah! I like your spirit!"));

        var demonTips = new DialogueNode("demon_tips",
            "And don't forget - demons can cast spells just like you. Fight fire with fire!");
        demonTips.Choices.Add(new DialogueChoice(
            "Good to know!",
            "Stay sharp out there!"));

        npc.Dialogue.AddNode(start);
        npc.Dialogue.AddNode(finestWork);
        npc.Dialogue.AddNode(demonTips);

        return npc;
    }

    /// <summary>
    /// Create a Town Guard NPC
    /// </summary>
    public static NPC CreateGuard(int x, int y)
    {
        var npc = new NPC("Captain Aldric", "A stern town guard captain", x, y);

        var start = new DialogueNode("start",
            "Keep the peace, citizen. What do you need?");
        start.Choices.Add(new DialogueChoice(
            "Heard of any threats nearby?",
            "Wolves from the forest have been getting bold. And there are worse things in the dungeons.",
            "threats"));
        start.Choices.Add(new DialogueChoice(
            "Just passing through.",
            "Move along then."));

        var threats = new DialogueNode("threats",
            "If you're brave enough to clear out those dungeons, the town would reward you handsomely.");
        threats.Choices.Add(new DialogueChoice(
            "I'll take care of it.",
            "We'll see if you survive first. Good luck."));

        npc.Dialogue.AddNode(start);
        npc.Dialogue.AddNode(threats);

        return npc;
    }

    /// <summary>
    /// Create a Mysterious Stranger NPC
    /// </summary>
    public static NPC CreateStranger(int x, int y)
    {
        var npc = new NPC("???", "A hooded figure in the shadows", x, y);

        var start = new DialogueNode("start",
            "...You can see me? Interesting.");
        start.Choices.Add(new DialogueChoice(
            "Who are you?",
            "Just a wanderer, like yourself. But I've seen things... things you wouldn't believe.",
            "identity"));
        start.Choices.Add(new DialogueChoice(
            "What do you want?",
            "To warn you. The deeper you go, the more reality bends. Not all enemies are what they seem.",
            "warning"));
        start.Choices.Add(new DialogueChoice(
            "Leave me alone.",
            "*The figure fades into the shadows without a word*"));

        var identity = new DialogueNode("identity",
            "I've walked between worlds. Fought alongside heroes and villains alike.");
        identity.Choices.Add(new DialogueChoice(
            "Teach me your ways.",
            "You must discover your own path. But here - take this advice: trust in your skills.",
            null,
            game => game.SetGoldForTesting(game.PlayerGold + 50))); // Give 50 gold as a "gift"

        var warning = new DialogueNode("warning",
            "The demon lord's influence corrupts. Some enemies can turn your own power against you.");
        warning.Choices.Add(new DialogueChoice(
            "I'll be careful.",
            "See that you are. *The figure vanishes*"));

        npc.Dialogue.AddNode(start);
        npc.Dialogue.AddNode(identity);
        npc.Dialogue.AddNode(warning);

        return npc;
    }
}
