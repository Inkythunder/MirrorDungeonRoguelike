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
        [SerializeField] TileBase wall_face;
        [SerializeField] TileBase wall_side_left;
        [SerializeField] TileBase wall_side_right;
        [SerializeField] TileBase wall_corner_UL;
        [SerializeField] TileBase wall_corner_DL;
        [SerializeField] TileBase wall_corner_DR;
        [SerializeField] TileBase wall_corner_UR;

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
            TileType up = map[position + Direction.Up];
            TileType down = map[position + Direction.Down];
            TileType left = map[position + Direction.Left];
            TileType right = map[position + Direction.Right];
            TileType up_right = map[position + Direction.Up_Right];
            TileType down_right = map[position + Direction.Down_Right];
            TileType down_left = map[position + Direction.Down_Left];
            TileType up_left = map[position + Direction.Up_Left];

            if (down == TileType.Floor) return wall_face;
            
            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Floor &&
                right == TileType.Wall) return wall_side_left;
            
            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Floor) return wall_side_right;
            
            if (up == TileType.Floor &&
                down == TileType.Wall &&
                left == TileType.Floor &&
                right == TileType.Wall) return wall_corner_DR;
            
            if (up == TileType.Floor &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Floor) return wall_corner_DL;

            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Wall &&
                up_right == TileType.Floor &&
                down_right == TileType.Wall &&
                down_left == TileType.Wall &&
                up_left == TileType.Wall) return wall_corner_UR;
            
            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Wall &&
                up_right == TileType.Wall &&
                down_right == TileType.Wall &&
                down_left == TileType.Wall &&
                up_left == TileType.Floor) return wall_corner_UL;
            
            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Wall &&
                up_right == TileType.Wall &&
                down_right == TileType.Floor &&
                down_left == TileType.Wall &&
                up_left == TileType.Wall) return wall_side_right;
            
            if (up == TileType.Wall &&
                down == TileType.Wall &&
                left == TileType.Wall &&
                right == TileType.Wall &&
                up_right == TileType.Wall &&
                down_right == TileType.Wall &&
                down_left == TileType.Floor &&
                up_left == TileType.Wall) return wall_side_left;
            
            return wall_face;
        }
        
        public void Clear()
        {
            floor_tilemap.ClearAllTiles();
            wall_tilemap.ClearAllTiles();
        }
    }
}
