using NUnit.Framework;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Tests
{
    public class PathfindingTests
    {
        static LevelData OpenRoom(int width, int height)
        {
            var level = new LevelData { map = new DungeonMap(width, height) };
            for (int x = 1; x < width - 1; x++)
            {
                for (int y = 1; y < height - 1; y++)
                {
                    level.map.Carve(new Vector2Int(x, y));
                }
            }

            return level;
        }

        [Test]
        public void DistanceInAnOpenRoomIsManhattenDistance()
        {
            LevelData level = OpenRoom(20, 20);
            var field = new FlowField(level.map);
            var origin = new Vector2Int(1, 1);
            field.Rebuild(origin);
            
            Assert.AreEqual(0, field.DistanceAt(origin));
            Assert.AreEqual(9 + 4, field.DistanceAt(new Vector2Int(10, 5)));
        }
        
        [Test]
        public void WallsNeverGivenDistance()
        {
            LevelData level = OpenRoom(20, 20);
            var field = new FlowField(level.map);
            field.Rebuild(new Vector2Int(1, 1));
            
            Assert.AreEqual(FlowField.unreachable, field.DistanceAt(new Vector2Int(0, 0)));
        }
        
        [Test]
        public void IsolatedSpaceIsUnreachable()
        {
            LevelData level = OpenRoom(20, 20);
            for (int i = 15; i <= 19; i++)
            {
                level.map[new Vector2Int(i, 15)] = TileType.Wall;
                level.map[new Vector2Int(15, i)] = TileType.Wall;
            }

            var field = new FlowField(level.map);
            field.Rebuild(new Vector2Int(2, 2));
            
            Assert.AreEqual(FlowField.unreachable, field.DistanceAt(new Vector2Int(17, 17)));
        }
        
        [Test]
        public void WalkingDownhillAlwaysReachesOrigin()
        {
            LevelData level = OpenRoom(20, 20);
            for (int y = 1; y < 19; y++)
            {
                if (y != 10) level.map[new Vector2Int(10, y)] = TileType.Wall;
            }

            var origin = new Vector2Int(2, 2);
            var field = new FlowField(level.map);
            field.Rebuild(origin);

            var position = new Vector2Int(18, 2);
            for (int guard = 0; guard < 500 && position != origin; guard++)
            {
                Assert.IsTrue(field.TryGetStep(position, level, origin, out Vector2Int step),
                    $"Stuck at {position}.");
                position += step;
                Assert.IsTrue(level.map.IsWalkable(position), $"Stepped into a wall at {position}.");
            }
            Assert.AreEqual(origin, position, "Never arrived.");
        }
        
        [Test]
        public void EnemyBesideOriginCanStillStepOntoIt()
        {
            LevelData level = OpenRoom(20, 20);
            var player = new EntityState
            {
                faction = Faction.Player,
                position = new Vector2Int(5, 1)
            };
            level.AddEntity(player);
            level.player = player;

            var field = new FlowField(level.map);
            field.Rebuild(player.position);
            
            Assert.IsTrue(field.TryGetStep(new Vector2Int(4, 1), level, player.position, out Vector2Int step));
            Assert.AreEqual(Direction.Right, step);
        }
    }
}

