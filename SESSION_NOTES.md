# Project Evolution - Session Notes

**Last Updated**: 2025-11-15
**Current Generation**: 12 (FINAL - Combat Dynamics Complete!)
**Status**: ✅ ALL EVOLUTIONS COMPLETE - Ready to test!

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

#### Generation 4: Health Points
- Player starts with 10 HP
- Enemy starts with 3 HP
- Multi-round combat system
- Attack deals 1 damage (if not blocked)
- Defend blocks all incoming damage
- Combat ends when either reaches 0 HP
- Added properties: PlayerHP, EnemyHP, CombatEnded
- Tests: `CombatWithHP_PlayerStartsWith10HP`, `CombatWithHP_EnemyStartsWith3HP`, `CombatWithHP_PlayerAttacks_EnemyDefends_EnemyTakesNoDamage`, `CombatWithHP_PlayerAttacks_EnemyAttacks_EnemyTakesDamage`, `CombatWithHP_PlayerDefends_EnemyAttacks_PlayerTakesNoDamage`, `CombatWithHP_DefeatEnemy_PlayerWins`, `CombatWithHP_PlayerHPReaches0_PlayerLoses`, `CombatWithHP_CombatEndsWhenEitherDies`

#### Generation 5: Loot & Rewards
- Player gains 10 gold per defeated enemy
- Added PlayerGold property
- Gold persists across multiple combats
- Combat log shows gold earned
- Only award gold on victory (not defeat)
- Tests: `CombatWithLoot_PlayerStartsWith0Gold`, `CombatWithLoot_DefeatEnemy_PlayerGainsGold`, `CombatWithLoot_LoseToEnemy_NoGoldAwarded`, `CombatWithLoot_GoldPersistsAcrossMultipleCombats`

#### Generation 6: Multiple Enemies
- Face multiple goblins in sequence (configurable count)
- Player HP persists between enemies (no healing!)
- Each defeated enemy spawns the next
- Must defeat all enemies to win
- Gold earned for each enemy (accumulates)
- Added RemainingEnemies property
- Tests: `MultiEnemy_Start_PlayerFaces3Enemies`, `MultiEnemy_DefeatOneEnemy_CountDecreases`, `MultiEnemy_DefeatAllEnemies_PlayerWins`, `MultiEnemy_PlayerHPPersistsBetweenFights`, `MultiEnemy_PlayerDies_GameOver`

#### Generation 7: Character Stats
- Added PlayerStrength and PlayerDefense properties
- SetPlayerStats(strength, defense) to customize character
- Strength determines damage dealt per attack
- Defense reduces incoming damage (minimum 1)
- Combat damage now shows actual damage numbers
- Stats work with all combat modes (single, loot, multi-enemy)
- Default stats: Strength 1, Defense 0 (backward compatible)
- Tests: `Stats_PlayerStartsWithDefaultStats`, `Stats_HigherStrength_DealsMoreDamage`, `Stats_Defense_ReducesDamageTaken`, `Stats_DefeatEnemyWithHighStrength_Faster`, `Stats_MultiEnemyWithStats_GoldAndHP`

#### Generation 8: Critical Hits & Misses
- Added HitType enum (Normal, Critical, Miss)
- 15% chance to miss (no damage dealt)
- 15% chance to critical hit (2x damage!)
- 70% chance for normal hit
- DetermineHitType() handles RNG
- Combat is now unpredictable - same fight can end differently
- Tests: `CriticalHit_DoesDoubleDamage`, `Miss_DealsNoDamage`, `NormalHit_DealsNormalDamage`, `BothMiss_NoDamageDealt`, `CriticalHit_RandomRNG_CanOccur`

#### Generation 9: Stamina Resource System
- Added PlayerStamina property (starts at 12)
- Attack costs 3 stamina
- Defend costs 1 stamina
- Running out of stamina forces defend action
- Can't attack if stamina < 3
- Adds strategic resource management to combat
- Must balance aggression vs conservation
- Tests: `Stamina_PlayerStartsWith12Stamina`, `Stamina_AttackCosts3Stamina`, `Stamina_DefendCosts1Stamina`, `Stamina_RunOutForcesSamina`, `Stamina_ManageResourceToWin`, `Stamina_CantGoNegative`

