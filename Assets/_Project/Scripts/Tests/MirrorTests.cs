using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Roguelike.Core;
using UnityEngine;

namespace Roguelike.Tests
{
    public class MirrorTests
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
        public void PetrifyAndRestoreRoundTrip()
        {
            LevelData level = MakeLevel();

            var player = new EntityState
            {
                name = "Player",
                faction = Faction.Player,
                position = new Vector2Int(1, 1)
            };
            level.player = player;
            level.primary.AddEntity(player);
            
            var thug_a = new EntityState
            {
                name = "Thug A",
                faction = Faction.Hostile,
                position = new Vector2Int(3, 3)
            };
            var thug_b = new EntityState
            {
                name = "Thug B",
                faction = Faction.Hostile,
                position = new Vector2Int(4, 3)
            };
            // Guardians start as statues and come alive in the mirror world.
            var guardian = new EntityState
            {
                name = "Mirror Guardina",
                faction = Faction.Petrified,
                is_guardian = true,
                position = new Vector2Int(5, 3)
            };

            var spawned = new[] { thug_a, thug_b, guardian };
            var start_positions = new Dictionary<EntityState, Vector2Int>();
            var start_factions = new Dictionary<EntityState, Faction>();

            foreach (EntityState entity in spawned)
            {
                level.primary.AddEntity(entity);
                start_positions[entity] = entity.position;
                start_factions[entity] = entity.faction;
            }

            var toggle = new ToggleWorldAction(player);

            // Ten round trips to make results more obvious.
            for (int trip = 0; trip < 10; trip++)
            {
                Assert.AreEqual(ActionResult.Moved, toggle.Perform(level), $"Entering mirror world failed on trip {trip}.");
                Assert.IsTrue(level.in_mirror_world, $"Should be in the mirror world on trip {trip}");
                
                Assert.AreEqual(ActionResult.Moved, toggle.Perform(level), $"Leaving mirror world failed on trip {trip}.");
                Assert.IsFalse(level.in_mirror_world, $"Should be back in primary world on trip {trip}.");
            }
            
            Assert.IsEmpty(level.mirror.AllEntities, "Mirror world still holds entities after returning.");
            
            // Player plus 3 spawned entities only.
            Assert.AreEqual(4, level.primary.AllEntities.Count, "Primary world has the wrong number of entities.");

            foreach (EntityState entity in spawned)
            {
                Assert.IsTrue(level.primary.AllEntities.Contains(entity), $"{entity.name} did not come back to the primary world.");
                Assert.AreEqual(start_positions[entity], entity.position, $"{entity.name} moved during the round trip.");
                Assert.AreEqual(start_factions[entity], entity.faction, $"{entity.name} came back with the wrong faction.");
                Assert.AreSame(entity, level.primary.EntityAt(entity.position), $"{entity.name} is not indexed at its own cell.");
            }
        }
    }
}