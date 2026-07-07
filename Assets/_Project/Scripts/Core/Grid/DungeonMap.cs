using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    public sealed class DungeonMap
    {
        public int width { get; }
        public int height { get; }

        private readonly TileType[] _tiles;

        public DungeonMap(int width, int height)
        {
            this.width = width;
            this.height = height;
            _tiles = new TileType[width * height];
        }

        public bool InBounds(Vector2Int p)
        {
            return p.x >= 0 && 
                   p.y >= 0 && 
                   p.x < width && 
                   p.y < height;
        }

        /// <summary>
        /// Indexer to allow accessing TileType with square brackets.
        /// </summary>
        /// <param name="p"></param>
        public TileType this[Vector2Int p]
        {
            get
            {
                if (this.InBounds(p))
                {
                    return _tiles[p.y * width + p.x];
                }
                return TileType.Wall;
            }
            set
            {
                if (InBounds(p))
                {
                    _tiles[p.y * width + p.x] = value;
                }
                else
                {
                    Debug.LogWarning("Tile provided is out of bounds.");
                }
            }
        }

        public bool IsWalkable(Vector2Int p) => this[p] == TileType.Floor;

        public void Carve(Vector2Int p) => this[p] = TileType.Floor;

        public List<Vector2Int> FloorTiles()
        {
            var result = new List<Vector2Int>();
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    var p = new Vector2Int(x, y);
                    if (this[p] == TileType.Floor)
                    {
                        result.Add(p);
                    }
                }
            }

            return result;
        }

        /// <summary>
        /// Breadth-first flood fill test to check connectivity.
        /// </summary>
        /// <param name="start"></param>
        /// <returns></returns>
        public HashSet<Vector2Int> ReachableFrom(Vector2Int start)
        {
            var visited_tiles = new HashSet<Vector2Int>();
            if (!IsWalkable(start))
            {
                return visited_tiles;
            }

            var queue = new Queue<Vector2Int>();
            queue.Enqueue(start);
            visited_tiles.Add(start);

            while (queue.Count > 0)
            {
                var current_tile = queue.Dequeue();
                foreach (var direction in Direction.Cardinals4)
                {
                    var next_tile = current_tile + direction;
                    if (visited_tiles.Contains(next_tile)) continue;
                    if(!IsWalkable(next_tile)) continue;
                    visited_tiles.Add(next_tile);
                    queue.Enqueue(next_tile);
                }
            }

            return visited_tiles;
        }
    }
}
