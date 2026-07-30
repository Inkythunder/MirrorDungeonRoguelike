using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    public static class SpawnPlacement
    {
        /// <summary>
        /// Picks 'count' number of distinct free floor tiles on the map.
        /// Picked tiles must be a minimum distance from the spawn otherwise they could spawn right next
        /// to the player which would not be fun.
        /// </summary>
        public static List<Vector2Int> ChooseTiles(
            LevelData level, 
            int count, 
            int min_distance_from_player, 
            Rng rng)
        {
            var candidates = new List<Vector2Int>();

            foreach (Room room in level.rooms)
            {
                foreach (Vector2Int tile in room.floor_tiles)
                {
                    if (!level.CanEnter(tile)) continue;
                    if (tile == level.stairs_position) continue;

                    int distance = Mathf.Abs(tile.x - level.player_spawn.x) +
                                   Mathf.Abs(tile.y - level.player_spawn.y);
                    if (distance < min_distance_from_player) continue;
                    
                    candidates.Add(tile);
                }
            }

            var chosen = new List<Vector2Int>();
            for (int i = 0; i < count; i++)
            {
                // If 0, there are no legal tiles to spawn on.
                if (candidates.Count == 0) break;
                
                int index = rng.Range(0, candidates.Count);
                chosen.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return chosen;
        }
    }
}