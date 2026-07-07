using System.Collections.Generic;
using UnityEngine;

namespace Roguelike.Core
{
    
    public sealed class Room
    {
        public RectInt bounds;
        public Vector2Int centre;
        public List<Vector2Int> floor_tiles = new List<Vector2Int>();
    }

    public sealed class LevelData
    {
        public DungeonMap map;
        public BspNode bsp_root;
        public List<Room> rooms = new List<Room>();

        public Vector2Int player_spawn;
        public Vector2Int stairs_position;

        public int depth;
        public int seed;
    }
}