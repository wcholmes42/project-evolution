namespace ProjectEvolution.Game;

public enum AIGoal
{
    HuntMobs,       // Actively seek combat for XP/gold
    SeekTown,       // Navigate to nearest town for healing
    ExploreDungeon, // Enter and explore dungeons
    Survive,        // Defensive mode - avoid combat
    Explore         // Random exploration
}

public class AutoPlayer
{
    private readonly Random _random = new Random();
    private readonly RPGGame _game;

    public int TurnsSurvived { get; set; }
    public int CombatsWon { get; set; }
    public int CombatsFled { get; set; }
    public int GoldEarned { get; set; }
    public int DamageReceived { get; set; }
    public string DeathReason { get; set; } = "";
    public bool IsAlive => _game.PlayerHP > 0;

    private AIGoal _currentGoal = AIGoal.Explore;
    private (int x, int y)? _targetLocation = null;

    public AutoPlayer(RPGGame game)
    {
        _game = game;
    }

    private AIGoal DetermineGoal()
    {
        double hpPercent = (double)_game.PlayerHP / _game.MaxPlayerHP;
        bool needsHealing = hpPercent < 0.5;
        bool criticalHP = hpPercent < 0.3;
        bool isStrong = _game.PlayerLevel > 1 && hpPercent > 0.6;
        bool nearLevelUp = _game.PlayerXP >= _game.XPForNextLevel * 0.8;

        // Critical HP - survival mode
        if (criticalHP)
        {
            return AIGoal.SeekTown;
        }

        // Needs healing but not critical
        if (needsHealing && _game.PlayerGold >= 10)
        {
            return AIGoal.SeekTown;
        }

        // Strong and near level up - hunt for that last bit of XP!
        if (nearLevelUp && isStrong)
        {
            return AIGoal.HuntMobs;
        }

        // Strong and healthy - be aggressive!
        if (isStrong)
        {
            // 60% hunt, 30% explore, 10% dungeon
            int roll = _random.Next(100);
            if (roll < 60) return AIGoal.HuntMobs;
            if (roll < 90) return AIGoal.Explore;
            return AIGoal.ExploreDungeon;
        }

        // Default: explore
        return AIGoal.Explore;
    }

    public void PlayTurn()
    {
        TurnsSurvived++;

        if (_game.InDungeon)
        {
            PlayDungeonTurn();
        }
        else
        {
            PlayWorldTurn();
        }
    }

    private void PlayWorldTurn()
    {
        // Determine current goal based on game state
        _currentGoal = DetermineGoal();

        // Handle town interactions first
        if (_game.GetCurrentTerrain() == "Town")
        {
            HandleTownInteraction();
            return;
        }

        // Execute goal-based behavior
        switch (_currentGoal)
        {
            case AIGoal.HuntMobs:
                HuntNearestMob();
                break;

            case AIGoal.SeekTown:
                NavigateToNearestTown();
                break;

            case AIGoal.ExploreDungeon:
                NavigateToNearestDungeon();
                break;

            case AIGoal.Survive:
                FleeFromAllThreats();
                break;

            default: // Explore
                ExploreIntelligently();
                break;
        }
    }

    private void HandleTownInteraction()
    {
        // Prioritize healing if hurt
        if (_game.PlayerHP < _game.MaxPlayerHP && _game.PlayerGold >= 10)
        {
            _game.VisitInn();
        }
        // Buy potions if affordable and don't have many
        else if (_game.PlayerGold >= 5 && _game.PotionCount < 2)
        {
            _game.BuyPotion();
        }
        // Leave town and continue adventuring
        else
        {
            MoveRandomly();
        }
    }

