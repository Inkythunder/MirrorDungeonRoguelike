using NUnit.Framework;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Tests
{
    public class ItemTests
    {
        static LevelData Level()
        {
            var level = new LevelData
            {
                map = new DungeonMap(10, 10)
            };
            for (int x = 1; x < 9; x++)
            {
                for (int y = 0; y < 9; y++)
                {
                    level.map.Carve(new Vector2Int(x, y));
                }
            }

            var player = new EntityState
            {
                name = "Player",
                faction = Faction.Player,
                position = new Vector2Int(2, 2),
                max_hp = 20,
                hp = 20,
                attack = 4,
                defence = 1
            };
            level.AddEntity(player);
            level.player = player;
            return level;
        }

        static void StepRight(LevelData level) => new MoveOrAttackAction(level.player, Direction.Right).Perform(level);

        [Test]
        public void WalkingOverBetterWeaponEquipsIt()
        {
            LevelData level = Level();
            level.AddItem(new Vector2Int(3, 2),
                new ItemState { name = "Rusty Sword", type = ItemType.Weapon, tier = 2 });
            
            StepRight(level);
            
            Assert.AreEqual("Rusty Sword", level.player.weapon_name);
            Assert.AreEqual(2, level.player.weapon_power);
            Assert.AreEqual(4 + 2, level.player.total_attack);
            Assert.IsNull(level.ItemAt(new Vector2Int(3, 2)), "Equipped weapon should leave floor." );
        }

        [Test]
        public void PotionsStack()
        {
            LevelData level = Level();
            level.AddItem(new Vector2Int(3, 2), new ItemState{name = "Potion", type = ItemType.Potion});
            
            StepRight(level);
            
            Assert.AreEqual(1, level.player.potions);
            Assert.IsNull(level.ItemAt(new Vector2Int(3, 2)), "Picked up potion should leave floor.");
        }
        
        [Test]
        public void DrinkingHealsAndConsumesPotion()
        {
            LevelData level = Level();
            level.player.potions = 2;
            level.player.hp = 5;

            ActionResult result = new DrinkPotionAction(level.player).Perform(level);
            
            Assert.AreNotEqual(ActionResult.Blocked, result);
            Assert.AreEqual(5 + DrinkPotionAction.heal_amount, level.player.hp);
            Assert.AreEqual(1, level.player.potions);
        }
        
        [Test]
        public void WalkingOverWorseWeaponLeavesIt()
        {
            LevelData level = Level();
            level.player.weapon_name = "Sword of Epicness";
            level.player.weapon_power = 99;
            level.AddItem(new Vector2Int(3, 2),
                new ItemState { name = "Sword of Lameness", type = ItemType.Weapon, tier = 2 });
            
            StepRight(level);
            
            Assert.AreEqual("Sword of Epicness", level.player.weapon_name);
            Assert.AreEqual(99, level.player.weapon_power);
            Assert.AreEqual(4 + 99, level.player.total_attack);
            Assert.IsNotNull(level.ItemAt(new Vector2Int(3, 2)), "Worse weapon should stay on floor." );
        }
        
        [Test]
        public void HealingNeverHealsMoreThanMaxHp()
        {
            LevelData level = Level();
            level.AddItem(new Vector2Int(3, 2), new ItemState{name = "Potion", type = ItemType.Potion});
            level.player.hp = level.player.max_hp - 1;
            level.player.potions = 1;
            
            ActionResult result = new DrinkPotionAction(level.player).Perform(level);
            
            Assert.AreNotEqual(ActionResult.Blocked, result);
            Assert.AreEqual(level.player.hp, level.player.max_hp, "Potions should not heal more than max_hp");
        }
        
        [Test]
        public void DrinkingWithNoPotionsIsBlocked()
        {
            LevelData level = Level();
            level.player.potions = 0;
            level.player.hp = 5;

            ActionResult result = new DrinkPotionAction(level.player).Perform(level);
            
            Assert.AreEqual(ActionResult.Blocked, result, 
                "Player should not be able to drink if they have no potions");
            Assert.AreEqual(5, level.player.hp);
            Assert.AreEqual(0, level.player.potions);
        }
        
        [Test]
        public void DrinkingWithFullHealthIsBlocked()
        {
            LevelData level = Level();
            level.player.potions = 1;
            level.player.hp = level.player.max_hp;

            ActionResult result = new DrinkPotionAction(level.player).Perform(level);
            
            Assert.AreEqual(ActionResult.Blocked, result, 
                "Player should not be able to drink if they have full health");
            Assert.AreEqual(level.player.max_hp, level.player.hp);
            Assert.AreEqual(1, level.player.potions);
        }
        
        [Test]
        public void MinimumDamageIsAlwaysApplied()
        {
            LevelData level = Level();
            var enemy = new EntityState
            {
                name = "Big dude",
                faction = Faction.Hostile,
                position = new Vector2Int(3, 2),
                max_hp = 999,
                hp = 100,
                attack = 1,
                defence = 99
            };
            level.AddEntity(enemy);
            
            StepRight(level);
            
            Assert.AreEqual(enemy.hp, 100, "Enemy hp should not change with absurdly high defence");
        }
    }
}
