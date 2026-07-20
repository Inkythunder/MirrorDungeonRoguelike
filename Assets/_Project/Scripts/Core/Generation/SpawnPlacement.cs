using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    public static class SpawnPlacement
    {
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
                int index = rng.Range(0, candidates.Count);
                chosen.Add(candidates[index]);
                candidates.RemoveAt(index);
            }

            return chosen;
        }
    }
}