    private void HuntNearestMob()
    {
        // Actively seek mobs for combat!
        var nearbyMobs = _game.GetMobsInRange(8); // Scan wider area

        if (nearbyMobs.Count > 0)
        {
            // Evaluate which mob to hunt
            var targetMob = nearbyMobs
                .OrderBy(m => EvaluateThreatLevel(m)) // Hunt easiest first
                .ThenBy(m => Math.Abs(m.X - _game.PlayerX) + Math.Abs(m.Y - _game.PlayerY))
                .First();

            MoveToward(targetMob.X, targetMob.Y);
        }
        else
        {
            // No mobs visible, explore to find some
            ExploreIntelligently();
        }
    }

    private int EvaluateThreatLevel(Mob mob)
    {
        // Lower = easier target
        // Consider mob level vs player level
        int levelDiff = mob.Level - _game.PlayerLevel;

        // Prefer lower level mobs
        return levelDiff * 10;
    }

    private bool ShouldEngageMob(Mob mob)
    {
        // Risk vs Reward evaluation
        double hpPercent = (double)_game.PlayerHP / _game.MaxPlayerHP;

        // Always engage if very strong
        if (hpPercent > 0.8 && _game.PlayerLevel >= mob.Level)
        {
            return true;
        }

        // Engage weaker mobs if decent HP
        if (hpPercent > 0.5 && _game.PlayerLevel > mob.Level)
        {
            return true;
        }

        // Need XP for level up? Take calculated risk
        bool closeToLevelUp = _game.PlayerXP >= _game.XPForNextLevel * 0.8;
        if (closeToLevelUp && hpPercent > 0.4 && _game.PlayerLevel >= mob.Level)
        {
            return true;
        }

        // Otherwise avoid
        return false;
    }

    private void NavigateToNearestTown()
    {
        // Known town locations
        List<(int x, int y)> towns = new List<(int, int)>
        {
            (5, 5),
            (15, 15)
        };

        var nearestTown = towns
            .OrderBy(t => Math.Abs(t.x - _game.PlayerX) + Math.Abs(t.y - _game.PlayerY))
            .First();

        MoveToward(nearestTown.x, nearestTown.y);
    }

    private void NavigateToNearestDungeon()
    {
        // Known dungeon locations
        List<(int x, int y)> dungeons = new List<(int, int)>
        {
            (10, 5),
            (10, 15)
        };

        var nearestDungeon = dungeons
            .OrderBy(d => Math.Abs(d.x - _game.PlayerX) + Math.Abs(d.y - _game.PlayerY))
            .First();

        MoveToward(nearestDungeon.x, nearestDungeon.y);

        // Enter if at dungeon
        if (_game.GetCurrentTerrain() == "Dungeon")
        {
            _game.EnterDungeon();
        }
    }

    private void FleeFromAllThreats()
    {
        var nearbyMobs = _game.GetMobsInRange(4);

        if (nearbyMobs.Count > 0)
        {
            var nearestMob = nearbyMobs
                .OrderBy(m => Math.Abs(m.X - _game.PlayerX) + Math.Abs(m.Y - _game.PlayerY))
                .First();

            FleeFromMob(nearestMob);
        }
        else
        {
            // No immediate threats, move toward town
            NavigateToNearestTown();
        }
    }

    private void ExploreIntelligently()
    {
        // Prefer unexplored areas
        // For now, just random with slight bias toward center/points of interest
        var nearbyMobs = _game.GetMobsInRange(3);

        // If mobs nearby, evaluate whether to engage
        if (nearbyMobs.Count > 0)
        {
            var nearestMob = nearbyMobs
                .OrderBy(m => Math.Abs(m.X - _game.PlayerX) + Math.Abs(m.Y - _game.PlayerY))
                .First();

            if (ShouldEngageMob(nearestMob))
            {
                // Move toward mob to engage!
                MoveToward(nearestMob.X, nearestMob.Y);
                return;
            }
        }

        // Random exploration
        MoveRandomly();
    }

