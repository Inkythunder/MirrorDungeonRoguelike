using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Guardians start as statues in both worlds but the longer the player spends in the mirror world,
    /// the more guardians wake up. They have an aggro range covering the whole map so they always
    /// hunt the player.
    /// </summary>
    public class Guardians
    {
        /// <summary>
        /// Called once per turn. Does nothing while player is in primary world.
        /// </summary>
        public static void Tick(LevelData level)
        {
            if (!level.in_mirror_world) return;
            if (level.turns_per_awakening <= 0) return;

            level.mirror_turns++;

            // How many guardians should have woken by now. Because 'mirror_turns' survives the trip back
            // to the primary world. Re-entering late in the level wakes several at once instead of from 0.
            int target_awake = level.mirror_turns / level.turns_per_awakening;

            while (level.guardians_awakened < target_awake)
            {
                // Stop early if every guardian on the level is already awake or dead.
                if (!AwakenNearest(level)) break;
                level.guardians_awakened++;
            }
        }

        /// <summary>
        /// Wakes the guardian nearest to the player.
        /// </summary>
        static bool AwakenNearest(LevelData level)
        {
            EntityState player = level.player;
            EntityState nearest = null;
            int shortest_distance = int.MaxValue;

            foreach (EntityState entity in level.mirror.AllEntities)
            {
                if (!entity.is_guardian || !entity.IsAlive) continue;
                if (entity.faction != Faction.Petrified) continue;

                int distance = Mathf.Abs(entity.position.x - player.position.x) +
                               Mathf.Abs(entity.position.y - player.position.y);

                if (distance >= shortest_distance) continue;

                shortest_distance = distance;
                nearest = entity;
            }

            if (nearest == null) return false;

            nearest.faction = Faction.Hostile;
            level.Log("You hear the cracking of stone...");
            return true;
        }
    }
}