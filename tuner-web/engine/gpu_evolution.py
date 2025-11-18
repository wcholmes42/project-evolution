"""
GPU-accelerated evolution engine that uses REAL C# game logic
- PyTorch for parallel candidate generation on GPU
- Subprocess pool for parallel C# game evaluation
- Hardware-aware throttling
"""
import torch
import asyncio
import subprocess
import json
import numpy as np
from dataclasses import dataclass
from typing import List, Dict
from pathlib import Path
import multiprocessing as mp


@dataclass
class FrameworkCandidate:
    """12-parameter progression framework"""
    base_hp: int
    hp_per_level: float
    base_str: int
    base_def: int
    stat_points_per_level: int
    enemy_base_hp: int
    enemy_hp_scaling: float
    enemy_base_damage: int
    enemy_damage_scaling: float
    base_gold: int
    gold_scaling: float
    equipment_drop_rate: float
    
    def to_dict(self) -> Dict:
        return {
            "PlayerProgression": {
                "BaseHP": self.base_hp,
                "HPPerLevel": self.hp_per_level,
                "BaseSTR": self.base_str,
                "BaseDEF": self.base_def,
                "StatPointsPerLevel": self.stat_points_per_level
            },
            "EnemyProgression": {
                "BaseHP": self.enemy_base_hp,
                "HPScalingCoefficient": self.enemy_hp_scaling,
                "BaseDamage": self.enemy_base_damage,
                "DamageScalingCoefficient": self.enemy_damage_scaling
            },
            "Economy": {
                "BaseGoldPerCombat": self.base_gold,
                "GoldScalingCoefficient": self.gold_scaling
            },
            "Loot": {
                "BaseTreasureGold": 25,
                "TreasurePerDungeonDepth": 30,
                "EquipmentDropRate": self.equipment_drop_rate
            }
        }


class GPUEvolutionEngine:
    """Hybrid evolution: GPU for mutations, C# for fitness"""
    
    def __init__(self, game_dll="../ProjectEvolution.Game/bin/Release/net9.0/ProjectEvolution.Game.dll"):
        self.game_dll = Path(game_dll)
        self.device = torch.device("cuda" if torch.cuda.is_available() else "cpu")
        self.population_size = 100 if torch.cuda.is_available() else 20
        self.max_parallel = mp.cpu_count()
        
        self.population = []
        self.generation = 0
        self.best_fitness = 0.0
        self.best_framework = None
        self.running = False
        self.paused = False
        self.throttle = 100
        
        self.stats = {
            "generation": 0,
            "best_fitness": 0.0,
            "population_size": self.population_size,
            "device": str(self.device)
        }
    
    def get_stats(self) -> Dict:
        return self.stats.copy()
    
    def stop(self):
        self.running = False
    
    def pause(self):
        self.paused = not self.paused
    
    def set_throttle(self, percentage: int):
        self.throttle = max(0, min(100, percentage))
    
    def auto_throttle(self, level: int):
        self.set_throttle(level)
