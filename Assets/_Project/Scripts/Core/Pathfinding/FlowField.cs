using System.Collections.Generic;
using UnityEditor.TerrainTools;
using UnityEngine;

namespace Roguelike.Core
{
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
