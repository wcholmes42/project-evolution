# Project Evolution - Session Notes

**Last Updated**: 2025-11-16
**Current Generation**: 33 (ML Optimization Complete!) 🎛️
**Status**: ✅ 9-Mode Self-Tuning System + 2M+ Games Tested!

## 🎮 PLAYABLE DEFAULTS (Human-Friendly!)
- **HP: 15** (forgiving, not brutal!)
- **Strength: 3** (kill mobs faster)
- **MaxMobs: 15** (half density, more exploration)
- **Detection: 2** (easier escapes)
- **Dungeons: Visible, less traps** (15% chance, 1-3 dmg)

## 🤖 AI-DISCOVERED OPTIMAL SETTINGS

After **11,692 cycles** and **233,840 simulated games**, the continuous tuner found:
- **Max Mobs: 29** (↑ from 20) - Players handle higher density!
- **Starting HP: 9** (↓ from 10) - Counter-intuitive but perfect tension!
- **Mob Detection: 3** (optimal chase/escape balance)
- **Result: 54.6 avg turns, 90.8/100 balance score** ✅

These are now the game defaults!

## 📝 TODO for Next Session

**Bug Fixes Completed:**
- ✅ Combat infinite loops (50-round limit)
- ✅ Enum parsing crashes (all mob spawns use EnemyType)
- ✅ Debug panel alignment
- ✅ ESC kills all test cycles
- ✅ Mob encounter triggers in simulation
- ✅ Dungeon entry and navigation

**Auto-Tuner System:**
- ✅ 10-cycle automated testing
- ✅ Self-adjusting parameters
- ✅ Best config recommendation

**Future Enhancements:**
- Equipment system (weapons, armor)
- Skills/abilities
- Different mob types beyond goblins
- Boss encounters
- Quest system

## 🏆 LINE IN THE SAND - What We've Built

**Git Tags**:
  - v1.0-roguelike-complete (Gen 0-12: Combat Dynamics)
  - v2.0-progression-complete (Gen 13-17: Character Progression)
  - v2.1-endless-mode (Gen 18: Game Loop)
  - v3.0-world-exploration (Gen 19-25: Warhammer Quest Fusion)
  - v3.1-demoscene-ui (Gen 26: Ultimate ASCII UI)
  - v3.2-logging (Gen 27: Comprehensive Logging)
  - v3.3-combat-overhaul (Gen 28: Combat Fixes)
  - v3.4-pacing (Gen 29: Game Feel & Pacing)
  - v3.5-turn-based (Gen 30: Turn-Based World)
  - v4.0-ai-fog-dungeons (Gen 31: AI, Fog, Dungeons)
  - v4.1-balanced-populations (Gen 32: Game of Life Mobs)
  - v4.2-simulation-tuning (Gen 33: Automated Testing) ⭐ **CURRENT**

**Test Coverage**: 137 passing tests (100% coverage maintained)
**From "you win" to production roguelike**: 26 generations!

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

#### Generation 12: Permadeath Mode
- Permadeath, permanent gold, death counter
- **Test Count**: 62 passing

### 📈 Progression Cohort (Gen 13-17)

#### Generation 13: Experience & Leveling
- PlayerXP, PlayerLevel, XPForNextLevel
- Earn 10 XP per enemy, level at 100/200/300...
- **Test Count**: 67 passing

#### Generation 14: Stat Points
- Gain 2 stat points per level
- SpendStatPoint(STR/DEF) for customization
- **Test Count**: 72 passing

#### Generation 15: Enemy XP Variety
- Scout:10xp, Warrior:25xp, Archer:15xp
- Risk vs reward balanced
- **Test Count**: 76 passing

#### Generation 16: Enemy Level Scaling
- Enemies have levels (EnemyLevel property)
- Scale with player level (±1)
- +2 HP, +1 damage per level
- **Test Count**: 78 passing

#### Generation 17: Max HP Growth
- MaxPlayerHP increases +2 per level
- HP fully restored on level up

#### Generation 18: Endless Game Loop 🏁
- Continuous play mode until death
- Continue or quit after victories
- **Test Count**: 84 passing

### 🗺️ World Exploration Cohort (Gen 19-25)

