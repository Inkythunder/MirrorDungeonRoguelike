using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    /// <summary>
    /// Generate a map that gives each walkable tile a step-count to reach an origin point.
    /// Instead of generating A* pathfinding for every entity on the map to find the player, a flow field
    /// is generated once for the entire map and any number of entities can navigate to the player choosing
    /// the direction with the lowest step-count.
    /// Rebuilt once per turn with the player's cell as the origin. Every enemy reads this map.
    /// </summary>
    public sealed class FlowField
    {
        public const int unreachable = int.MaxValue;
        
        readonly DungeonMap map;
        readonly int[] distance;
        private readonly Queue<Vector2Int> frontier = new Queue<Vector2Int>();

        public FlowField(DungeonMap map)
        {
            this.map = map;
            this.distance = new int[map.width * map.height];
        }

        public int DistanceAt(Vector2Int position)
        {
            if (!map.InBounds(position)) return unreachable;
            return distance[position.y * map.width + position.x];
        }

        /// <summary>
        /// Recompute every cell's step-count to the origin point (player position).
        /// Only needs to be done once per turn no matter how many enemies read the map.
        /// </summary>
        public void Rebuild(Vector2Int origin)
        {
            for (int i = 0; i < distance.Length; i++)
            {
                distance[i] = unreachable;
            }
            frontier.Clear();

            if (!map.IsWalkable(origin)) return;

            distance[origin.y * map.width + origin.x] = 0;
            frontier.Enqueue(origin);

            while (frontier.Count > 0)
            {
                Vector2Int current = frontier.Dequeue();
                int step = DistanceAt(current) + 1;

                foreach (Vector2Int direction in Direction.Cardinals4)
                {
                    Vector2Int neighbour = current + direction;

                    if (!map.IsWalkable(neighbour)) continue;
                    if (DistanceAt(neighbour) <= step) continue;

                    distance[neighbour.y * map.width + neighbour.x] = step;
                    frontier.Enqueue(neighbour);
                }
            }
        }

        /// <summary>
        /// Populates 'direction' with the direction that has the lowest steps to reach 'origin'.
        /// Returns false if the moves is blocked from reaching 'origin' or if they are already there.
        /// Other entities that are not the player block the path. The player does not block because if
        /// it did, it would be considered unreachable.
        /// </summary>
        public bool TryGetStep(
            Vector2Int from, 
            LevelData level, 
            Vector2Int origin, 
            out Vector2Int direction)
        {
            direction = Vector2Int.zero;

            int best = DistanceAt(from);
            if (best == unreachable || best == 0) return false;

            bool found = false;
            foreach (Vector2Int candidate in Direction.Cardinals4)
            {
                Vector2Int neighbour = from + candidate;

                int distance = DistanceAt(neighbour);
                if (distance >= best) continue;

                if (neighbour != origin && level.EntityAt(neighbour) != null) continue;

                best = distance;
                direction = candidate;
                found = true;
            }

            return found;
        }
    }
}
