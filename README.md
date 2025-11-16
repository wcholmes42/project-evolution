# Project Evolution

An RPG game built through Test-Driven Development (TDD), evolving from the simplest possible game to Ultima IV-level gameplay.

## Philosophy

This project follows a strict TDD evolution approach:
1. Write a failing test
2. Make it pass with the simplest implementation
3. Add a new feature through a new test
4. Repeat

## Current State - Generation 3

Combat with enemy AI:
- Face a goblin warrior with AI
- Enemy makes random choices (Attack/Defend)
- Rock-paper-scissors combat system
- Your choice vs enemy choice determines outcome

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

### Generation 3 (Current)
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
