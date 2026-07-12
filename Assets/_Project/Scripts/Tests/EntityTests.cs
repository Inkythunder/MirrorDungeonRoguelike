using NUnit.Framework;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Tests
{
    public class EntityTests
    {
        static LevelData MakeLevel()
        {
            var level = new LevelData { map = new DungeonMap(10, 10) };
            for (int x = 1; x < 9; x++)
            {
                for (int y = 1; y < 9; y++)
                {
                    level.map.Carve(new Vector2Int(x, y));
                }
            }

            return level;
        }

        [Test]
        public void MovingAnEntityKeepsOccupancyIndex()
        {
            LevelData level = MakeLevel();
            var entity = new EntityState { position = new Vector2Int(2, 2) };
            level.AddEntity(entity);
            
            level.MoveEntity(entity, new Vector2Int(3, 2));
            
            Assert.IsNull(level.EntityAt(new Vector2Int(2, 2)), "Old cell still occupied.");
            Assert.AreSame(entity, level.EntityAt(new Vector2Int(3, 2)));
        }

        [Test]
        public void BlockedMoveDoesNotIncrementTurnCountOrMoveEntity()
        {
            LevelData level = MakeLevel();
            var player = new EntityState
            {
                faction = Faction.Player,
                position = new Vector2Int(1, 1)
            };
            level.AddEntity(player);

            var action = new MoveOrAttackAction(player, Direction.Left);
            ActionResult result = action.Perform(level);
            
            Assert.AreEqual(ActionResult.Blocked, result);
            Assert.AreEqual(new Vector2Int(1, 1), player.position);
        }

        [Test]
        public void MovingIntoHostileAttacksInsteadOfMoves()
        {
            LevelData level = MakeLevel();
            var player = new EntityState
            {
                faction = Faction.Player,
                position = new Vector2Int(2, 2), 
                attack = 5
            };
            var enemy = new EntityState
            {
                faction = Faction.Hostile,
                position = new Vector2Int(3, 2), hp = 10, 
                defence = 1
            };
            level.AddEntity(player);
            level.AddEntity(enemy);

            var action = new MoveOrAttackAction(player, Direction.Right);
            ActionResult result = action.Perform(level);
            
            Assert.AreEqual(ActionResult.Attacked, result);
            Assert.AreEqual(new Vector2Int(2, 2), player.position, "Player should not have moved.");
            Assert.AreEqual(6, enemy.hp, "5 attack - 1 defence = 4 damage.");
        }
    }
}

