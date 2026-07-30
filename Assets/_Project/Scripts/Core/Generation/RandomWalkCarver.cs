using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    public static class RandomWalkCarver
    {

        /// <summary>
        /// Carves an organic cave shape for each room.
        /// Fewer iterations and longer walk_length creates longer, narrower caves.
        /// More iterations and shorter walk_length creates rounder shaped caves.
        /// </summary>
        /// <returns></returns>
        public static HashSet<Vector2Int> Carve
        (RectInt room, int iterations, int walk_length, float minimum_fill, Rng rng)
        {
            var floor = new HashSet<Vector2Int>();
            var start = new Vector2Int(room.xMin + room.width / 2, room.yMin + room.height / 2);

            floor.Add(start);

            int target_tiles = Mathf.RoundToInt(room.width * room.height * minimum_fill);
            int number_of_walks_limit = iterations * 10;
            int number_of_walks = 0;
            
            // Keep running walks for 'interation' number of times and met the minimum fill requirement
            while ((number_of_walks < iterations || floor.Count < target_tiles) &&
                   number_of_walks < number_of_walks_limit)
            {
                number_of_walks++;

                var current_tile = start;
                for (int step = 0; step < walk_length; step++)
                {
                    var next_tile = current_tile + Direction.Cardinals4[rng.Range(0, 4)];
                    
                    if (!room.Contains(next_tile)) continue;

                    current_tile = next_tile;
                    floor.Add(current_tile);
                }
            }

            return floor;
        }
    }
}