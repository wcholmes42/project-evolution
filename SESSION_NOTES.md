# Project Evolution - Session Notes

**Last Updated**: 2025-11-15
**Current Generation**: 4
**Status**: ✅ Ready to continue

## Quick Start
```bash
cd project-evolution
dotnet test                    # Run all tests
dotnet run --project ProjectEvolution.Game  # Play the game
```

## Evolution Roadmap

### ✅ Completed Generations

#### Generation 0: The Beginning
- Auto-win game
- Validates TDD setup
- Test: `StartGame_ImmediatelyWins`

#### Generation 1: Chance
- Coin flip mechanic
- Random win/lose
- Tests: `CoinFlipGame_CanWin`, `CoinFlipGame_CanLose`, `CoinFlipGame_RandomOutcome_ProducesBothResults`

#### Generation 2: Player Agency
- Combat with weak goblin
- Player chooses: Attack (wins) or Defend (loses)
- Tests: `Combat_PlayerAttacks_DefeatsWeakEnemy`, `Combat_PlayerDefends_FailsToDefeatEnemy`, `Combat_NotStarted_CannotChooseAction`

#### Generation 3: Enemy AI
- Enemy fights back with random actions
- Rock-paper-scissors combat:
  - Attack vs Attack = 50/50 coin flip
  - Attack vs Defend = Attacker wins
  - Defend vs Attack = Attacker wins
  - Defend vs Defend = Draw (player loses)
- Added CombatLog property for narrative feedback
- Tests: `CombatWithAI_PlayerAttacks_EnemyDefends_PlayerWins`, `CombatWithAI_PlayerDefends_EnemyAttacks_PlayerLoses`, `CombatWithAI_BothDefend_Draw_PlayerLoses`, `CombatWithAI_BothAttack_CoinFlip_CanWin`, `CombatWithAI_BothAttack_CoinFlip_CanLose`, `CombatWithAI_RandomEnemy_ProducesBothOutcomes`

#### Generation 4: Health Points (Current)
- Player starts with 10 HP
- Enemy starts with 3 HP
- Multi-round combat system
- Attack deals 1 damage (if not blocked)
- Defend blocks all incoming damage
- Combat ends when either reaches 0 HP
- Added properties: PlayerHP, EnemyHP, CombatEnded
- Tests: `CombatWithHP_PlayerStartsWith10HP`, `CombatWithHP_EnemyStartsWith3HP`, `CombatWithHP_PlayerAttacks_EnemyDefends_EnemyTakesNoDamage`, `CombatWithHP_PlayerAttacks_EnemyAttacks_EnemyTakesDamage`, `CombatWithHP_PlayerDefends_EnemyAttacks_PlayerTakesNoDamage`, `CombatWithHP_DefeatEnemy_PlayerWins`, `CombatWithHP_PlayerHPReaches0_PlayerLoses`, `CombatWithHP_CombatEndsWhenEitherDies`
- **Current Test Count**: 21 passing

### 🎯 Next Generations (Planned)

#### Generation 5: Loot & Rewards
- Win combat → gain gold
- Track player inventory
- Display rewards

#### Generation 6: Multiple Enemies
- Face 2-3 goblins in sequence
- Must defeat all to win
- Progressive difficulty

#### Generation 7: Character Stats
- Player Strength/Defense attributes
- Stats affect combat outcomes
- Foundation for character progression

## Technical Details

### Project Structure
```
project-evolution/
├── ProjectEvolution.sln
├── ProjectEvolution.Game/
│   ├── Program.cs              # Console interface
│   ├── RPGGame.cs              # Core game logic
│   └── CombatAction.cs         # Enum: Attack, Defend
└── ProjectEvolution.Tests/
    └── UnitTest1.cs            # All tests (7 passing)
```

### Current API
```csharp
var game = new RPGGame();

// Generation 0
game.Start();  // Auto-win

// Generation 1
game.StartWithCoinFlip(heads: true);  // Deterministic
game.StartWithRandomCoinFlip();       // Random

// Generation 2
game.StartCombat();
game.ChooseAction(CombatAction.Attack);  // or CombatAction.Defend

// Generation 3
game.StartCombatWithAI();
game.ChooseActionWithRandomEnemy(CombatAction.Attack);
// or for testing:
game.ChooseActionAgainstEnemy(CombatAction.Attack, enemyAction: CombatAction.Defend);

// Generation 4
game.StartCombatWithHP();
while (!game.CombatEnded)
{
    game.ExecuteHPCombatRoundWithRandomEnemy(CombatAction.Attack);
    // or for testing:
    // game.ExecuteHPCombatRound(CombatAction.Attack, CombatAction.Defend);
}

// Check results
bool won = game.IsWon;
string log = game.CombatLog;
int playerHP = game.PlayerHP;
int enemyHP = game.EnemyHP;
bool ended = game.CombatEnded;
```

### Design Principles
1. **TDD First**: Always write failing test before implementation
2. **Backward Compatible**: Old tests keep passing
3. **Incremental**: Each gen adds ONE core concept
4. **Thematic**: Stay true to RPG theme
5. **Git History**: Each generation is a commit

## GitHub Repository
https://github.com/wcholmes42/project-evolution

## How to Continue This Project

1. Read this file to understand current state
2. Run `dotnet test` to verify all tests pass
3. Implement next generation following TDD:
   - Write failing tests
   - Implement minimal code to pass
   - Update Program.cs for user experience
   - Update this SESSION_NOTES.md
   - Commit and push
4. Repeat for next generation

## Notes for Future Sessions

- Tech stack: C# .NET 9.0, xUnit
- All generations maintain backward compatibility
- Tests are in one file (UnitTest1.cs) for simplicity
- Console uses ReadLine/ReadKey for interaction
- Random seed not controlled (true randomness in tests where applicable)

---
*This file is updated after each generation to ensure safe session closure*
