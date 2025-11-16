namespace ProjectEvolution.Game;

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

    public AutoPlayer(RPGGame game)
    {
        _game = game;
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
        // Simple AI: avoid mobs if low HP, otherwise explore randomly
        bool lowHP = _game.PlayerHP < _game.MaxPlayerHP * 0.3;

        // Check for nearby mobs
        var nearbyMobs = _game.GetMobsInRange(2);

        if (lowHP && nearbyMobs.Count > 0)
        {
            // Try to flee away from nearest mob
            var nearestMob = nearbyMobs.OrderBy(m =>
                Math.Abs(m.X - _game.PlayerX) + Math.Abs(m.Y - _game.PlayerY)).First();

            FleeFromMob(nearestMob);
        }
        else if (_game.GetCurrentTerrain() == "Town" && _game.PlayerGold >= 10 && _game.PlayerHP < _game.MaxPlayerHP)
        {
            // Visit inn if hurt and can afford it
            _game.VisitInn();
        }
        else if (_game.PlayerGold >= 5 && _game.PotionCount < 3)
        {
            // Buy potions if affordable
            if (_game.GetCurrentTerrain() == "Town")
            {
                _game.BuyPotion();
            }
            else
            {
                MoveRandomly();
            }
        }
        else
        {
            // Random exploration
            MoveRandomly();
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
        // Simple combat AI
        bool lowHP = _game.PlayerHP < _game.MaxPlayerHP * 0.3;
        bool lowStamina = _game.PlayerStamina < 6;

        // High chance to flee if very hurt
        if (lowHP && _random.Next(100) < 70)
        {
            return CombatAction.Defend; // Will handle flee separately
        }

        // Defend if low stamina
        if (lowStamina)
        {
            return CombatAction.Defend;
        }

        // Otherwise mostly attack (70% attack, 30% defend)
        return _random.Next(100) < 70 ? CombatAction.Attack : CombatAction.Defend;
    }

    public bool ShouldFlee()
    {
        // Flee if very low HP
        return _game.PlayerHP < _game.MaxPlayerHP * 0.25;
    }

    public bool ShouldUsePotion()
    {
        // Use potion if HP below 40% and have potions
        return _game.PlayerHP < _game.MaxPlayerHP * 0.4 && _game.PotionCount > 0;
    }
}
