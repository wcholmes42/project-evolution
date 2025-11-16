# Project Evolution

An RPG game built through Test-Driven Development (TDD), evolving from the simplest possible game to Ultima IV-level gameplay.

## Philosophy

This project follows a strict TDD evolution approach:
1. Write a failing test
2. Make it pass with the simplest implementation
3. Add a new feature through a new test
4. Repeat

## Current State - Generation 8

RNG-Based Combat - Every Fight is Different:
- **15% Miss Chance** - Attacks can whiff completely!
- **15% Critical Hit** - 2x damage when RNG blesses you!
- **70% Normal Hits** - Standard damage
- **True Unpredictability**: Same fight, different outcome every time
- **High Risk**: Miss twice? You might die. Crit twice? Easy victory!
- The game is no longer a foregone conclusion!

## Running the Game

```bash
dotnet run --project ProjectEvolution.Game
```

## Running Tests

```bash
dotnet test
```

## Tech Stack

- C# / .NET 9.0
- xUnit for testing
- Console-based interface

## Evolution Goal

To reach Ultima IV level of:
- Gameplay depth (virtues, quests, exploration)
- Graphics (ASCII art evolving to tile-based)
- Engagement (story, character development, combat)

## Evolution History

### Generation 8 (Current)
- [x] Critical hit system (15% chance, 2x damage)
- [x] Miss system (15% chance, no damage)
- [x] RNG makes combat unpredictable
- [x] HitType enum for combat outcomes
- [x] Same fight can go completely different ways
- [x] **40 tests, all passing**

### Generation 7
- [x] Character stats: Strength and Defense
- [x] Customizable character with SetPlayerStats()
- [x] Damage dealt = Strength value
- [x] Damage taken reduced by Defense (min 1)
- [x] Combat log shows actual damage numbers
- [x] Stats integrate with all combat systems
- [x] **35 tests, all passing**

### Generation 6
- [x] Multiple enemies in sequence (3 goblins)
- [x] HP persists between enemy encounters
- [x] Strategic resource management across battles
- [x] Each enemy defeated spawns next
- [x] Gold earned per enemy (30g total for 3 goblins)
- [x] Must survive all encounters to win

### Generation 5
- [x] Loot system: 10 gold per defeated goblin
- [x] Gold accumulates across combats
- [x] Combat log shows gold rewards
- [x] No gold awarded on defeat
- [x] First persistent player progression

### Generation 4
- [x] Health point system (Player: 10 HP, Enemy: 3 HP)
- [x] Multi-round turn-based combat
- [x] Attack deals 1 damage when not blocked
- [x] Defend blocks all incoming damage
- [x] Combat continues until someone dies
- [x] Victory/defeat based on HP reaching 0

### Generation 3
- [x] Enemy AI fights back
- [x] Rock-paper-scissors combat resolution
- [x] Attack vs Attack = 50/50 chance
- [x] Attack vs Defend = Attacker wins
- [x] Defend vs Defend = Draw (player loses)
- [x] Combat log for narrative feedback

### Generation 2
- [x] Combat encounter with a weak goblin
- [x] Player choice: Attack or Defend
- [x] Attack = win, Defend = lose
- [x] First player agency in the game

### Generation 1
- [x] Random coin flip determines outcome
- [x] Can win or lose

### Generation 0
- [x] Game starts
- [x] Game is immediately won

## Next Evolution

TBD - What feature should we add next?
