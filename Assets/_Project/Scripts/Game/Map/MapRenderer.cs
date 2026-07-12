using Roguelike.Core;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace Roguelike.Game
{
    public class MapRenderer : MonoBehaviour
    {
        [SerializeField] Tilemap floor_tilemap;
        [SerializeField] Tilemap wall_tilemap;
        [SerializeField] TileBase floor_tile;
        [SerializeField] TileBase wall_face_tile;
        [SerializeField] TileBase wall_side_left_tile;
        [SerializeField] TileBase wall_side_right_tile;
        [SerializeField] TileBase wall_corner_UL;
        [SerializeField] TileBase wall_corner_DL;
        [SerializeField] TileBase wall_corner_DR;
        [SerializeField] TileBase wall_corner_UR;
        [SerializeField] TileBase wall_top_tile;

        public void Render(DungeonMap map)
        {
            Clear();

            var bounds = new BoundsInt(0, 0, 0, map.width, map.height, 1);
            var floorTiles = new TileBase[map.width * map.height];
            var wallTiles = new TileBase[map.width * map.height];

            for (int y = 0; y < map.height; y++)
            {
                for (int x = 0; x < map.width; x++)
                {
                    int index = y * map.width + x;
                    var position = new Vector2Int(x, y);

                    if (map[position] == TileType.Floor)
                    {
                        floorTiles[index] = floor_tile;
                    }
                    else if (HasFloorNeighbour(map, position))
                    {
                        wallTiles[index] = PickWallTile(map, position);
                    }
                }
            }
            
            floor_tilemap.SetTilesBlock(bounds, floorTiles);
            wall_tilemap.SetTilesBlock(bounds, wallTiles);
        }

        /// <summary>
        /// Only draws wall tiles that actually touch the open cave space.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        static bool HasFloorNeighbour(DungeonMap map, Vector2Int position)
        {
            foreach (Vector2Int direction in Direction.Cardinals8)
            {
                if (map[position + direction] == TileType.Floor) return true;
            }

            return false;
        }

        /// <summary>
        /// Chooses which wall sprite a solid cell uses.
        /// </summary>
        /// <param name="map"></param>
        /// <param name="position"></param>
        /// <returns></returns>
        TileBase PickWallTile(DungeonMap map, Vector2Int position)
        {
            if (map[position + Direction.Up] == TileType.Wall &&
                map[position + Direction.Down] == TileType.Wall &&
                map[position + Direction.Left] == TileType.Floor &&
                map[position + Direction.Right] == TileType.Wall) return wall_side_left_tile;
            
            if (map[position + Direction.Up] == TileType.Wall &&
                map[position + Direction.Down] == TileType.Wall &&
                map[position + Direction.Left] == TileType.Wall &&
                map[position + Direction.Right] == TileType.Floor) return wall_side_right_tile;
            
            if (map[position + Direction.Up] == TileType.Floor &&
                map[position + Direction.Down] == TileType.Wall &&
                map[position + Direction.Left] == TileType.Floor &&
                map[position + Direction.Right] == TileType.Wall) return wall_corner_DR;
            
            if (map[position + Direction.Up] == TileType.Floor &&
                map[position + Direction.Down] == TileType.Wall &&
                map[position + Direction.Left] == TileType.Wall &&
                map[position + Direction.Right] == TileType.Floor) return wall_corner_DL;
            
            return wall_face_tile;
        }
        
        public void Clear()
        {
            floor_tilemap.ClearAllTiles();
            wall_tilemap.ClearAllTiles();
        }
    }
}