#### Generation 19: Grid-Based World Map
- 20x20 procedurally generated world
- PlayerX, PlayerY with N/S/E/W movement
- Terrain types: Grassland, Forest, Mountain
- Special locations: Towns & Dungeons
- Bounds checking
- **Test Count**: 92 passing

#### Generation 20: Location System
- EnterLocation() for Towns & Dungeons
- ExitLocation() back to world
- InLocation and CurrentLocation properties
- **Test Count**: 96 passing

#### Generation 21: Random Encounter Tables 🎲
- Warhammer Quest style encounter rolling
- Forest: 40% | Mountain: 30% | Grassland: 20%
- Towns: 0% (safe zones)
- **Test Count**: 100 passing

#### Generation 22: Town Services 🏪
- Inn & Shop services
- Potions for healing
- **Test Count**: 104 passing

#### Generation 23: Dungeon Generation ⚔️
- Depth tracking, descend mechanics
- RollForRoom() tables (50% Empty, 35% Monster, 15% Treasure)
- **Test Count**: 109 passing

#### Generation 24: Loot Tables 💎
- RollForTreasure(depth) - depth-scaled gold rewards
- Base: 10-30 gold + (depth * 10)
- Depth 5 = 60-80 gold!
- **Test Count**: 111 passing

#### Generation 25: Event System ⚡
- RollForEvent() - 60% Nothing, 25% Trap, 15% Discovery
- TriggerTrap() - 1-5 damage
- TriggerDiscovery() - bonus gold or XP
- **Test Count**: 114 passing

#### Generation 26: Demoscene ASCII UI 🎨
- UIRenderer with static layout
- Box drawing, ANSI colors, map view
- **Test Count**: 114 passing

#### Generation 27: Comprehensive Logging 📊
- GameLogger class, state dumps, event tracking
- **Test Count**: 114 passing

#### Generation 28: Combat Overhaul ⚔️
- FIXED: StartWorldExploration() now initializes HP!
- Encounters have infinite stamina (999)
- AttemptFlee() - 50% escape or take damage
- RenderCombat() - "across the table" side-by-side display
- Clear visual combat feedback
- [F] Flee option in all combats
- No more stamina trap!
- **Test Count**: 114 passing ✅

#### Generation 29: Game Pacing & Feel ⏱️
- Strategic delays throughout gameplay for better readability
- Combat rounds pause 900ms so player can read results
- Encounters pause 800ms to build tension
- Victory pause 1000ms, death pause 1500ms (dramatic!)
- Dungeon events (traps, treasure) have appropriate pauses
- FIXED: Map view restored after combat (ui.RenderMap after victory)
- Town/potion actions have brief pauses for feedback
- No code changes needed - game already uses ReadKey (no Enter required!)
- **Test Count**: 114 passing ✅

#### Generation 30: Turn-Based World System (CURRENT) 🕐
- WorldTurn counter tracks game time
- Terrain movement costs implemented:
  - Grassland/Town: 1 turn (fast)
  - Forest: 2 turns (difficult terrain!)
  - Mountain: 3 turns (very slow, treacherous!)
- Strategic terrain choices matter now!
- Mob system with visible enemies on map ('M' in red)
- 10-15 mobs spawn across world at start
- Mobs don't spawn in towns or on player
- Mobs visible in 3-tile radius (tactical awareness!)
- Turn counter displayed in status bar
- Movement feedback shows terrain difficulty
- Mob class: X, Y, Name, Level properties
- **Integrated mob encounters**: Walking into 'M' triggers combat!
- Defeated mobs removed from map (permanent progress!)
- Random encounters drastically reduced (5-15% vs old 20-40%)
- Random encounters are now "ambushes" from hidden enemies
- Mobs are the PRIMARY encounter system now!
- FIXED: Treasure test ranges (depth bonus was miscalculated)
- **Test Count**: 124 passing ✅

#### Generation 31: AI, Fog of War & Dungeon Maps (CURRENT) 🗺️
- **Mob AI System**: Mobs hunt the player!
  - Detection range: 5 tiles (Manhattan distance)
  - Mobs move toward player each turn
  - Simple pathfinding (try horizontal, then vertical)
  - Mobs outside range stay dormant
