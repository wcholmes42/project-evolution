using System.Text.Json.Serialization;

namespace ProjectEvolution.Game;

/// <summary>
/// Centralized configuration for ALL tunable game parameters
/// This is the SINGLE source of truth for progression tuning
/// </summary>
public class GameConfig
{
    // ═══════════════════════════════════════════════════════════
    // PLAYER PROGRESSION
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("player_base_hp")]
    public int PlayerBaseHP { get; set; } = 20;
    
    [JsonPropertyName("player_hp_per_level")]
    public double PlayerHPPerLevel { get; set; } = 3.0;
    
    [JsonPropertyName("player_base_str")]
    public int PlayerBaseSTR { get; set; } = 3;
    
    [JsonPropertyName("player_base_def")]
    public int PlayerBaseDEF { get; set; } = 1;
    
    [JsonPropertyName("player_stat_points_per_level")]
    public int PlayerStatPointsPerLevel { get; set; } = 2;
    
    // ═══════════════════════════════════════════════════════════
    // ENEMY SCALING
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("enemy_base_hp")]
    public int EnemyBaseHP { get; set; } = 5;
    
    [JsonPropertyName("enemy_hp_scaling")]
    public double EnemyHPScaling { get; set; } = 1.5;
    
    [JsonPropertyName("enemy_base_damage")]
    public int EnemyBaseDamage { get; set; } = 2;
    
    [JsonPropertyName("enemy_damage_scaling")]
    public double EnemyDamageScaling { get; set; } = 0.4;
    
    // ═══════════════════════════════════════════════════════════
    // ECONOMY
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("base_gold_per_combat")]
    public int BaseGoldPerCombat { get; set; } = 10;
    
    [JsonPropertyName("gold_scaling")]
    public double GoldScaling { get; set; } = 3.5;
    
    [JsonPropertyName("potion_cost")]
    public int PotionCost { get; set; } = 5;
    
    [JsonPropertyName("inn_cost")]
    public int InnCost { get; set; } = 10;
    
    // ═══════════════════════════════════════════════════════════
    // LOOT
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("base_treasure_gold")]
    public int BaseTreasureGold { get; set; } = 25;
    
    [JsonPropertyName("treasure_per_dungeon_depth")]
    public int TreasurePerDungeonDepth { get; set; } = 30;
    
    [JsonPropertyName("equipment_drop_rate")]
    public double EquipmentDropRate { get; set; } = 15.0;
    
    // ═══════════════════════════════════════════════════════════
    // EQUIPMENT TIERS
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("weapon_tier_power_multiplier")]
    public double WeaponTierPowerMultiplier { get; set; } = 1.5;
    
    [JsonPropertyName("armor_tier_power_multiplier")]
    public double ArmorTierPowerMultiplier { get; set; } = 1.4;
    
    // ═══════════════════════════════════════════════════════════
    // COMBAT TUNING
    // ═══════════════════════════════════════════════════════════
    
    [JsonPropertyName("player_crit_chance")]
    public double PlayerCritChance { get; set; } = 0.15;
    
    [JsonPropertyName("player_crit_multiplier")]
    public double PlayerCritMultiplier { get; set; } = 2.0;
    
    [JsonPropertyName("defend_damage_reduction")]
    public double DefendDamageReduction { get; set; } = 0.5;
    
    [JsonPropertyName("flee_base_chance")]
    public double FleeBaseChance { get; set; } = 0.6;
    
    // ═══════════════════════════════════════════════════════════
    // HELPERS
    // ═══════════════════════════════════════════════════════════
    
    /// <summary>
    /// Create config from progression framework (for tuner compatibility)
    /// </summary>
    public static GameConfig FromFramework(ProgressionFrameworkData framework)
    {
        return new GameConfig
        {
            PlayerBaseHP = framework.PlayerProgression.BaseHP,
            PlayerHPPerLevel = framework.PlayerProgression.HPPerLevel,
            PlayerBaseSTR = framework.PlayerProgression.BaseSTR,
            PlayerBaseDEF = framework.PlayerProgression.BaseDEF,
            PlayerStatPointsPerLevel = framework.PlayerProgression.StatPointsPerLevel,
            
            EnemyBaseHP = framework.EnemyProgression.BaseHP,
            EnemyHPScaling = framework.EnemyProgression.HPScalingCoefficient,
            EnemyBaseDamage = framework.EnemyProgression.BaseDamage,
            EnemyDamageScaling = framework.EnemyProgression.DamageScalingCoefficient,
            
            BaseGoldPerCombat = framework.Economy.BaseGoldPerCombat,
            GoldScaling = framework.Economy.GoldScalingCoefficient,
            
            BaseTreasureGold = framework.Loot.BaseTreasureGold,
            TreasurePerDungeonDepth = framework.Loot.TreasurePerDungeonDepth,
            EquipmentDropRate = framework.Loot.EquipmentDropRate
        };
    }
    
    /// <summary>
    /// Create default balanced config
    /// </summary>
    public static GameConfig CreateDefault()
    {
        return new GameConfig();  // Uses property initializers above
    }
    
    /// <summary>
    /// Validate config (ensure no broken values)
    /// </summary>
    public bool IsValid(out string? error)
    {
        if (PlayerBaseHP < 10 || PlayerBaseHP > 100)
        {
            error = "PlayerBaseHP must be 10-100";
            return false;
        }
        
        if (EnemyHPScaling < 0.5 || EnemyHPScaling > 5.0)
        {
            error = "EnemyHPScaling must be 0.5-5.0";
            return false;
        }
        
        if (GoldScaling < 1.0 || GoldScaling > 10.0)
        {
            error = "GoldScaling must be 1.0-10.0";
            return false;
        }
        
        error = null;
        return true;
    }
}
