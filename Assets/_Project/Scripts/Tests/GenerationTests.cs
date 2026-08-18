using System.Collections.Generic;
using NUnit.Framework;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Tests
{
    public class GenerationTests
    {
        // How many seeds we will test.
        private const int seed_count = 200;

        static GenerationSettings DefaultSettings() => new GenerationSettings();

        [Test]
        public void EveryFloorTileReachableFromSpawn()
        {
            var settings = DefaultSettings();

            for (int seed = 0; seed <= seed_count; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);
                
                HashSet<Vector2Int> reachable = level.map.ReachableFrom(level.player_spawn);
                List<Vector2Int> allFloor = level.map.FloorTiles();
                
                Assert.AreEqual(allFloor.Count, reachable.Count, 
                    $"Seed {seed}: {allFloor.Count - reachable.Count} floor tiles are unreachable.");
            }
        }

        [Test]
        public void SameSeedProducesIdenticalMap()
        {
            var settings = DefaultSettings();

            for (int seed = 1; seed <= 20; seed++)
            {
                LevelData a = MapGenerator.Generate(settings, seed, 1);
                LevelData b = MapGenerator.Generate(settings, seed, 1);

                for (int y = 0; y < a.map.height; y++)
                {
                    for (int x = 0; x < a.map.width; x++)
                    {
                        var point = new Vector2Int(x, y);
                        Assert.AreEqual(a.map[point], b.map[point], $"Seed {seed} differ at {point}.");
                    }
                    
                    Assert.AreEqual(a.player_spawn, b.player_spawn);
                    Assert.AreEqual(a.stairs_position, b.stairs_position);
                }
            }
        }

        [Test]
        public void DifferentSeedsProduceDifferentMaps()
        {
            var settings = DefaultSettings();
            LevelData a = MapGenerator.Generate(settings, 1, 1);
            LevelData b = MapGenerator.Generate(settings, 2, 1);

            bool difference = false;
            for (int y = 0; y < a.map.height && !difference; y++)
            {
                for (int x = 0; x < a.map.width; x++)
                {
                    var point = new Vector2Int(x, y);
                    if (a.map[point] != b.map[point])
                    {
                        difference = true;
                        break;
                    }
                }
            }
            Assert.IsTrue(difference, "Two different seeds produced the same map.");
        }

        [Test]
        public void BspLeavesAreInsideRootAndDoNotOverlap()
        {
            var settings = DefaultSettings();

            for (int seed = 1; seed <= seed_count; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);
                List<BspNode> leaves = level.bsp_root.Leaves();
                RectInt root = level.bsp_root.bounds;

                foreach (BspNode leaf in leaves)
                {
                    Assert.IsTrue(leaf.bounds.xMin >= root.xMin && 
                                  leaf.bounds.yMin >= root.yMin && 
                                  leaf.bounds.xMax <= root.xMax && 
                                  leaf.bounds.yMax <= root.yMax);
                    Assert.GreaterOrEqual(leaf.bounds.width, settings.minimum_partition_size);
                    Assert.GreaterOrEqual(leaf.bounds.height, settings.minimum_partition_size);
                }

                for (int i = 0; i < leaves.Count; i++)
                {
                    for (int j = i + 1; j < leaves.Count; j++)
                    {
                        Assert.IsFalse(leaves[i].bounds.Overlaps(leaves[j].bounds), 
                            $"Seed {seed}: leaves {leaves[i].bounds} and {leaves[j].bounds} overlap.");
                    }
                }
            }
        }

        [Test]
        public void SpawnAndStairsAreOnFloorAndDistinct()
        {
            var settings = DefaultSettings();
            for (int seed = 1; seed <= seed_count; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);
                
                Assert.IsTrue(level.map.IsWalkable(level.player_spawn),
                    $"Seed {seed}: spawn is inside a wall.");
                Assert.IsTrue(level.map.IsWalkable(level.stairs_position),
                    $"Seed {seed}: stairs are inside a wall.");
                Assert.AreNotEqual(level.player_spawn, level.stairs_position,
                    $"Seed {seed}: stairs are on the spawn.");
            }
        }

        [Test]
        public void TheMapBorderIsAlwaysSolid()
        {
            var settings = DefaultSettings();

            for (int seed = 1; seed <= 50; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);
                DungeonMap map = level.map;

                for (int x = 0; x < map.width; x++)
                {
                    Assert.AreEqual(TileType.Wall, map[new Vector2Int(x, 0)]);
                    Assert.AreEqual(TileType.Wall, map[new Vector2Int(x, map.width - 1)]);
                }
                for (int y = 0; y < map.width; y++)
                {
                    Assert.AreEqual(TileType.Wall, map[new Vector2Int(0, y)]);
                    Assert.AreEqual(TileType.Wall, map[new Vector2Int(map.width - 1, y)]);
                }
            }
        }

        [Test]
        public void CarvedRoomTilesStayInsideTheirRoom()
        {
            var settings = new GenerationSettings();

            for (int seed = 1; seed <= 100; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);

                foreach (Room room in level.rooms)
                {
                    foreach (Vector2Int tile in room.floor_tiles)
                    {
                        Assert.IsTrue(room.bounds.Contains(tile), $"Seed {seed}: tile {tile} is outside {room.bounds}.");
                    }
                }
            }
        }

        [Test]
        public void DepthsDifferButStayDeterministic()
        {
            var settings = new GenerationSettings();

            LevelData depth1 = MapGenerator.Generate(settings, 123, 1);
            LevelData depth2 = MapGenerator.Generate(settings, 123, 2);
            LevelData depth2_again = MapGenerator.Generate(settings, 123, 2);
            
            CollectionAssert.AreEqual(depth2.map.FloorTiles(), depth2_again.map.FloorTiles(),
                "Same seed and depth must produce identical maps");
            CollectionAssert.AreNotEqual(depth1.map.FloorTiles(), depth2.map.FloorTiles(),
                "Depth 2 produced depth 1's layout - depth isn't reaching Rng.");
        }

        [Test]
        public void StairsAreAlwaysReachableFromSpawn()
        {
            var settings = new GenerationSettings();
            for (int seed = 1; seed <= 200; seed++)
            {
                LevelData level = MapGenerator.Generate(settings, seed, 1);
                Assert.IsTrue(level.map.ReachableFrom(level.player_spawn).Contains(level.stairs_position),
                    $"Seed {seed}: stairs are inaccessible from spawn.");
            }
        }

        [Test]
        public void SpawnPlacementSurvivesRunningOutOfTiles()
        {
            LevelData level = MapGenerator.Generate(new GenerationSettings(), 7, 1);

            List<Vector2Int> tiles = SpawnPlacement.ChooseTiles(level, 10_000, 0, new Rng(7));
            
            Assert.LessOrEqual(tiles.Count, level.map.FloorTiles().Count,
                "Asking for too many tiles should return fewer, not throw.");
        }
    }    
}
