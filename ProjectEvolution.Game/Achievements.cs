namespace ProjectEvolution.Game;

public enum AchievementCategory
{
    Combat,
    Exploration,
    Social,
    Virtue,
    Collection,
    Challenge
}

public class Achievement
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public AchievementCategory Category { get; set; }
    public bool IsUnlocked { get; set; } = false;
    public DateTime? UnlockedAt { get; set; }

    public Achievement(string id, string name, string description, AchievementCategory category)
    {
        Id = id;
        Name = name;
        Description = description;
        Category = category;
    }

    public void Unlock()
    {
        if (!IsUnlocked)
        {
            IsUnlocked = true;
            UnlockedAt = DateTime.Now;
        }
    }
}

public class AchievementSystem
{
    private List<Achievement> _achievements = new List<Achievement>();

    public AchievementSystem()
    {
        InitializeAchievements();
    }

    private void InitializeAchievements()
    {
        // COMBAT ACHIEVEMENTS
        _achievements.Add(new Achievement("first_blood", "First Blood", "Defeat your first enemy", AchievementCategory.Combat));
        _achievements.Add(new Achievement("slayer", "Slayer", "Defeat 50 enemies", AchievementCategory.Combat));
        _achievements.Add(new Achievement("legend", "Legend", "Defeat 200 enemies", AchievementCategory.Combat));
        _achievements.Add(new Achievement("flawless", "Flawless Victory", "Win a combat without taking damage", AchievementCategory.Combat));
        _achievements.Add(new Achievement("berserker", "Berserker", "Use Berserker Rage and survive", AchievementCategory.Combat));
        _achievements.Add(new Achievement("unstoppable", "Unstoppable", "Defeat 5 enemies in a row without healing", AchievementCategory.Combat));

        // EXPLORATION ACHIEVEMENTS
        _achievements.Add(new Achievement("explorer", "Explorer", "Reveal 50% of the world map", AchievementCategory.Exploration));
        _achievements.Add(new Achievement("cartographer", "Cartographer", "Reveal 100% of the world map", AchievementCategory.Exploration));
        _achievements.Add(new Achievement("delver", "Dungeon Delver", "Reach dungeon depth 3", AchievementCategory.Exploration));
        _achievements.Add(new Achievement("secret_hunter", "Secret Hunter", "Discover 3 world secrets", AchievementCategory.Exploration));
        _achievements.Add(new Achievement("completionist", "Completionist", "Discover all 6 world secrets", AchievementCategory.Exploration));

        // SOCIAL ACHIEVEMENTS
        _achievements.Add(new Achievement("socialite", "Socialite", "Talk to all 7 NPCs", AchievementCategory.Social));
        _achievements.Add(new Achievement("quest_giver", "Quest Taker", "Accept your first quest", AchievementCategory.Social));
        _achievements.Add(new Achievement("hero_for_hire", "Hero for Hire", "Complete 5 quests", AchievementCategory.Social));
        _achievements.Add(new Achievement("not_alone", "Not Alone Anymore", "Recruit your first companion", AchievementCategory.Social));
        _achievements.Add(new Achievement("devoted", "Devoted", "Raise a companion's loyalty to 90+", AchievementCategory.Social));

        // VIRTUE ACHIEVEMENTS
        _achievements.Add(new Achievement("virtuous", "Virtuous", "Reach 60+ in any virtue", AchievementCategory.Virtue));
        _achievements.Add(new Achievement("exemplar", "Exemplar", "Reach 80+ in any virtue", AchievementCategory.Virtue));
        _achievements.Add(new Achievement("avatar", "The Avatar", "Reach 80+ in ALL virtues", AchievementCategory.Virtue));
        _achievements.Add(new Achievement("merciful", "Merciful Heart", "Show mercy 10 times", AchievementCategory.Virtue));
        _achievements.Add(new Achievement("honest", "Honest Soul", "Never lie in dialogue", AchievementCategory.Virtue));

        // COLLECTION ACHIEVEMENTS
        _achievements.Add(new Achievement("armed", "Well Armed", "Equip an Iron Sword or better", AchievementCategory.Collection));
        _achievements.Add(new Achievement("armored", "Well Armored", "Equip Chain Mail or better", AchievementCategory.Collection));
        _achievements.Add(new Achievement("wealthy", "Wealthy", "Accumulate 500 gold", AchievementCategory.Collection));
        _achievements.Add(new Achievement("rich", "Filthy Rich", "Accumulate 2000 gold", AchievementCategory.Collection));

        // CHALLENGE ACHIEVEMENTS
        _achievements.Add(new Achievement("survivor", "Survivor", "Die and respawn 5 times", AchievementCategory.Challenge));
        _achievements.Add(new Achievement("demon_slayer", "Demon Slayer", "Defeat a demon-family enemy", AchievementCategory.Challenge));
        _achievements.Add(new Achievement("undead_hunter", "Undead Hunter", "Defeat all 3 undead types", AchievementCategory.Challenge));
        _achievements.Add(new Achievement("beast_master", "Beast Master", "Defeat all 3 beast types", AchievementCategory.Challenge));
        _achievements.Add(new Achievement("rare_encounter", "Legendary", "Defeat a rare encounter boss", AchievementCategory.Challenge));
        _achievements.Add(new Achievement("main_quest_done", "The Demon Lord Falls", "Complete the main questline", AchievementCategory.Challenge));
    }

    public List<Achievement> GetAllAchievements()
    {
        return _achievements.ToList();
    }

    public List<Achievement> GetUnlockedAchievements()
    {
        return _achievements.Where(a => a.IsUnlocked).ToList();
    }

    public Achievement? GetAchievement(string id)
    {
        return _achievements.FirstOrDefault(a => a.Id == id);
    }

    public bool UnlockAchievement(string id)
    {
        var achievement = GetAchievement(id);
        if (achievement != null && !achievement.IsUnlocked)
        {
            achievement.Unlock();
            return true; // Newly unlocked
        }
        return false; // Already unlocked or doesn't exist
    }

    public int GetTotalAchievements()
    {
        return _achievements.Count;
    }

    public int GetUnlockedCount()
    {
        return _achievements.Count(a => a.IsUnlocked);
    }

    public int GetCompletionPercentage()
    {
        if (_achievements.Count == 0) return 0;
        return (GetUnlockedCount() * 100) / _achievements.Count;
    }

    public List<Achievement> GetByCategory(AchievementCategory category)
    {
        return _achievements.Where(a => a.Category == category).ToList();
    }
}