    private void MoveToward(int targetX, int targetY)
    {
        // Simple pathfinding - move toward target
        int dx = targetX - _game.PlayerX;
        int dy = targetY - _game.PlayerY;

        // Prefer moving on axis with greater distance
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            // Move horizontally
            if (dx > 0) _game.MoveEast();
            else if (dx < 0) _game.MoveWest();
            else MoveRandomly();
        }
        else
        {
            // Move vertically
            if (dy > 0) _game.MoveSouth();
            else if (dy < 0) _game.MoveNorth();
            else MoveRandomly();
        }
    }

    private void FleeFromMob(Mob mob)
    {
        // Move away from mob
        int dx = _game.PlayerX - mob.X;
        int dy = _game.PlayerY - mob.Y;

        // Try to move in opposite direction
        if (Math.Abs(dx) > Math.Abs(dy))
        {
            if (dx > 0) _game.MoveEast();
            else _game.MoveWest();
        }
        else
        {
            if (dy > 0) _game.MoveSouth();
            else _game.MoveNorth();
        }
    }

    private void MoveRandomly()
    {
        int direction = _random.Next(4);
        switch (direction)
        {
            case 0: _game.MoveNorth(); break;
            case 1: _game.MoveSouth(); break;
            case 2: _game.MoveEast(); break;
            case 3: _game.MoveWest(); break;
        }
    }

    private void PlayDungeonTurn()
    {
        // In dungeon: explore randomly, exit if low HP
        bool lowHP = _game.PlayerHP < _game.MaxPlayerHP * 0.4;

        if (lowHP)
        {
            _game.ExitDungeon();
        }
        else
        {
            MoveRandomly();
        }
    }

    public CombatAction DecideCombatAction()
    {
        double hpPercent = (double)_game.PlayerHP / _game.MaxPlayerHP;
        bool lowStamina = _game.PlayerStamina < 6;

        // Critical HP - defend to minimize damage
        if (hpPercent < 0.25)
        {
            return CombatAction.Defend;
        }

        // Defend if low stamina (forced anyway)
        if (lowStamina)
        {
            return CombatAction.Defend;
        }

        // Aggressive when hunting or strong
        if (_currentGoal == AIGoal.HuntMobs && hpPercent > 0.5)
        {
            return CombatAction.Attack; // Always attack when hunting!
        }

        // Risk-based combat
        if (hpPercent > 0.6)
        {
            return CombatAction.Attack; // Aggressive when healthy
        }
        else if (hpPercent > 0.4)
        {
            // Mixed strategy when moderate HP (60% attack)
            return _random.Next(100) < 60 ? CombatAction.Attack : CombatAction.Defend;
        }
        else
        {
            // Defensive when hurt (30% attack)
            return _random.Next(100) < 30 ? CombatAction.Attack : CombatAction.Defend;
        }
    }

    public bool ShouldFlee()
    {
        double hpPercent = (double)_game.PlayerHP / _game.MaxPlayerHP;

        // Always flee if critical
        if (hpPercent < 0.2)
        {
            return true;
        }

        // Flee if hurt and enemy is higher level
        if (hpPercent < 0.4 && _game.EnemyLevel > _game.PlayerLevel)
        {
            return true;
        }

        // Don't flee if hunting (we wanted this fight!)
        if (_currentGoal == AIGoal.HuntMobs && hpPercent > 0.3)
        {
            return false;
        }

        return false;
    }

    public bool ShouldUsePotion()
    {
        double hpPercent = (double)_game.PlayerHP / _game.MaxPlayerHP;

        // Use potion if HP below 35% and have potions
        return hpPercent < 0.35 && _game.PotionCount > 0;
    }

    public string GetCurrentGoalDescription()
    {
        return _currentGoal switch
        {
            AIGoal.HuntMobs => "🎯 HUNTING",
            AIGoal.SeekTown => "🏥 SEEKING TOWN",
            AIGoal.ExploreDungeon => "⚔️ DUNGEON DELVING",
            AIGoal.Survive => "🛡️ SURVIVING",
            _ => "🗺️ EXPLORING"
        };
    }
}