#### Generation 10: Enemy Variety
- Added EnemyType enum and InitializeEnemy() method
- Three distinct enemy types with different strategies:
  * Goblin Scout: 2 HP, 1 damage (quick fights)
  * Goblin Warrior: 5 HP, 1 damage (endurance test)
  * Goblin Archer: 3 HP, 2 damage (hit hard, die fast)
- Added EnemyDamage and EnemyName properties
- Random enemy selection each combat
- Each enemy type requires different tactics
- Combat log shows enemy name
- Tests: `EnemyType_GoblinScout_Has2HP`, `EnemyType_GoblinWarrior_Has5HP`, `EnemyType_GoblinArcher_Has3HP2Damage`, `EnemyType_ArcherDealsMoreDamage`, `EnemyType_WarriorTakesLongerToDefeat`, `EnemyType_RandomEncounters_AllTypesCanAppear`

#### Generation 11: Variable Enemy Stats
- InitializeEnemyWithVariableStats() adds randomness to EVERY enemy
- Enemy stat ranges:
  * Scout: 1-3 HP, 1 damage
  * Warrior: 4-6 HP, 1-2 damage
  * Archer: 2-4 HP, 1-3 damage (MAX DANGER!)
- Same enemy type = different stats each spawn
- Impossible to predict exact fight length
- Archer with 3 damage + crit = 6 damage in one hit!
- Tests: `VariableStats_ScoutHPInRange`, `VariableStats_WarriorHPAndDamageInRange`, `VariableStats_ArcherCanBeVeryDangerous`, `VariableStats_SameTypeCanHaveDifferentStats`, `VariableStats_CombatWithVariableEnemy`

#### Generation 12: Permadeath Mode (FINAL)
- Added PermanentGold and DeathCount properties
- Win combat → CommitGoldOnVictory() makes gold permanent
- Lose combat → HandlePermadeath() resets to permanent gold, increments death count
- High stakes: Current run gold at risk every fight!
- Permanent gold is safe forever
- Can chain victories to build wealth, or lose it all on one bad roll
- Tests: `Permadeath_StartsWith0PermanentGold`, `Permadeath_WinCombat_GoldBecomesPermanent`, `Permadeath_LoseCombat_GoldIsLost`, `Permadeath_MultipleWins_GoldAccumulates`, `Permadeath_DeathCounter_IncrementsOnLoss`
- **FINAL Test Count**: 62 passing

### 🎉 Original Evolution Complete! Now adding combat dynamics...

The game has evolved from "start and win" to a full RPG combat system with:
- Turn-based combat
- Character stats and customization
- Multiple enemies and strategic resource management
- Loot and progression system
- Comprehensive test coverage (35 tests, all passing)

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

// Generation 5
game.StartCombatWithLoot();
while (!game.CombatEnded)
{
    game.ExecuteLootCombatRoundWithRandomEnemy(CombatAction.Attack);
    // or for testing:
    // game.ExecuteLootCombatRound(CombatAction.Attack, CombatAction.Defend);
}

// Generation 6
game.StartMultiEnemyCombat(enemyCount: 3);
while (!game.CombatEnded)
{
    game.ExecuteMultiEnemyRoundWithRandomEnemy(CombatAction.Attack);
    // or for testing:
    // game.ExecuteMultiEnemyRound(CombatAction.Attack, CombatAction.Defend);
}

// Generation 7
game.SetPlayerStats(strength: 2, defense: 1);
game.StartCombatWithStats();
game.ExecuteStatsCombatRound(CombatAction.Attack, CombatAction.Attack);

// Or multi-enemy with stats:
game.StartMultiEnemyCombatWithStats(enemyCount: 3);
while (!game.CombatEnded)
{
    game.ExecuteStatsMultiEnemyRoundWithRandomEnemy(CombatAction.Attack);
}

// Check results
bool won = game.IsWon;
string log = game.CombatLog;
int playerHP = game.PlayerHP;
int enemyHP = game.EnemyHP;
bool ended = game.CombatEnded;
int gold = game.PlayerGold; // Persists across combats
int remaining = game.RemainingEnemies;
int strength = game.PlayerStrength;
int defense = game.PlayerDefense;
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
