# Comprehensive Fitness Evaluation System

## Overview
The fitness system has been completely refactored from a binary pass/fail checklist to a comprehensive, granular, extensible metric-based architecture.

## What Changed

### Before (Problems)
- ❌ Binary scoring (pass/fail = 100 or 0)
- ❌ Only tested 3 levels (1, 3, 5)
- ❌ No time-to-kill measurement
- ❌ No difficulty curve analysis
- ❌ All builds scored identically (100%)
- ❌ 916,000 generations stuck at 100% with no improvement

### After (Solutions)
- ✅ Granular 0-100 continuous scoring
- ✅ Tests ALL levels 1-10
- ✅ Measures time-to-kill (3-7 turn target)
- ✅ Detects difficulty spikes
- ✅ Validates equipment power curves
- ✅ Measures build differentiation
- ✅ Adaptive mutation (explores when stuck, fine-tunes when improving)

## Metric Architecture

### Current Metrics (100% Total Weight)

1. **CombatBalanceMetric (30%)**
   - Win rate across all levels 1-10
   - Time-to-kill analysis (target: 3-7 turns)
   - Combat variance/consistency
   - Detects: boring 1-shot kills, grindy 20-turn slogs

2. **EconomicHealthMetric (25%)**
   - Affordability at each level
   - Economic surplus (20-50% ideal)
   - CRITICAL: Instant failure if broken economy
   - Detects: inflation, starvation, progression blockers

3. **EquipmentCurveMetric (15%)**
   - Power increase per tier (15-50% ideal)
   - Cost scaling validation (1.5-5x per tier)
   - Detects: weak upgrades, P2W power spikes

4. **DifficultyPacingMetric (15%)**
   - Smoothness (no >30% spikes)
   - Gradual increase trend
   - Detects: difficulty walls, unbalanced progression

5. **BuildDiversityMetric (15%)**
   - Viability count (all 3 builds should work)
   - Differentiation (builds should feel different, not identical)
   - Target variance: 10-20 points

## How to Add New Metrics

### Step 1: Create Your Metric Class
```csharp
public class YourNewMetric : IFitnessMetric
{
    public string Name => "Your Metric Name";
    public double Weight => 0.10; // 10% of total fitness

    public MetricResult Evaluate(ProgressionFrameworkData framework)
    {
        var result = new MetricResult { MetricName = Name };

        // Your evaluation logic here
        double score = 0; // 0-100

        // Add details for logging
        result.Details.Add("Something measured: X");

        // Add warnings for problems
        if (somethingBad)
            result.Warnings.Add("Warning message");

        // Set critical flag for instant failures
        result.Critical = true; // optional

        result.Score = score;
        result.WeightedScore = score * Weight;
        return result;
    }
}
```

### Step 2: Register in FitnessEvaluator
```csharp
private static readonly List<IFitnessMetric> _metrics = new()
{
    new CombatBalanceMetric(),      // 30%
    new EconomicHealthMetric(),     // 25%
    new EquipmentCurveMetric(),     // 15%
    new DifficultyPacingMetric(),   // 15%
    new BuildDiversityMetric(),     // 15%
    new YourNewMetric()             // 10% <- ADD HERE
    // Total should = 100% (adjust other weights as needed)
};
```

That's it! The metric automatically integrates into:
- Fitness evaluation
- Detailed logging
- Dashboard reporting

## Adaptive Mutation Strategy

The system now adapts based on progress:

**Stuck (100+ gens no improvement)**
- Mutation rate: 70%
- Mutation strength: 2.0x
- Strategy: Aggressive exploration

**Getting stuck (50+ gens)**
- Mutation rate: 50%
- Mutation strength: 1.5x
- Strategy: Moderate exploration

**Improving (< 10 gens)**
- Mutation rate: 20%
- Mutation strength: 0.5x
- Strategy: Fine-tuning

**Default**
- Mutation rate: 30%
- Mutation strength: 1.0x
- Strategy: Balanced

## Expected Improvements

1. **No more 100% plateau** - Granular scoring means continuous optimization
2. **Catches more issues** - Tests all levels, not just 1,3,5
3. **Better player experience** - Optimizes for fun (TTK, pacing, progression feel)
4. **Easy extensibility** - Add dungeon metrics, skill tree metrics, etc.

## Future Metric Ideas

When new features are added, consider these metrics:

- **DungeonBalanceMetric** - Multi-floor difficulty, loot distribution
- **SkillTreeMetric** - Build synergies, power spike validation
- **DeathPenaltyMetric** - Is dying too punishing or too forgiving?
- **GrindingMetric** - Time investment vs reward feel
- **NewPlayerExperienceMetric** - First 30 minutes feel
- **EndgameMetric** - Level 10+ content sustainability

## Testing the New System

Run the tuning and watch for:
1. Fitness scores now in 40-90 range (not stuck at 100)
2. Detailed metric breakdowns in logs every 50 generations
3. Adaptive mutation messages when stuck
4. More varied parameter exploration

## Performance Notes

- Each metric runs per generation (~1-2 seconds total)
- CombatBalanceMetric runs 20 simulations × 10 levels = 200 combats
- Detailed reports logged only on improvements or every 50 gens
- System designed for continuous overnight runs
