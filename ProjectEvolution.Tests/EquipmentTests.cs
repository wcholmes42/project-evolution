using Xunit;
using ProjectEvolution.Game;
using System.Collections.Generic;

namespace ProjectEvolution.Tests
{
    public class EquipmentTests
    {
        [Fact]
        public void Equipment_Increases_EffectiveStats()
        {
            var game = new RPGGame();
            game.SetPlayerStats(strength: 5, defense: 3);
            
            // Default equipment is Rusty Dagger (0) and Cloth Rags (0)
            Assert.Equal(5, game.GetEffectiveStrength());
            Assert.Equal(3, game.GetEffectiveDefense());

            // Equip better items manually for test
            game.PlayerInventory.EquippedWeapon = new Weapon("Test Sword", 5, 100);
            game.PlayerInventory.EquippedArmor = new Armor("Test Plate", 4, 100);

            Assert.Equal(10, game.GetEffectiveStrength()); // 5 + 5
            Assert.Equal(7, game.GetEffectiveDefense());   // 3 + 4
        }

        [Fact]
        public void Combat_Uses_EquipmentBonus()
        {
            var game = new RPGGame();
            game.SetPlayerStats(strength: 2, defense: 0);
            
            // Equip weapon with +3 STR (Total 5)
            game.PlayerInventory.EquippedWeapon = new Weapon("Strong Sword", 3, 100);
            
            game.StartCombatWithStats();
            // Enemy has 3 HP. With 5 damage, it should be a one-shot.
            
            game.ExecuteStatsCombatRound(CombatAction.Attack, CombatAction.Attack);
            
            Assert.True(game.IsWon);
            Assert.True(game.CombatEnded);
            Assert.Contains("5 damage", game.CombatLog);
        }

        [Fact]
        public void Shop_CanBuyWeapon()
        {
            var game = new RPGGame();
            game.SetGoldForTesting(100);
            
            // Equip a non-default weapon first so we can verify it gets saved
            var oldWeapon = new Weapon("Old Sword", 1, 10);
            game.PlayerInventory.EquippedWeapon = oldWeapon;
            
            var newWeapon = new Weapon("Shop Sword", 2, 50);
            bool success = game.BuyWeapon(newWeapon);

            Assert.True(success);
            Assert.Equal(50, game.PlayerGold); // 100 - 50
            Assert.Equal(newWeapon, game.PlayerInventory.EquippedWeapon);
            Assert.Contains(oldWeapon, game.PlayerInventory.Weapons); // Old weapon moved to inventory
        }

        [Fact]
        public void Shop_CannotBuy_TooExpensive()
        {
            var game = new RPGGame();
            game.SetGoldForTesting(10);
            
            var expensiveWeapon = new Weapon("God Sword", 10, 1000);
            bool success = game.BuyWeapon(expensiveWeapon);

            Assert.False(success);
            Assert.Equal(10, game.PlayerGold);
            Assert.NotEqual(expensiveWeapon, game.PlayerInventory.EquippedWeapon);
        }
    }
}