- **Fog of War**: Exploration-based visibility
  - Only explored tiles shown on map ('?' for unexplored)
  - Player vision range: 3 tiles
  - Explored tiles stay revealed permanently
  - Strategic reconnaissance required!
- **Real Dungeon Maps**: Procedurally generated interiors
  - 30x30 dungeon grid with rooms & corridors
  - 5-8 rooms per dungeon level
  - Rooms connected by L-shaped corridors
  - Wall collision detection
  - Can't move through walls
  - Larger viewport (7 tiles) in dungeons
  - Persistent dungeon layouts per level
- **UI Overhaul for Dungeons**:
  - Dual rendering: world map vs dungeon map
  - Dungeon uses compact ASCII (@ . █)
  - World map shows fog of war
  - Updated legends for each mode
- **Test Count**: 134 passing ✅ (+10 new tests!)

#### Generation 32: Game of Life Population Control (CURRENT) ⚖️
- **BALANCE PATCH**: Mobs were too aggressive!
- **Reduced detection range**: 5 tiles → 3 tiles (less relentless pursuit)
- **Random despawn**: 2% chance per tick (mobs can wander off)
- **Game of Life dynamics**:
  - Overpopulation (4+ neighbors): 30% despawn chance (flee crowding)
  - Isolation (0 neighbors): 10% despawn chance (no pack support)
  - Sweet spot (2-3 neighbors): 5% spawn chance (reproduction!)
  - CountNearbyMobs(4-tile radius) determines population pressure
- **Population bounds**:
  - Minimum: 5 mobs (always some threats)
  - Maximum: 20 mobs (prevent overwhelming swarms)
  - Auto-spawn when below minimum
  - Reproduction respects maximum cap
- **Emergent behavior**:
  - Mobs cluster in "packs" (2-3 creates stable populations)
  - Overcrowded areas self-regulate
  - Isolated mobs tend to die off
  - Dynamic, living ecosystem!
- **Test Count**: 137 passing ✅ (+3 new tests!)

#### Generation 33: Automated Testing & Tuning System (CURRENT) 🎛️
- **Interactive Simulation Mode**: Watch the game play itself!
- **AutoPlayer AI**: Makes strategic decisions
  - Flees from mobs when low HP
  - Visits inns when hurt and can afford it
  - Buys potions strategically
  - Random exploration otherwise
  - Combat AI: attack when strong, defend when weak
  - Uses potions intelligently
- **GameSimulator**: Runs automated game sessions
  - Configurable number of runs (1-100)
  - Collects comprehensive statistics
  - Visual or headless mode
- **SimulationStats**: Tracks everything
  - Total runs, deaths, turns survived
  - Average turns per run
  - Max turns survived
  - Combats won, gold earned
  - Death reasons and patterns
- **The Fun Knobs** 🎛️ (All tunable!):
  - Mob Detection Range (1-10 tiles)
  - Max/Min Mob populations
  - Player starting HP
  - Player strength/defense
  - Encounter rate multiplier (0-500%)
  - Simulation speed (0-1000ms per turn)
  - Visual toggle (watch or batch run)
- **Balance Analysis**:
  - Automatic difficulty assessment
  - "Too Hard" warning if avg < 20 turns
  - "Too Easy" warning if avg > 200 turns
  - Recommendations for tuning
- **Main Menu**: Choose Play vs Simulation mode at launch
- **BONUS FIX**: Dungeon lockup issue resolved!
  - Added ui.RenderMap() on dungeon entry
  - Replaced old [R]oll system with movement exploration
  - Dungeons now fully functional
- **UPDATED**: Goal-driven aggressive AI!
  - Tiered goals: Level 5, 500g, 2 dungeons
  - Default behavior: HUNTING (not passive exploring)
  - Risk/reward evaluation for every mob
  - Actively seeks combat for progression
  - LastDecision tracking shows reasoning
  - CurrentTarget shows what AI is pursuing
- **Debug Panel**: Live AI thinking display
  - Shows current goal, objective, target
  - Displays reasoning for decisions
  - Progress bars toward goals
  - Session stats (turns, combats, fled)
  - ESC key to abort simulation anytime!
- **Test Count**: 137 passing ✅

### 🎉 Now we can SCIENCE the fun! Data-driven game design!

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
