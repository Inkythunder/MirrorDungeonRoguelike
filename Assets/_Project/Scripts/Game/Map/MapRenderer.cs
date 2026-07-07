using Roguelike.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.Tilemaps;

namespace Roguelike.Game
{
    public class MapRenderer : MonoBehaviour
    {
        [SerializeField] Tilemap floor_tilemap;
        [SerializeField] Tilemap wall_tilemap;
        [SerializeField] TileBase floor_tile;
        [SerializeField] TileBase wall_tile;

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
                        wallTiles[index] = wall_tile;
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
        
        public void Clear()
        {
            floor_tilemap.ClearAllTiles();
            wall_tilemap.ClearAllTiles();
        }
    }
}
